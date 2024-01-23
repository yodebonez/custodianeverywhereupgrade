using System;
using WindowsService1.NewsAPIJob;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            // Console.WriteLine("Hello World!");
            GetAllLeague.GetLeague();
            Console.ReadKey();
        }
    }
}
