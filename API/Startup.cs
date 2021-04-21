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
using Microsoft.OpenApi.Models;
using API.Data;
using Microsoft.EntityFrameworkCore;
using API.Interfaces;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using API.Extensions;
using API.Middleware;
using API.SignalR;

namespace API
{
   public class Startup
   {
      private readonly IConfiguration _config;
      public Startup(IConfiguration config)
      {
         _config = config;
      }


      // This method gets called by the runtime. Use this method to add services to the container.
      public void ConfigureServices(IServiceCollection services)
      {
         services.AddApplicationServices(_config);
         services.AddControllers();
         services.AddCors();
         services.AddIdentityServices(_config);
         services.AddSignalR();
         services.AddSwaggerGen(c =>
         {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
         });
      }

      // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
      public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
      {
         if (env.IsDevelopment())
         {
            // app.UseDeveloperExceptionPage();
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));
         }

         app.UseHttpsRedirection();

         app.UseRouting();
         app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins("https://localhost:4200"));

         app.UseAuthentication(); // must come before UseAuthorization
         app.UseAuthorization();

         app.UseDefaultFiles();     // Use default files like index.html
         app.UseStaticFiles();      // this is for other static files like the angular files

         app.UseEndpoints(endpoints =>
         {
            endpoints.MapControllers();
            endpoints.MapHub<PresenceHub>("hubs/presence");
            endpoints.MapHub<MessageHub>("hubs/message");
            endpoints.MapFallbackToController("Index", "Fallback");
         });
      }
   }
}
