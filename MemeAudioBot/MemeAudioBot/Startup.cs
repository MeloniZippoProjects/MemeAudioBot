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
using MemeAudioBot.Service;

namespace MemeAudioBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            MemeDbContext.ConnectionString = Configuration.GetConnectionString("AudioDatabase") ?? Configuration["AudioDatabase"];
            services.AddDbContext<MemeDbContext>(options => options.UseSqlServer(MemeDbContext.ConnectionString));

            var token = Configuration["TELEGRAM_TOKEN"];
            var websiteUrl = Configuration["WEBSITE_HOSTNAME"];
            var webhookUrl = $"https://{websiteUrl}/api/bot";

            var telegramBotClient = new TelegramBotClient(token);
            telegramBotClient.SetWebhookAsync(webhookUrl).Wait();
            services.AddSingleton<ITelegramBotClient>(telegramBotClient);
            
            services.AddTransient<IMemeAudioService, MemeAudioService>();
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
