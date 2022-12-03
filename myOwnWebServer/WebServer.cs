/*
* FILE			: WebServer.cs
* PROJECT		: Assignment 6
* PROGRAMMERS	: Kyle Dunn, David Czachor
* FIRST VERSION : 2022-12-01
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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
                catch { }
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

                // Waits for clients to connect
                while (Listening)
                {
                    try
                    {
                        Logger.Log("Waiting for clients...");

                        TcpClient client = server.AcceptTcpClient();

                        NetworkStream stream = client.GetStream();

                        string dataStr = "";

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
                                contentLength = Convert.ToInt32(line.ToLower().Replace("content-length: ", ""));
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
            // Sample request
            //
            // GET /sample.txt HTTP/1.1
            // HOST: localhost

            string responseHeader = null;
            byte[] content = null;

            if (!string.IsNullOrEmpty(request.Trim()))
            {

                string methodLine = request.Substring(0, request.IndexOf("\n"));
                string[] methodSplit = methodLine.Split(' ');

                Logger.Log($"Request received with HTTP Verb '{methodSplit[0]}' and resource '{methodSplit[1]}'", "Request");

                // Only GET method calls are valid
                if (methodSplit[0] == "GET")
                {
                    string path = methodSplit[1];

                    string fullPath = Root + path;

                    // Checks if the path exists
                    if (File.Exists(fullPath))
                    {
                        // Determine the mime type for the path
                        string mimeType = null;
                        string extension = Path.GetExtension(fullPath);
                        switch (extension)
                        {
                            case ".txt":
                                mimeType = "text/plain";
                                break;
                            case ".html":
                                mimeType = "text/html";
                                break;
                            case ".jpeg":
                            case ".jpg":
                                mimeType = "image/jpeg";
                                break;
                            case ".gif":
                                mimeType = "image/gif";
                                break;
                        }

                        if (mimeType != null) // If mime type is supported
                        {
                            content = File.ReadAllBytes(fullPath);

                            responseHeader = $"HTTP/2 200 OK\r\n" +
                                                    $"Server: Assignment 6 Web Server\r\n" +
                                                    $"Date: {DateTime.Now}\r\n" +
                                                    $"Content-Type: {mimeType}\r\n" +
                                                    $"Content-Length: {content.Length}\r\n\r\n";
                        }
                        else // If mime type isn't supported
                        {
                            responseHeader = "HTTP/2 415 Unsupported Media Type\r\n";
                        }
                    }
                    else // If the path doesn't exist
                    {
                        responseHeader = "HTTP/2 404 Not Found\r\n";
                    }
                }
                else // If user tries to use anything but GET
                {
                    responseHeader = "HTTP/2 405 Method Not Allowed\r\n";
                }

                if (responseHeader == null)
                {
                    responseHeader = "HTTP/2 500 Internal Server Error\r\n";
                }

                if (responseHeader.Contains("200 OK"))
                {
                    Logger.Log($"Sending successful response with header: {responseHeader}", "Response");
                }
                else
                {
                    Logger.Log($"Sending failed response with HTTP Response Code: {responseHeader.Replace("HTTP/2 ", "")}", "Response");
                }

                // Sends the response header to the client
                byte[] rhBytes = System.Text.Encoding.ASCII.GetBytes(responseHeader);

                nStream.Write(rhBytes, 0, rhBytes.Length);

                if (content != null) // If there is content send it to the client
                {
                    nStream.Write(content, 0, content.Length);
                }
            }
        }
    }
}
