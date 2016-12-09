using System;
using System.IO;
using System.Linq;
using System.Reactive;
using OTTProject.Queue;
using System.Threading;
using System.Reactive.Linq;
using OTTProject.Interfaces;
using System.Collections.Generic;
using System.Security.Permissions;
using OTTProject.Utils.Logging;

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
        private static Random _Rnd = new Random();

        private static readonly object _lock = new object();

        private static ManualResetEvent mre = new ManualResetEvent(false);

        public static void Main()
        {

            Thread.Sleep(1000);

            Run();
        }

        /// <summary>
        /// Handle file created event 
        /// </summary>
        /// <param name="file"></param>
        private static void HandleCreated(string file)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(
                ConsumeQueue));
            XTVDGenerator generator = new XTVDGenerator(file);
            var item = new KeyValuePair<int, IGenerator>(_Rnd.Next(1, 20), generator);
            _queue.Enqueue(item);

        }

        /// <summary>
        /// Used by static thread to consume the concurrent queue.
        /// </summary>
        public static void ConsumeQueue(object stateInfo)
        {
            Thread.Sleep(1000);
            
            KeyValuePair<int, IGenerator> result = new KeyValuePair<int, IGenerator>();
            bool success = _queue.TryDequeue(out result);
            lock (_lock)
            {
                if (!success)
                {
                    mre.WaitOne();
                    return;
                }
                result.Value.Generate();
            }
            
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public static void Run()
        {

            string[] args = Environment.GetCommandLineArgs();

            Logger.ConsoleOutput = true;
            // If a directory is not specified, exit program.
            if (args.Length != 2)
            {
                // Display the proper way to call the program.
                Console.WriteLine("Usage: Watcher.exe (directory)");
                return;
            }

            // Create a new FileSystemWatcher and set its properties.
            FileSystemWatcher watcher = new FileSystemWatcher();
            if (!Directory.Exists(args[1]))
            {
                Console.WriteLine("directory: " + args[1] + " doesn't exist, create it? (y/n)");
                if (Console.ReadKey().Key != ConsoleKey.Y)
                {
                    Console.WriteLine("exiting.");
                    return;
                }
                DirectoryInfo dir = Directory.CreateDirectory(args[1]);                
            }
            watcher.Path = args[1];
            /* Watch for changes in LastAccess and LastWrite times, and
               the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            // Only XML files at the moment.
            watcher.Filter = "*.xml";
            
            IObservable<EventPattern<FileSystemEventArgs>> created = Observable.FromEventPattern<FileSystemEventArgs>(watcher, "Created");
            IObservable<string> filePath = from change in created
                           select change.EventArgs.FullPath;

            filePath.Subscribe(
                file => HandleCreated(file)
            );
            // Begin watching.
            watcher.EnableRaisingEvents = true;
            Logger.Debug("starting to watch folder: {0}", args[1]);
            // Wait for the user to quit the program.
            Console.WriteLine("Press any key to stop watching folder: {0}", args[1]);
            while (true)
            {
                Console.ReadKey();
                Logger.Info("a thread was busy, keep going");
            }
        }


    }


}
