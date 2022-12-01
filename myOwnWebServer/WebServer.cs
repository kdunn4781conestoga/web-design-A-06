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
                Console.WriteLine("Failed to start web server, already listening");
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

                // Waits for clients to connect
                while (true)
                {
                    try
                    {
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
                        string response = ParseRequest(dataStr);

                        byte[] bytes = System.Text.Encoding.ASCII.GetBytes(response);

                        stream.Write(bytes, 0, bytes.Length);

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
        private string ParseRequest(string request)
        {
            string response = "<body><h1>Hello World!</h1></body>";

            return $"HTTP/2 200 OK\r\nContent-Length:{response.Length}\r\n\r\n{response}"; // temp
        }
    }
}
