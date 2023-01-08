using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace SimpleAudioDownloaderBot
{
    public partial class SimpleAudioDownloaderService : ServiceBase
    {
        private readonly string telegamToken = "TOKEN";
        private  TelegramBotClient telegramBot;

        private string TelegamToken => telegamToken;
        private TelegramBotClient TelegramBot => telegramBot;
        public SimpleAudioDownloaderService()
        {
            InitializeComponent();
            CanStop = true;
            CanPauseAndContinue = true;
        }

        protected override void OnStart(string[] args)
        {
            telegramBot = new TelegramBotClient(TelegamToken);
        }

        protected override void OnStop()
        {
            
        }
    }
}
