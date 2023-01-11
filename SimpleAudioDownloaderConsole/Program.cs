using System;

namespace SimpleAudioDownloaderConsole
{
    class Program
    {
        static void Main(string[] args)
        {

            string token = string.Empty;
            while (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Введите токен");
                token = Console.ReadLine();
            }
            var TelegramDownloader = new TelegramDownloader(token);
            Console.WriteLine();
            Console.ReadKey();
        }
    }
}
