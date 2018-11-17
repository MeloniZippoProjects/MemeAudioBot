using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace MemeAudioBot.Service
{
    public interface IMemeAudioService
    {
        Task ServeUpdateAsync(Update update);
    }

}
