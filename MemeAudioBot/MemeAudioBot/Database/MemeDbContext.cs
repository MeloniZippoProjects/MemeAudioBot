using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MemeAudioBot.Database
{
    public class MemeDbContext : DbContext
    {
        public static string ConnectionString;
        public MemeDbContext(DbContextOptions<MemeDbContext> options) : base(options)
        { }

        public DbSet<Audio> Audios { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
    }
}
