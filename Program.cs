
using System;
using System.Threading.Tasks;

namespace OTTProject
{

    public class Program
    {
        public static void Main(string[] args)
        {
            DateTime localDate = DateTime.Now;
            Logger.Log("starting test: " + localDate.ToString());
            TestCases test = new TestCases();
            test.FillQueue();
            test.EmptyQueue();
            Logger.Log("end test: " + localDate.ToString());
        }


    }


}
