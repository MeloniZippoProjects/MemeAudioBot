using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Telegram.Bot.Types;

namespace MemeAudioBot.Controllers
{
    [Route("api/[controller]")]
    public class BotController : Controller
    {
        private readonly IConfiguration Configuration;

        public BotController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // POST api/values
        [HttpPost]
        public string Post([FromBody]Update update)
        {
            var chat = update.Message.Chat;
            var text = update.Message.Text + " sei tu!!!";
            
            Program.TelegramBotClient.SendTextMessageAsync(chat, text).Wait();

            return "ok";
        }
    }
}
