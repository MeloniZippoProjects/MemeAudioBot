﻿// <auto-generated />
using MemeAudioBot.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MemeAudioBot.Migrations
{
    [DbContext(typeof(MemeDbContext))]
    partial class AudioContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024");

            modelBuilder.Entity("MemeAudioBot.Database.Audio", b =>
                {
                    b.Property<int>("AudioId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<string>("Url");

                    b.HasKey("AudioId");

                    b.ToTable("Audios");
                });
#pragma warning restore 612, 618
        }
    }
}
