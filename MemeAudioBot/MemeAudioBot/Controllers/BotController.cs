﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MemeAudioBot.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MemeAudioBot.Controllers
{
    [Route("api/[controller]")]
    public class BotController : Controller
    {
        private readonly IConfiguration Configuration;
        private readonly AudioContext AudioContext;
        private readonly ITelegramBotClient TelegramBotClient;

        public BotController(IConfiguration configuration, AudioContext audioContext, ITelegramBotClient telegramBotClient)
        {
            Configuration = configuration;
            AudioContext = audioContext;
            TelegramBotClient = telegramBotClient;
        }

        [HttpGet]
        public string Get()
        {
            var audioRequested = AudioContext.Audios.FirstOrDefault(audio => audio.Name == "lele");
            string responseText = audioRequested?.ToString() ?? "No audio found";
            return responseText;
        }

        // POST api/values
        [HttpPost]
        public string Post([FromBody]Update update)
        {
            var chat = update.Message.Chat;
            var audioRequestedName = update.Message.Text;

            var audioRequested = AudioContext.Audios.FirstOrDefault(audio => audio.Name == audioRequestedName);
            string responseText = audioRequested?.ToString() ?? "No audio found";
            TelegramBotClient.SendTextMessageAsync(chat, responseText).Wait();

            return "ok";
        }
    }
}
