using OTTProject.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Security.Permissions;
using System.Threading;

namespace OTTProject
{

    public class Watcher
    {

        private static ConcurrentPriorityQueue<int, IGenerator> _queue = new ConcurrentPriorityQueue<int, IGenerator>();

        private static bool _Idle = true;

        private static readonly object _lock = new object();

        private static IList<Thread> _Threads = new List<Thread>();

        private static int _NumberOfThreads = 10;

        public static void Main()
        {
            for (int i = 0; i < _NumberOfThreads; i = i + 1)
            {
                Thread GeneratorThread = new Thread(ConsumeQueue);
                GeneratorThread.Start();
                _Threads.Add(GeneratorThread);
            }
            Run();
        }

        /// <summary>
        /// Handle file created event 
        /// </summary>
        /// <param name="file"></param>
        private static void HandleCreated(string file)
        {
            XTVDGenerator generator = new XTVDGenerator(file);
            var item = new KeyValuePair<int, IGenerator>(2, generator);
            _queue.Enqueue(item);
        }

        /// <summary>
        /// Used by static thread to consume the concurrent queue.
        /// </summary>
        public static void ConsumeQueue()
        {
            while(true)
            {
                Thread.Sleep(1000);
                KeyValuePair<int, IGenerator> result = new KeyValuePair<int, IGenerator>();
                bool success = _queue.TryDequeue(out result);
                lock (_lock)
                {
                    if (!success)
                    {
                        _Idle = true;
                        continue;
                    }
                    _Idle = false;
                    result.Value.Generate();
                }
            }
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public static void Run()
        {

            string[] args = Environment.GetCommandLineArgs();

            Logger.ConsoleOutput = false;
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
            Logger.Log("starting to watch folder: " + args[1]);
            // Wait for the user to quit the program.
            Console.WriteLine("Press any key to stop watching folder: " + args[1]);
            while (true)
            {
                Console.ReadKey();
                if (_Idle)
                {
                    foreach (var thread in _Threads)
                    {
                        thread.Abort();
                    }
                    return;
                }
                Console.WriteLine("a thread was busy, keep going");
            }
        }


    }
}
