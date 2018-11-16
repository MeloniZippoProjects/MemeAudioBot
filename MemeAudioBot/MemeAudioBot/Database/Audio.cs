using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MemeAudioBot.Database
{
    public class Audio
    {
        public int AudioId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }


        public override string ToString()
        {
            return $"{AudioId}: {Name} at {Url}";
        }
    }
}
