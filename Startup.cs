using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using ViberAPI.Controllers;

namespace ViberAPI
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
            //services.AddSwaggerGen(c =>
            //{
            //    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ViberAPI", Version = "v1" });
            //});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseSwagger();
                //app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ViberAPI v1"));
            }

            app.UseRouting();

            //app.Use(async (context, next) =>
            //{
            //    // получаем конечную точку
            //    Endpoint endpoint = context.GetEndpoint();

            //    if (endpoint != null)
            //    {
            //        // получаем шаблон маршрута, который ассоциирован с конечной точкой
            //        var routePattern = (endpoint as Microsoft.AspNetCore.Routing.RouteEndpoint)?.RoutePattern?.RawText;

            //        Debug.WriteLine($"Endpoint Name: {endpoint.DisplayName}");
            //        Debug.WriteLine($"Route Pattern: {routePattern}");

            //        // если конечная точка определена, передаем обработку дальше
            //        await next();
            //    }
            //    else
            //    {
            //        Debug.WriteLine("Endpoint: null");
            //        // если конечная точка не определена, завершаем обработку
            //        await context.Response.WriteAsync("Endpoint is not defined");
            //    }
            //});

            app.UseAuthorization();
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    //await context.Response.WriteAsync(@"Go to https://viber.ars.ua/main/viber.ars.ua for set webhook");
                    await context.Response.WriteAsync(@"Hello! Don't go here.");
                });
                endpoints.MapControllers();
            });
        }
    }
}
