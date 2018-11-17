using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MemeAudioBot.Database;
using MemeAudioBot.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace MemeAudioBot.Controllers
{
    [Route("api/[controller]")]
    public class BotController : Controller
    {
        private readonly IMemeAudioService MemeAudioBot;

        public BotController(IMemeAudioService memeAudioBotService)
        {
            MemeAudioBot = memeAudioBotService;
        }

        [HttpGet]
        public string Get()
        {
            /*var audioRequested = AudioContext.Audios.FirstOrDefault(audio => audio.Name == "lele");
            string responseText = audioRequested?.ToString() ?? "No audio found";
            return responseText;*/

            return "ok";
        }

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Update update)
        {
            //await MemeAudioBot.ServeUpdateAsync(update);
            //return Ok();
        }
    }
}
