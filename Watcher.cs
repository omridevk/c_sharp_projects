using System;
using System.IO;
using System.Linq;
using System.Reactive;
using OTTProject.Queue;
using System.Threading;
using System.Reactive.Linq;
using OTTProject.Interfaces;
using OTTProject.Utils.Logging;
using System.Collections.Generic;
using System.Security.Permissions;


namespace OTTProject
{

    public class Watcher
    {

        /// <summary>
        /// Instance of a concurrent priority queue to play with.
        /// </summary>
        private static ConcurrentPriorityQueue<int, IGenerator> _queue = new ConcurrentPriorityQueue<int, IGenerator>();
        
        /// <summary>
        /// Instance of random to be used when setting item's priority before inserting to queue.
        /// </summary>
        private static Random _Rnd = new Random(5);

        private static readonly object _lock = new object();

        public static void Main()
        {
            // set logger level and set console output settings
            Logger.ConsoleOutput = true;
            Logger.SetVerbosity(VerbosityEnum.LEVEL.DEBUG);
            Run();

        }

        /// <summary>
        /// Handle file created event 
        /// </summary>
        /// <param name="file"></param>
        private static void HandleCreated(string file)
        {
            ThreadPool.QueueUserWorkItem(ConsumeQueue);
            XTVDGenerator generator = new XTVDGenerator(file);
            var item = new KeyValuePair<int, IGenerator>(_Rnd.Next(1, 20), generator);
            _queue.Enqueue(item);


        }

        /// <summary>
        /// Used by static thread to consume the concurrent queue.
        /// </summary>
        public static void ConsumeQueue(object stateInfo)
        {
            lock (_lock)
            {
                KeyValuePair<int, IGenerator> result = new KeyValuePair<int, IGenerator>();
                bool success = _queue.TryDequeue(out result);
                while (!success)
                {
                    success = _queue.TryDequeue(out result);
                }
                result.Value.Generate();
            }
            
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public static void Run()
        {
            string[] args = Environment.GetCommandLineArgs();
            // If a directory is not specified, exit program.
            if (args.Length != 2)
            {
                // Display the proper way to call the program.
                Logger.Critical("Usage: OTTProject.exe (directory)");
                return;
            }

            // Create a new FileSystemWatcher and set its properties.
            FileSystemWatcher watcher = new FileSystemWatcher();
            if (!Directory.Exists(args[1]))
            {
                Console.WriteLine("directory: " + args[1] + " doesn't exist, create it? (y/n)");
                if (Console.ReadKey().Key != ConsoleKey.Y)
                {
                    Logger.Critical("exiting.");
                    return;
                }
                DirectoryInfo dir = Directory.CreateDirectory(args[1]);                
            }
            watcher.Path = args[1];
            /* Watch for changes in LastAccess and LastWrite times, and
               the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName;
            // Only XML files at the moment.
            watcher.Filter = "*.xml";
            
            IObservable<EventPattern<FileSystemEventArgs>> created = Observable.FromEventPattern<FileSystemEventArgs>(watcher, "Created");
            IObservable<string> filePath = created
                .Select(change => change.EventArgs.FullPath)
                .Delay(TimeSpan.FromMilliseconds(500));

            filePath
                .Subscribe(file => HandleCreated(file));
            watcher.EnableRaisingEvents = true;
            // Wait for the user to quit the program.
            Logger.Info("Press any key to stop watching folder: {0}", args[1]);
            Console.ReadKey();
        }
    }


}
