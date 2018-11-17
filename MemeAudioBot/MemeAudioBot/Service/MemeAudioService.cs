using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MemeAudioBot.Database;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;

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
                .Select(audio => new InlineQueryResultVoice(audio.Name, audio.Url, audio.Name)) //todo: change result id to audio.AudioId
                .ToListAsync();

            var nextOffset = (offset + MaxResults).ToString();

            //todo: remove cacheTime, set to default
            await TelegramBotClient.AnswerInlineQueryAsync(inlineQuery.Id, queryResults, cacheTime: 0, nextOffset: nextOffset);
        }


        private async Task ServeMessageQuery(Update update)
        {
            var chat = update.Message.Chat;
            var audioRequestedName = update.Message.Text;

            if (audioRequestedName.StartsWith("/"))
                audioRequestedName = audioRequestedName.Substring(1);
            
            var audioRequested = AudioContext.Audios.FirstOrDefault(audio => audio.Name == audioRequestedName);

            if (audioRequested == null)
            {
                await TelegramBotClient.SendTextMessageAsync(chat, "No audio found");
            }
            else
            {
                var url = audioRequested.Url;

                var voiceFile = new InputOnlineFile(url);
                await TelegramBotClient.SendVoiceAsync(chat, voiceFile, caption: audioRequested.Name,
                    replyToMessageId: update.Message.MessageId);
            }
        }
    }
}
