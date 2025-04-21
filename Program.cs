
using System;
using JARVIS.Modules;

namespace JARVIS
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("JARVIS system booting up...");
            Logger.Log("System startup complete.");
            WakeWordListener.Listen();
        }
    }
}
