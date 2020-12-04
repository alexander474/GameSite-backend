using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ExamBackend.Models;
using ExamBackend.Models.Game;
using Microsoft.Extensions.Options;
using ExamBackend.Services;

namespace ExamBackend
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
            services.Configure<GamesDatabaseSettings>(
                Configuration.GetSection(nameof(GamesDatabaseSettings))
            );

            services.AddSingleton<IGamesDatabaseSettings>( sp => 
                sp.GetRequiredService<IOptions<GamesDatabaseSettings>>().Value
            );

            services.AddSingleton<GameService>();
            services.AddSingleton<CharacterService>();
            services.AddControllers();
            services.AddCors( 
                options => {
                    options.AddPolicy("AllowAll", 
                        builder => builder 
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowAnyOrigin()
                    );
                }
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseCors("AllowAll");

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
