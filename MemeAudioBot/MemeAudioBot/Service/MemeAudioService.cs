using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MemeAudioBot.Database;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using File = System.IO.File;

namespace MemeAudioBot.Service
{
    public class MemeAudioService : IMemeAudioService
    {
        private readonly ITelegramBotClient TelegramBotClient;
        private readonly AudioContext AudioContext;
        private const int MaxResults = 20;

        public MemeAudioService(ITelegramBotClient telegramBotClient, AudioContext audioContext)
        {
            TelegramBotClient = telegramBotClient;
            AudioContext = audioContext;
        }

        public async Task ServeUpdateAsync(Update update)
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    await ServeMessageQuery(update);
                    break;
                case UpdateType.InlineQuery:
                    await ServeInlineQuery(update);
                    break;
                default:
                    break;
            }
        }

        private async Task ServeInlineQuery(Update update)
        {
            var inlineQuery = update.InlineQuery;
            var audioRequested = inlineQuery.Query;

            if (!int.TryParse(inlineQuery.Offset, out var offset))
            {
                offset = 0;
            }

            var queryResults = await AudioContext.Audios
                .Where(audio => audio.Name.Contains(audioRequested))
                .Skip(offset)
                .Take(MaxResults)
                .Select(audio =>
                    new InlineQueryResultVoice(audio.Name, audio.Url,
                        audio.Name)) //todo: change result id to audio.AudioId
                .ToListAsync();

            var nextOffset = (offset + MaxResults).ToString();

            //todo: remove cacheTime, set to default
            await TelegramBotClient.AnswerInlineQueryAsync(inlineQuery.Id, queryResults, cacheTime: 0,
                nextOffset: nextOffset);
        }


        private async Task ServeMessageQuery(Update update)
        {
            var message = update.Message;

            switch (message.Type)
            {
                case MessageType.Text:
                    await ServeTextMessageAsync(message);
                    break;
            }
        }


        private static readonly List<String> Commands = new List<string>
        {
            "/help",
            "/random",
            "/donate",
            "/trending"
        };

        private async Task ServeTextMessageAsync(Message message)
        {
            var command = message.Text;

            switch (command)
            {
                case "/help":
                    await HelpCommand(message);
                    break;
                case "/random":
                    await RandomCommand(message);
                    break;
                case "/donate":
                    await DonateCommand(message);
                    break;
                case "/trending":
                    break;
                default:
                    await DefaultCommand(message);
                    break;
            }
        }

        private static List<String> _badCommandAnswers = null;

        private static List<String> BadCommandAnswers
        {
            get
            {
                if (_badCommandAnswers == null || true)
                {
                    try
                    {
                        using (var fileReader = File.OpenText("./BadCommandAnswers.json"))
                        {
                            using (var jsonReader = new JsonTextReader(fileReader))
                            {
                                var badCommandAnswersJson = (JObject) JToken.ReadFrom(jsonReader);
                                _badCommandAnswers = badCommandAnswersJson["reactions"]
                                    .Select(jtoken => jtoken["url"].ToString())
                                    .ToList();
                            }
                        }
                    }
                    catch (JsonException)
                    { 
                        _badCommandAnswers = File.ReadLines("./BadCommandAnswers.txt").ToList();
                    }
                }

                return _badCommandAnswers;
            }
        }

        private static

        private static readonly Random RandomGenerator = new Random();

        private async Task DefaultCommand(Message message)
        {

            var randomPictureUrl = BadCommandAnswers[RandomImageIndexGenerator.Next(0, BadCommandAnswers.Count)];
 
            var reactionFile = new InputOnlineFile(randomPictureUrl);
 
            if(randomPictureUrl.EndsWith("gif") or randomPictureUrl.EndsWith("mp4"))
            {
                await TelegramBotClient.SendDocumentAsync(message.Chat, reactionFile, "wtf is that, I don't know that command");
            }
            else
            {
                await TelegramBotClient.SendPhotoAsync(message.Chat, reactionFile, "wtf is that, I don't know that command");
            }
        }

        private async Task HelpCommand(Message message)
        {
            var helpText = string.Join(
                Environment.NewLine,
                "Hi, I am Meme Audio Bot, the dankest bot of them all.",
                "",
                "I can provide you with audios from famous vines or memes to send them to your friends!",
                "",
                "I am an inline bot, so you can just ask for audiomemes by writing in your text chat \"@memeaudio_bot <meme>\" and replacing <meme> with the meme of your choice.",
                "",
                "You can also add me to groups, and use the command /random to send random audios to the group."
            );

            await TelegramBotClient.SendTextMessageAsync(message.Chat, helpText);
        }

        private async Task RandomCommand(Message message)
        {
            var randomAudioIndex = RandomGenerator.Next(0, AudioContext.Audios.Count());
            var randomAudio = AudioContext.Audios.Single(audio => audio.AudioId == randomAudioIndex);

            var voiceFile = new InputOnlineFile(randomAudio.Url);
            await TelegramBotClient.SendVoiceAsync(message.Chat, voiceFile, caption: randomAudio.Name,
                replyToMessageId: message.MessageId);
        }

        private async Task DonateCommand(Message message)
        {
            await TelegramBotClient.SendTextMessageAsync(message.Chat, "WIP: donations not yet supported");
        }
    }
}