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
            var audiosFound = AudioContext.Audios.Where(audio => audio.Name.Contains(audioRequested));

            var queryResults = new List<InlineQueryResultVoice>();

            foreach (var audio in audiosFound)
            {
                var voiceResult = new InlineQueryResultVoice(audio.Name, audio.Url, audio.Name);
                queryResults.Add(voiceResult);
            }

            TelegramBotClient.AnswerInlineQueryAsync(inlineQuery.Id, queryResults);
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
