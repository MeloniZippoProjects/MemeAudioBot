using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MemeAudioBot.Database
{
    public class Feedback
    {
        public int FeedbackId { get; set; }
        public string Text { get; set; }
        public string MessageJson { get; set; }
        public int FromUser { get; set; }
    }
}
