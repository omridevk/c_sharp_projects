using System;
using System.IO;

namespace OTTProject
{
    public static class Logger
    {

        /// <summary>
        /// Lock for "semi" safe thread logging.
        /// </summary>
        private static readonly object _lock = new object();

        private static readonly StreamWriter _w;

        public static bool ConsoleOutput = false;
 

        /// <summary>
        /// static init to create/edit file.
        /// </summary>
        static Logger()
        {
            _w = File.AppendText("..\\..\\log.txt");
        }
        

        /// <summary>
        /// Public API to save the trouble of passing a textwriter instance.
        /// </summary>
        /// <param name="message"></param>
        public static void Log(string message)
        {
            message = "[log]: " + message;
            Log(message, _w);
        }

        public static void Info(string message)
        {
            message = "[info]: " + message;
            Log(message, _w);
        }

        public static void Error(string message)
        {
            message = "[error]: " + message;
            Log(message, _w);
        }
        public static void Warning(string message)
        {
            message = "[warning]:" + message;
            Log(message, _w);
        }

        /// <summary>
        /// private for ease of use (no need to pass TextWriter instance)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="w"></param>
        private static void Log(string message, TextWriter w)
        {
            lock (_lock)
            {
                string output = DateTime.Now.ToLongTimeString()
                     + " "  
                     + DateTime.Now.ToLongDateString()
                     + " "
                     + message;
                w.WriteLine(output);
                if (ConsoleOutput)
                {
                    Console.WriteLine(output);
                }
                w.Flush();
            }
        }
    }
}
