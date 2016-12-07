using System.IO;
using System.Collections.Generic;


namespace OTTProject
{
    public class TestCases
    {


        private ConcurrentPriorityQueue<int, XTVDGenerator> _queue = new ConcurrentPriorityQueue<int, XTVDGenerator>();

        public void FillQueue()
        {
            try
            {
                XTVDGenerator handler = new XTVDGenerator("..\\..\\xml\\xmltv_ch_2231_FOX SPORTS CHILE_3793_201611231829.xml");               
                _queue.Enqueue(2, handler);
            }
            catch (FileNotFoundException e)
            {
                Logger.Log("recieved an error while opening file: " + e.Message);
            }

        }
        public bool EmptyQueue()
        {
            while (true)
            {
                KeyValuePair<int, XTVDGenerator> outOfQueue = new KeyValuePair<int, XTVDGenerator>();
                bool success = _queue.TryDequeue(out outOfQueue);
                if (!success) return false;
                Logger.Log("removed : " + outOfQueue.Key);
                outOfQueue.Value.Generate();

            }

        }
    }
}