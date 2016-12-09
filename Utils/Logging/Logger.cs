using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

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
        public static void Log(VerbosityEnum.LEVEL level, params string[] messages)
        {
            Log(_w, level, messages);
        }

        public static void Debug(params string[] messages)
        {
            Log(VerbosityEnum.LEVEL.DEBUG ,messages);
        }

        /// <summary>
        /// Info to console and file
        /// </summary>
        /// <param name="messages"></param>
        public static void Info(params string[] messages)
        {
            Log(VerbosityEnum.LEVEL.INFO, messages);
        }

        /// <summary>
        /// Error to console and file
        /// </summary>
        /// <param name="messages"></param>
        public static void Error(params string[] messages)
        {
            Log(VerbosityEnum.LEVEL.ERROR, messages);
        }
        /// <summary>
        /// Error to console and file
        /// </summary>
        /// <param name="messages"></param>
        public static void Warning(params string[] messages)
        {
            Log(VerbosityEnum.LEVEL.WARNING, messages);
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
        private static void Log(TextWriter w, VerbosityEnum.LEVEL level, params string[] messages)
        {
            lock (_lock)
            {
                if (_VerbosityLevel > level) return;
                string output = ParseMessage(level, messages);
                w.WriteLine(output);
                if (ConsoleOutput)
                {
                    Console.WriteLine(output);
                }
                w.Flush();
            }
        }


        private static string ParseMessage(VerbosityEnum.LEVEL level, params string[] messages)
        {
            string levelName = Enum.GetName(typeof(VerbosityEnum.LEVEL), level).ToLower();
            messages[0] = "[" + levelName + "]: " + messages[0];
            IList<string> tmp = new List<string>(messages);
            tmp.RemoveAt(0);
            string message = string.Format(messages[0], tmp.ToArray());
            // string concatenation happens at compile time so it's fine.
            return DateTime.Now.ToLongTimeString()
                 + " "
                 + DateTime.Now.ToLongDateString()
                 + " thread id: "
                 + Thread.CurrentThread.ManagedThreadId
                 + " "
                 + message;
        }
    }
}
