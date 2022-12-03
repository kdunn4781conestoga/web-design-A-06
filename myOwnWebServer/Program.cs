/*
* FILE			: Program.cs
* PROJECT		: Assignment 6
* PROGRAMMERS	: Kyle Dunn, David Czachor
* FIRST VERSION : 2022-12-01
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace myOwnWebServer
{
    /// <summary>
    /// The main class of the web server
    /// </summary>
    internal class Program
    {
        static void Main(string[] args)
        {
            // Checks if number of arguments is 3
            if (args.Length == 3)
            {
                string root = args[0].Replace("-webRoot=", "");
                string address = args[1].Replace("-webIP=", "");
                string port = args[2].Replace("-webPort=", "");

                // Checks if all of the arguments are valid
                if (!string.IsNullOrEmpty(root)
                    && !string.IsNullOrEmpty(address) 
                    && !string.IsNullOrEmpty(port)
                    && int.TryParse(port, out int portNum))
                {
                    // Starts the web server
                    WebServer webServer = new WebServer(root, address, portNum);
                    webServer.StartListener();

                    // Pressing a key stops the web server
                    Console.WriteLine("Press a key to end server");
                    Console.ReadKey();

                    webServer.StopListener();

                    return;
                }
            }

            Logger.Log($"Program failed to start with invalid arguments");

            Console.WriteLine("Required arguments:");
            Console.WriteLine("\tmyOwnWebServer -webRoot='root' -webIP='ip' -webPort='port'\n");
        }
    }
}
