using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OTTProject.Utils.Logging
{
    public static class Logger
    {

        /// <summary>
        /// Lock for "semi" safe thread logging.
        /// </summary>
        private static readonly object _lock = new object();

        private static readonly StreamWriter _w;

        /// <summary>
        /// Output to console or not.
        /// </summary>
        public static bool ConsoleOutput = false;

        private static VerbosityEnum.LEVEL _VerbosityLevel = VerbosityEnum.LEVEL.DEBUG;
        /// <summary>
        /// static init to create/edit file.
        /// </summary>
        static Logger()
        {
            var dir = Directory.GetCurrentDirectory();
            _w = File.AppendText(Path.Combine(dir, "log.txt"));
        }
        

        /// <summary>
        /// Public API to save the trouble of passing a textwriter instance.
        /// </summary>
        /// <param name="message"></param>
        public static void Log(params string[] messages)
        {
            messages[0] = "[log]: " + messages[0];
            Log(_w, VerbosityEnum.LEVEL.DEBUG, messages);
        }

        /// <summary>
        /// Info to console and file
        /// </summary>
        /// <param name="messages"></param>
        public static void Info(params string[] messages)
        {
            messages[0] = "[info]: " + messages[0];
            Log(_w, VerbosityEnum.LEVEL.INFO, messages);
        }

        /// <summary>
        /// Error to console and file
        /// </summary>
        /// <param name="messages"></param>
        public static void Error(params string[] messages)
        {
            messages[0] = "[error]: " + messages[0];
            Log(_w, VerbosityEnum.LEVEL.ERROR, messages);
        }
        /// <summary>
        /// Error to console and file
        /// </summary>
        /// <param name="messages"></param>
        public static void Warning(params string[] messages)
        {
            messages[0] = "[warning]: " + messages[0];
            Log(_w, VerbosityEnum.LEVEL.WARNING, messages);
        }

        /// <summary>
        /// Set the verbosity level.
        /// </summary>
        /// <param name="level"></param>
        public static void SetVerbosity(VerbosityEnum.LEVEL level)
        {
            _VerbosityLevel = level;
        }

        /// <summary>
        /// private for ease of use (no need to pass TextWriter instance)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="w"></param>
        private static void Log(TextWriter w,VerbosityEnum.LEVEL level, params string[] messages)
        {
            lock (_lock)
            {
                if (level < _VerbosityLevel) return;
                IList<string> tmp = new List<string>(messages);
                tmp.RemoveAt(0);
                string message = String.Format(messages[0], tmp.ToArray());
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
