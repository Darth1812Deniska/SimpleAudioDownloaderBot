using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SimpleAudioDownloaderConsole
{
    public class TelegramDownloader
    {
        private  TelegramBotClient telegramBot;

        private Queue<Message> Messages = new Queue<Message>();
        private Queue<Update> Updates = new Queue<Update>();
        private TelegramBotClient TelegramBot => telegramBot;

        
        public TelegramDownloader(string token)
        {
            telegramBot = new TelegramBotClient(token);
            Thread threadgetUpdates = new Thread(GetUpdates);
            Thread threadGetMessages = new Thread(GetMessages);
            threadgetUpdates.Start();
            threadGetMessages.Start();

        }

        void GetUpdates()
        {
            while(true)
            {
                var updates = TelegramBot.GetUpdatesAsync().Result;
                var orderedUpdates = updates.OrderBy(x => x.Id).ToList();
                foreach (var update in orderedUpdates)
                {
                    Updates.Enqueue(update);
                }
                Thread.Sleep(10000);
            }
        }



        void GetMessages()
        {
            while(true)
            {
                if (Updates.Count > 0)
                {
                    Update update = Updates.Dequeue();
                    Console.WriteLine(update.Message.Text);
                }
                Thread.Sleep(1000);
            }
        }
    }
}
