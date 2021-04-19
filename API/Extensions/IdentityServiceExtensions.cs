using System.Text;
using API.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using API.Data;
using System.Threading.Tasks;

namespace API.Extensions
{
   public static class IdentityServiceExtensions
   {
      public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
      {
         services.AddIdentityCore<AppUser>(options =>
         {
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireDigit = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.Password.RequiredLength = 1;
         })
            .AddRoles<AppRole>()
            .AddRoleManager<RoleManager<AppRole>>()
            .AddSignInManager<SignInManager<AppUser>>()
            .AddRoleValidator<RoleValidator<AppRole>>()
            .AddEntityFrameworkStores<DataContext>();

         services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(Options =>
            {
               Options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
               {
                  ValidateIssuerSigningKey = true,
                  IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"])),
                  ValidateIssuer = false,
                  ValidateAudience = false,
               };

               // Needed for signalr authorization
               Options.Events = new JwtBearerEvents
               {
                  OnMessageReceived = context =>
                  {
                     var accessToken = context.Request.Query["access_token"];

                     var path = context.HttpContext.Request.Path;
                     if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                     {
                        context.Token = accessToken;
                     }

                     return Task.CompletedTask;
                  }
               };
            });

         services.AddAuthorization(options =>
         {
            options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
            options.AddPolicy("ModeratePhotoRole", policy => policy.RequireRole("Admin", "Moderator"));
         });
         
         return services;
      }
   }
}