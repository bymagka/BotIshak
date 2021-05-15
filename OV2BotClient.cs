using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using System.Net.Http;
using Telegram.Bot.Args;
using BotModel;

namespace OV2Bot
{
     public class OV2BotClient : TelegramBotClient
    {
        public List<BotUser> ActiveUsers { get; set; }

        public OV2BotClient(string token,HttpClient httpClient = null) : base(token,httpClient)
        {
            ActiveUsers = new List<BotUser>();
        }
    }
}
