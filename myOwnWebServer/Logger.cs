/*
* FILE			: Logger.cs
* PROJECT		: Assignment 6
* PROGRAMMERS	: Kyle Dunn, David Czachor
* FIRST VERSION : 2022-12-01
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace myOwnWebServer
{
    /// <summary>
    /// This class handles writing messages to a log file
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// This function logs a message to the myOwnWebServer.log
        /// </summary>
        /// <param name="action">the action for the log message</param>
        /// <param name="message">the message for the log</param>
        public static void Log(string message, string action = null)
        {
            string formattedMsg = null;
            try
            {
                // writes the message to the log file
                using (StreamWriter writer = File.AppendText("./myOwnWebServer.log"))
                {
                    formattedMsg = $"{DateTime.Now} ";

                    if (action != null)
                    {
                        formattedMsg += $"[{action}] ";
                    }

                    formattedMsg += Regex.Replace(message, "[\\r\\n]+", "\\s");

                    writer.WriteLine(formattedMsg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
