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
        private static readonly YoutubeClient Youtube = new YoutubeClient();

        private TelegramBotClient TelegramBot { get; }


        public TelegramDownloader(string token)
        {
            TelegramBot = new TelegramBotClient(token);
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // разрешено получать все виды апдейтов
            };
            TelegramBot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken);
        }

        public async Task HandleUpdateAsync(
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
                if (message != null && !string.IsNullOrEmpty(message.Text.ToLower()))
                {
                    await Task.Factory.StartNew(() => SendTestMessageAsync(botClient, update));

                    return;
                }
            }

            if (update.Type == UpdateType.CallbackQuery)
            {
                // Тут получает нажатия на inline кнопки
            }
        }

        public async Task HandleErrorAsync(
            ITelegramBotClient botClient,
            Exception exception,
            CancellationToken cancellationToken
        )
        {
            // Данный Хендлер получает ошибки и выводит их в консоль в виде JSON
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }

        public async Task SendTestMessageAsync(ITelegramBotClient bot, Update update)
        {
            var message = update.Message;
            string messaageText = message.Text;
            var chat = message.Chat;
            var video = await Youtube.Videos.GetAsync(messaageText);

            var title = video.Title;
            var author = video.Author.ChannelTitle;
            var duration = (int)video.Duration.Value.TotalSeconds;
            string messageText = $"{author} {title}";
            StreamManifest streamManifest = await Youtube.Videos.Streams.GetManifestAsync(messaageText);
            var streamInfo = GetOptimalAudioStreamInfo(streamManifest);
            //var streamSize = streamInfo.Size.MegaBytes;
            //await bot.SendTextMessageAsync(chatId: chat, text: streamSize.ToString());
            //streamInfo.Url
            //var stream = await youtube.Videos.Streams.GetAsync(streamInfo);
            await Youtube.Videos.Streams.DownloadAsync(streamInfo, $"{video.Id.Value}.{streamInfo.Container}");

            using (FileStream audio = File.OpenRead($"{video.Id.Value}.{streamInfo.Container}"))
            {
                await bot.SendAudioAsync(chatId: chat, audio: audio, duration: duration, performer: author,
                    title: title);
            }

            await bot.SendTextMessageAsync(chatId: chat, text: messageText);
        }

        private AudioOnlyStreamInfo GetOptimalAudioStreamInfo(StreamManifest streamManifest)
        {
            var audioStreamsInfo = streamManifest.GetAudioOnlyStreams();
            return audioStreamsInfo
                .Where(x => (x.Container == Container.Mp4 || x.Container == Container.Mp3) && x.Size.MegaBytes < 50)
                .MaxBy(x => x.Size);
        }
    }
}