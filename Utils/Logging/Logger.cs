using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

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
        public static void Log(VerbosityEnum.LEVEL level, params object[] messages)
        {
            Log(_w, level, messages);
        }

        public static void Debug(params object[] messages)
        {
            Log(VerbosityEnum.LEVEL.DEBUG ,messages);
        }

        /// <summary>
        /// Info to console and file
        /// </summary>
        /// <param name="messages"></param>
        public static void Info(params object[] messages)
        {
            Log(VerbosityEnum.LEVEL.INFO, messages);
        }

        /// <summary>
        /// Error to console and file
        /// </summary>
        /// <param name="messages"></param>
        public static void Error(params object[] messages)
        {
            Log(VerbosityEnum.LEVEL.ERROR, messages);
        }
        /// <summary>
        /// Error to console and file
        /// </summary>
        /// <param name="messages"></param>
        public static void Warning(params object[] messages)
        {
            Log(VerbosityEnum.LEVEL.WARNING, messages);
        }
       
        /// <summary>
        /// Set the verbosity level.
        /// </summary>
        /// <param name="level"></param>
        public static void SetVerbosity(VerbosityEnum.LEVEL level)
        {
            lock (_lock)
            {
                _VerbosityLevel = level;
            }
        }

        /// <summary>
        /// private for ease of use (no need to pass TextWriter instance)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="w"></param>
        private static void Log(TextWriter w, VerbosityEnum.LEVEL level, params object[] messages)
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


        private static string ParseMessage(VerbosityEnum.LEVEL level, params object[] messages)
        {
            string levelName = Enum.GetName(typeof(VerbosityEnum.LEVEL), level).ToLower();
            // messages[0] contains string that might have {0}
            
            messages[0] = "[" + levelName + "]: " + messages[0];
            LinkedList<object> tmp = new LinkedList<object>(messages);
            object first = tmp.First.Value;
            tmp.RemoveFirst();
            string message = string.Format(first.ToString(), tmp.ToArray());
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
