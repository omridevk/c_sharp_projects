using System;
using System.IO;

namespace OTTProject
{
    public static class Logger
    {

        private static readonly object _lock = new object();

        private static readonly StreamWriter _w; 
 

        static Logger()
        {
            _w = File.AppendText("..\\..\\log.txt");
        }
        

        public static void Log(string message)
        {
            Logger.Log(message, Logger._w);
        }

        private static void Log(string message, TextWriter w)
        {
            lock (_lock)
            {
                w.WriteLine(
                    "{0} {1}: {2}", 
                    DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString(),
                    message
                );
                w.Flush();
            }
        }
    }
}
