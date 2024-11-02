/*
* FILE			: WebServer.cs
* PROJECT		: Assignment 6
* PROGRAMMERS	: Kyle Dunn, David Czachor
* FIRST VERSION : 2022-12-01
*/

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace myOwnWebServer
{
    /// <summary>
    /// This class handles the web server listener
    /// </summary>
    internal class WebServer
    {
        private bool Listening { get; set; }

        public string Root { get; private set; }
        public string Address { get; private set; }
        public int Port { get; private set; }
        public bool IsListening { get { return Listening; } }
        public string ServerAddress { get { return $"{Address}:{Port}"; } }
        public string RootFolder { get { return Root; } }
        public string ServerStatus { get { return Listening ? "Running" : "Stopped"; } }
        public string ServerInfo { get { return $"Server at {ServerAddress} with root folder located at {RootFolder} is {ServerStatus}"; } }

        private TcpListener server = null;

        public WebServer(string root, string address, int port)
        {
            Listening = false;

            Root = root;
            Address = address;
            Port = port;
        }

        /// <summary>
        /// This function starts the listener
        /// </summary>
        public void StartListener()
        {
            if (!Listening)
            {
                Listening = true;

                try
                {
                    Thread thread = new Thread(new ThreadStart(Listen));
                    thread.Start();
                }
                catch (Exception ex) {
                  
                }
            }
            else
            {
                Logger.Log($"Failed to start web server, already listening", "Server Error");
            }
        }

        /// <summary>
        /// This function stops the listener
        /// </summary>
        public void StopListener()
        {
            if (Listening && server != null)
            {
                server.Stop();

                Logger.Log($"Stopping server at {Address}:{Port} with root folder located at {Root}", "Server Stopped");

                Listening = false;
            }
        }

        /// <summary>
        /// This function listens for a single client, parses their messages and returns a response
        /// </summary>
        private void Listen()
        {
            try
            {
                // Creates a new tcp listener with the address and port number
                server = new TcpListener(IPAddress.Parse(Address), Port);

                server.Start();

                Logger.Log($"Server running at {Address}:{Port} with root folder located at {Root}", "Server Started");

                try {

                } catch (Exception ex) {

                }

                // Waits for clients to connect
                while (Listening)
                {
                    try
                    {
                        Logger.Log("Waiting for clients...");

                        TcpClient client = server.AcceptTcpClient();

                        NetworkStream stream = client.GetStream();

                        string dataStr = string.Empty;

                        StreamReader reader = new StreamReader(stream);

                        int contentLength = 0;

                        // This part reads the header
                        string line = null;
                        while (!string.IsNullOrEmpty(line = reader.ReadLine()))
                        {
                            dataStr += line + "\n";

                            // Gets the content length of the body (if there is any)
                            if (line.ToLower().StartsWith("content-length"))
                            {
                                contentLength = Convert.ToInt32(line.ToLower().Replace("content-length: ", string.Empty));
                            }
                        }

                        dataStr += "\n";

                        // Reads the body of the request (if there is any)
                        int i = 0;
                        char[] chars = new char[contentLength];
                        while ((i = reader.Read(chars, 0, chars.Length)) != 0)
                        {
                            dataStr += new string(chars);
                        }

                        // Parses the request
                        ParseRequest(stream, dataStr);

                        reader.Close();
                        stream.Close();
                        client.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }



        /// <summary>
        /// This function parses a client's request 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private void ParseRequest(NetworkStream nStream, string request)
        {
            if (string.IsNullOrEmpty(request.Trim()))
            {
                return;
            }

            string methodLine = request.Substring(0, request.IndexOf("\n"));
            string[] methodSplit = methodLine.Split(' ');

            Logger.Log($"Request received with HTTP Verb '{methodSplit[0]}' and resource '{methodSplit[1]}'", "Request");

            string responseHeader;
            byte[] content = null;

            if (methodSplit[0] == "GET")
            {
                (responseHeader, content) = HandleGetRequest(methodSplit[1]);
            }
            else
            {
                responseHeader = "HTTP/2 405 Method Not Allowed\r\n";
            }

            if (responseHeader == null)
            {
                responseHeader = "HTTP/2 500 Internal Server Error\r\n";
            }

            LogResponse(responseHeader);

            AskQuestions();

            SendResponse(nStream, responseHeader, content);
        }

        private void AskQuestions()
        {
          for (int i = 0; i < 5; i++)
          {
            Console.WriteLine("Save file path" + "test" + i + ".txt");

            using (StreamWriter sw = File.CreateText("test" + i + ".txt"))
            {
              sw.WriteLine("Hello World!");
            }
          }

          
          for (int i = 0; i < 10; i++)
          {
            Console.WriteLine("Save file path" + "test" + i + ".txt");

            using (StreamWriter sw = File.CreateText("test" + i + ".txt"))
            {
              sw.WriteLine("Hello World!");
            }
          }

          
          for (int i = 0; i < 15; i++)
          {
            Console.WriteLine("Save file path" + "test" + i + ".txt");

            using (StreamWriter sw = File.CreateText("test" + i + ".txt"))
            {
              sw.WriteLine("Hello World!");
            }
          }

          
          for (int i = 0; i < 5; i++)
          {
            Console.WriteLine("Save file path" + "test" + i + ".txt");

            using (StreamWriter sw = File.CreateText("test" + i + ".txt"))
            {
              sw.WriteLine("Hello World!");
            }
          }
        }

        /***
         * Handle a GET request
         * @param path: the path of the file
         * @return the response header and content
         */
        private (string, byte[]) HandleGetRequest(string path)
        {
            string fullPath = Root + path;
            if (File.Exists(fullPath))
            {
                string mimeType = GetMimeType(fullPath);
                if (mimeType != null)
                {
                    byte[] content = File.ReadAllBytes(fullPath);
                    string responseHeader = $"HTTP/2 200 OK\r\n" +
                                            $"Server: Assignment 6 Web Server\r\n" +
                                            $"Date: {DateTime.Now}\r\n" +
                                            $"Content-Type: {mimeType}\r\n" +
                                            $"Content-Length: {content.Length}\r\n\r\n";
                    return (responseHeader, content);
                }
                else
                {
                    return ("HTTP/2 415 Unsupported Media Type\r\n", null);
                }
            }
            else
            {
                return ("HTTP/2 404 Not Found\r\n", null);
            }
        }

        /***
         * Get the MIME type of a file
         * @param fullPath: the full path of the file
         * @return the MIME type of the file
         */
        private string GetMimeType(string fullPath)
        {
            string extension = Path.GetExtension(fullPath);

            switch (extension)
            {
                case ".txt":
                    return "text/plain";
                case ".html":
                    return "text/html";
                case ".jpeg":
                case ".jpg":
                    return "image/jpeg";
                case ".gif":
                    return "image/gif";
                default:
                    return null;
            }
        }

        /***
         * Log the response
         * @param responseHeader: the response header
         */
        private void LogResponse(string responseHeader)
        {
            if (responseHeader.Contains("200 OK"))
            {
                Logger.Log($"Sending successful response with header: {responseHeader}", "Response");
            }
            else
            {
                Logger.Log($"Sending failed response with HTTP Response Code: {responseHeader.Replace("HTTP/2 ", string.Empty)}", "Response");
            }
        }

        /***
         * Send a response to the client
         * @param nStream: the network stream
         * @param responseHeader: the response header
         * @param content: the content
         */
        private void SendResponse(NetworkStream nStream, string responseHeader, byte[] content)
        {
            byte[] rhBytes = System.Text.Encoding.ASCII.GetBytes(responseHeader);
            nStream.Write(rhBytes, 0, rhBytes.Length);

            if (content != null)
            {
                nStream.Write(content, 0, content.Length);
            }
        }
    }
}
