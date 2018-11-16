using System;
using System.Collections.Generic;
using System.Linq;
using MemeAudioBot.Database;
using Telegram.Bot;
using Telegram.Bot.Types;
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

        public void ServeUpdate(Update update)
        {
            if (update.InlineQuery == null)
            {
                ServeMessageQuery(update);
            }
            else
            {
                ServeInlineQuery(update);
            }
        }
        
        private void ServeInlineQuery(Update update)
        {
            var inlineQuery = update.InlineQuery;
            var audioRequested = inlineQuery.Query;

            if (!int.TryParse(inlineQuery.Offset, out var offset))
            {
                offset = 0;
            }

            var audiosFound = AudioContext.Audios.Where(audio => audio.Name.Contains(audioRequested)).Skip(offset).Take(MaxResults);
            
            var queryResults = new List<InlineQueryResultVoice>();

            //todo: can be parallelized with linq?
            foreach (var audio in audiosFound)
            {
                //todo: change result id to audio.AudioId
                var voiceResult = new InlineQueryResultVoice(audio.Name, audio.Url, audio.Name);
                queryResults.Add(voiceResult);
            }

            var nextOffset = (offset + MaxResults).ToString();

            //todo: remove cacheTime, set to default
            TelegramBotClient.AnswerInlineQueryAsync(inlineQuery.Id, queryResults, cacheTime: 0, nextOffset: nextOffset);
        }


        private void ServeMessageQuery(Update update)
        {
            var chat = update.Message.Chat;
            var audioRequestedName = update.Message.Text;

            if (audioRequestedName.StartsWith("/"))
                audioRequestedName = audioRequestedName.Substring(1);


            var audioRequested = AudioContext.Audios.FirstOrDefault(audio => audio.Name == audioRequestedName);

            if (audioRequested == null)
            {
                TelegramBotClient.SendTextMessageAsync(chat, "No audio found").Wait();
            }
            else
            {
                var url = audioRequested.Url;

                var voiceFile = new InputOnlineFile(url);
                TelegramBotClient.SendVoiceAsync(chat, voiceFile, caption: audioRequested.Name,
                    replyToMessageId: update.Message.MessageId);
            }
        }
    }
}
