using System;

namespace SimpleAudioDownloaderConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            string token = "TOKEN";
            var TelegramDownloader = new TelegramDownloader(token);
            Console.WriteLine();
            Console.ReadKey();
        }
    }
}
