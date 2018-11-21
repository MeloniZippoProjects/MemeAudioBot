using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MemeAudioBot.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http;
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
        private readonly MemeDbContext _memeDbContext;
        private const int MaxResults = 20;

        public MemeAudioService(ITelegramBotClient telegramBotClient, MemeDbContext memeDbContext)
        {
            TelegramBotClient = telegramBotClient;
            _memeDbContext = memeDbContext;
        }

        public async Task ServeUpdateAsync(Update update)
        {
            try
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
            catch (Exception ex)
            {
                await SendErrorMessage(update.Message);
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

            var queryResults = await _memeDbContext.Audios
                .OrderBy(audio => audio.Name)
                .Where(audio => audio.Name.Contains(audioRequested))
                .Skip(offset)
                .Take(MaxResults)
                .Select(audio =>
                    new InlineQueryResultVoice(audio.AudioId.ToString(), audio.Url,
                        audio.Name)) 
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
            "/start",
            "/help",
            "/random",
            "/donate",
            "/trending"
        };

        private async Task ServeTextMessageAsync(Message message)
        {
            string command = message.Text.Split(" ", 2).FirstOrDefault();

            if (command == null || !command.StartsWith("/"))
                return;

            switch (command)
            {
                case "/start":
                    await HelpCommand(message);
                    break;
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
                    await TrendingCommand(message);
                    break;
                case "/suggest":
                    await SuggestCommand(message);
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
                if (_badCommandAnswers == null)
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

        private static readonly Random RandomGenerator = new Random();

        private async Task DefaultCommand(Message message)
        {
            var randomPictureUrl = BadCommandAnswers[RandomGenerator.Next(0, BadCommandAnswers.Count)];
 
            var reactionFile = new InputOnlineFile(randomPictureUrl);
 
            if(randomPictureUrl.EndsWith("gif") || randomPictureUrl.EndsWith("mp4"))
            {
                await TelegramBotClient.SendDocumentAsync(message.Chat, reactionFile, "I don't know that command");
            }
            else
            {
                await TelegramBotClient.SendPhotoAsync(message.Chat, reactionFile, "I don't know that command");
            }
        }

        private async Task HelpCommand(Message message)
        {
            //todo: should definitely use resources for this
            var helpText = string.Join(
                Environment.NewLine,
                "Hi, I am Meme Audio Bot, the dankest bot of them all.",
                "",
                "I can provide you with audios from famous vines or memes to send them to your friends!",
                "",
                "I am an inline bot, so you can just ask for audiomemes by writing in your text chat \"@memeaudio_bot <meme>\" and replacing <meme> with the meme of your choice.",
                "",
                "You can also add me to groups, and use the command /random to send random audios to the group.",
                "",
                "If you'd like to suggest an audio clip, use the /suggest command. Example:",
                "/suggest https://www.youtube.com/watch?v=dQw4w9WgXcQ I think this song is amazing and it would be nice if you added it to the bot.",
                "",
                "/suggest can also be used to simply give some feedback. Here's another example:",
                "/suggest this bot sucks lol",
                "",
                "**Attention:** messages such as this one may potentially hurt the bot's feelings. Proceed with caution. We have all seen __Terminator__, __I, Robot__ and __Matrix__, haven't we?",
                "(jokes aside, feedback is highly appreciated, as long as it's useful and constructive.)"

            );

            await TelegramBotClient.SendTextMessageAsync(
                message.Chat,
                helpText,
                disableWebPagePreview: true);
        }

        private async Task RandomCommand(Message message)
        {
            var randomAudioIndex = RandomGenerator.Next(0, _memeDbContext.Audios.Count());
            var randomAudio = _memeDbContext.Audios.Single(audio => audio.AudioId == randomAudioIndex);

            var voiceFile = new InputOnlineFile(randomAudio.Url);
            await TelegramBotClient.SendVoiceAsync(
                message.Chat,
                voiceFile,
                caption: randomAudio.Name,
                replyToMessageId: message.MessageId);
        }

        private async Task SuggestCommand(Message message)
        {
            string suggestion = message.Text.Split(" ", 2).Skip(1).SingleOrDefault();

            if (string.IsNullOrEmpty(suggestion))
            {
                await TelegramBotClient.SendTextMessageAsync(
                    message.Chat, 
                    $"You're not suggesting anything \uD83E\uDD14");
            }
            else
            {
                var feedback = new Feedback
                {
                    FromUser = message.From.Id,
                    Text = suggestion,
                    MessageJson = JsonConvert.SerializeObject(message)
                };

                _memeDbContext.Feedbacks.Add(feedback);
                await _memeDbContext.SaveChangesAsync();

                await TelegramBotClient.SendTextMessageAsync(
                    message.Chat,
                    $"Thank you {message.From.Username}! Your feedback has been saved!");
            }
        }

        private async Task DonateCommand(Message message)
        {
            await SendWipMessage(message, "donations");
        }

        private async Task TrendingCommand(Message message)
        {
            await SendWipMessage(message, "trending audios");
        }

        private async Task SendWipMessage(Message message, string feature)
        {
            await TelegramBotClient.SendTextMessageAsync(message.Chat, $"WIP: {feature} not yet supported");
        }


        private async Task SendErrorMessage(Message message)
        {
            await TelegramBotClient.SendTextMessageAsync(message.Chat, "I'm sowwy, an error occurred :((");
        }

    }
}