using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MemeAudioBot.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace MemeAudioBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            var token = Configuration["TELEGRAM_TOKEN"];
            var websiteUrl = Configuration["WEBSITE_HOSTNAME"];
            var webhookUrl = $"https://{websiteUrl}/api/bot";

            Program.TelegramBotClient = new TelegramBotClient(token);
            Program.TelegramBotClient.SetWebhookAsync(webhookUrl).Wait();
            
        }

        public static IConfiguration Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            AudioContext.ConnectionString = Configuration.GetConnectionString("AudioDatabase") ?? Configuration["AudioDatabase"];

            services.AddDbContext<AudioContext>(options => options.UseSqlServer(AudioContext.ConnectionString));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
