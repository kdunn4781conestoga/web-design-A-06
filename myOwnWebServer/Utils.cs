
using System.Text.RegularExpressions;

namespace myOwnWebServer
{
    /// <summary>
    /// This class contains tools for the web server
    /// </summary>
    internal class Utils
    {
        /***
         * Check if a string is numeric
         * @param str: the string to check
         * @return true if the string is numeric, false otherwise
         */
        public static bool IsNumeric(string str)
        {
            return new Regex(@"^\d+$").IsMatch(str);
        }

        /***
         * Check if the connection is valid
         * @param root: the root directory
         * @param address: the address
         * @param port: the port
         * @return true if the connection is valid, false otherwise
         */
        public static bool IsConnectionValid(string root, string address, string port)
        {
            return !string.IsNullOrEmpty(root)
                && !string.IsNullOrEmpty(address)
                && !string.IsNullOrEmpty(port)
                && IsNumeric(port);
        }
    }
}