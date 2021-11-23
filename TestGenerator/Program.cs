using System;

namespace TestGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] srcFiles = new string[]
            {
                "C:\\Users\\nikst\\RiderProjects\\TestGenerator\\Example\\1.cs",
                "C:\\Users\\nikst\\RiderProjects\\TestGenerator\\Example\\2.cs"
            };
            string dstPath = "C:\\Users\\nikst\\RiderProjects\\TestGenerator\\Destiny";
            int pipelineLimit = 2;
            new Conveyer().startConveyer(srcFiles, dstPath, pipelineLimit);
            Console.ReadLine();
        }
    }
}