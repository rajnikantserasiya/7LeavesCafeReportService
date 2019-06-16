using BusinessLogicLayer.Contracts;
using BusinessLogicLayer.Models;
using BusinessLogicLayer.Services;
using LoggerService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MVCCoreApplication.ExceptionHandler;
using NLog;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MVCCoreApplication
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            //Configure NLog
            LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));

            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            RegisterServices(services);            
            ConfigureJWTTokenService(services);

        }
        

        /// <summary>
        /// Register Service components
        /// </summary>
        /// <param name="services"></param>
        public void RegisterServices(IServiceCollection services)
        {
            //Register configuration options of appsettings.json file
            services.Configure<DatabaseSettings>(Configuration.GetSection("DatabaseSettings"));
            services.AddOptions();

            //Register interface and service class            
            services.AddScoped<IUserManagementService, UserManagementService>();
            services.AddScoped<RequestDelegate>();
            //Register Logger service class as Singleton
            services.AddSingleton<ILoggerManager, LoggerManager>();
        }

        /// <summary>
        /// Configure JWT as default authentication scheme
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureJWTTokenService(IServiceCollection services)
        {
            // Register default authentication scheme to JWT Bearer scheme
            services.Configure<TokenManagementModel>(Configuration.GetSection("tokenManagement"));
            var token = Configuration.GetSection("tokenManagement").Get<TokenManagementModel>();
            var IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(token.Secret));

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    // Validate the token expiry
                    ValidateLifetime = true,

                    // The signing key must match!
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = IssuerSigningKey,

                    // Validate the JWT Issuer (iss) claim
                    ValidateIssuer = false,
                    ValidIssuer = token.Issuer,

                    // Validate the JWT Audience (aud) claim
                    ValidateAudience = false,
                    ValidAudience = token.Audience,

                    //token must have expiration time value
                    RequireExpirationTime = true,

                    // If you want to allow a certain amount of clock drift, set that here:
                    ClockSkew = TimeSpan.Zero
                };
                x.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerManager logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // intercept token expiration unauthorised request
            app.UseStatusCodePages(async context =>
            {
                if (context.HttpContext.Request.Path.StartsWithSegments("/api"))
                {
                    if (context.HttpContext.Response.StatusCode == 401 ||
                    context.HttpContext.Response.StatusCode == 403)
                    {
                        logger.LogError(string.Format("Unauthorized request. StatusCode:{0} Token is expired or is not valid.", context.HttpContext.Response.StatusCode));
                        await context.HttpContext.Response.WriteAsync(ResponseMessageModel.CreateResponseMessage("Unauthorised", "Token is expired or is not valid."));
                    }
                    else
                    {

                    }
                }
            });

            app.UseMiddleware<CustomExceptionMiddleware>();


            // AlLOW CORS request
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().WithExposedHeaders(new string[] { "Token-Expired" }));
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            // Register JWT authentication
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }

    
}
