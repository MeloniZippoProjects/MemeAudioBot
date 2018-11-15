using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MemeAudioBot.Database
{
    public class AudioContext : DbContext
    {
        public static string ConnectionString;
        public AudioContext(DbContextOptions<AudioContext> options) : base(options)
        { }

        public DbSet<Audio> Audios { get; set; }
    }
}
