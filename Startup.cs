using CurrencyConverter.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CurrencyConverter
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSingleton<ICurrencyLookup, CurrencyLookup>();
            services.AddScoped<ICurrenciesData, CurrenciesData>();

            //DownloadData();

        }

        private void DownloadData()
        {
            //DateTime dt = DateTime.Today - 1;
            for (int i = 0; i < 10; i++)
            {
                DateTime dt = DateTime.Today.AddDays(-i);
                string histy = dt.ToString("yyyy-MM-dd");

                string filepath = Path.Combine("Data", histy + ".json");
                if (!File.Exists(filepath))
                {
                    var wbc = new WebClient();
                    wbc.Headers.Add("apikey", "6IJLr7E5CLFVMaeYarVAFv1QxzCdckcx");
                    string json = wbc.DownloadString("https://api.apilayer.com/fixer/" + histy);
                    //if (i == 0)
                    //    json = wbc.DownloadString("https://api.apilayer.com/fixer/latest" + histy);
                    //else
                    //    json = wbc.DownloadString("https://api.apilayer.com/fixer/" + histy);
                    File.WriteAllText(filepath, json);
                }
            }
            
            ////generate api key https://fixer.io/documentation
            
            //return json;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            var origins = Configuration.GetSection($"{"ApplicationSettings"}:{"Client_URLs"}").Get<string[]>();

            app.UseCors(options => options.WithOrigins(origins)
           .AllowAnyMethod()
           .AllowAnyHeader());

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
