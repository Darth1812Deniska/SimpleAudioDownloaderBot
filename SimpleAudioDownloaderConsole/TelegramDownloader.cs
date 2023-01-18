using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using File = System.IO.File;

namespace SimpleAudioDownloaderConsole
{
    public class TelegramDownloader
    {
        private  TelegramBotClient telegramBot;
        private static YoutubeClient youtube = new YoutubeClient();

        private Queue<Message> Messages = new Queue<Message>();
        private Queue<Update> Updates = new Queue<Update>();
        private TelegramBotClient TelegramBot => telegramBot;


        public TelegramDownloader(string token)
        {
            telegramBot = new TelegramBotClient(token);
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // разрешено получать все виды апдейтов
            };
            TelegramBot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken);
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
                    await Task.Factory.StartNew(()=> SendTestMessageAsync(botClient, update));

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

        public static async Task SendTestMessageAsync(ITelegramBotClient bot, Update update)
        {
            // 
            // Парсим сообщение - проверям ссылка ли это
            var message = update.Message;
            string messaageText = message.Text;
            var chat = message.Chat;
            var video = await youtube.Videos.GetAsync(messaageText);
            if (video != null) 
            {
                var title = video.Title;
                var author = video.Author;
                string messageText = $"{author} {title}";
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(messaageText);
                var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                //streamInfo.Url
                //var stream = await youtube.Videos.Streams.GetAsync(streamInfo);
                await youtube.Videos.Streams.DownloadAsync(streamInfo, $"{messageText}.{streamInfo.Container}");
                
                using (FileStream audio = File.OpenRead($"{messageText}.{streamInfo.Container}"))
                {
                    await bot.SendAudioAsync(chat, audio);
                }
                await bot.SendTextMessageAsync(chatId: chat, text: messageText);
            }
            /*var message = update.Message;
            string mainMessaageText = message.Text;
            var chat = message.Chat;
            string messageText = $"Ответ на сообщение \"{mainMessaageText}\"";
            for (int i = 0; i < 10; i++)
            {
                bot.SendTextMessageAsync(chatId: chat, text: $"{messageText} №{i}");
            }
            */
        }
    }
}
