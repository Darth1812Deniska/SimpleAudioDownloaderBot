using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

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
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // разрешено получать все виды апдейтов
            };
            TelegramBot.StartReceiving(
                HandleUpdateAsync,
            HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            //threadgetUpdates.Start();
            //threadGetMessages.Start();

        }

        public static async Task HandleUpdateAsync(
            ITelegramBotClient botClient,
            Update update,
            CancellationToken cancellationToken
        )
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            if (update.Type == UpdateType.Message)
            {
                // Тут бот получает сообщения от пользователя
                // Дальше код отвечает за команду старт, которую можно добавить через botfather
                // Если все хорошо при запуске program.cs в консоль выведется, что бот запущен
                // а при отправке команды бот напишет Привет

                var message = update.Message;
                if (!string.IsNullOrEmpty(message.Text.ToLower()) )
                {
                    //await DataBaseMethods.ToggleInDialogStatus(update.Message.Chat.Id, 0);
                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: message.Text);
                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Привет");

                    return;
                }
            }
            if (update.Type == UpdateType.CallbackQuery)
            {
                // Тут получает нажатия на inline кнопки
            }
        }

        public static async Task HandleErrorAsync(
            ITelegramBotClient botClient,
            Exception exception,
            CancellationToken cancellationToken
        )
        {
            // Данный Хендлер получает ошибки и выводит их в консоль в виде JSON
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }

        void GetUpdates()
        {
            while(true)
            {
                var updates = TelegramBot.GetUpdatesAsync().Result;
                //TelegramBot.
                Console.WriteLine(updates.Count().ToString());
                var orderedUpdates = updates.Where(x =>x.Type==UpdateType.Message)
                    .ToList();
                foreach (var update in orderedUpdates)
                {
                    Updates.Enqueue(update);
                }
                Thread.Sleep(2000);
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
