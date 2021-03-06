﻿using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSLunches.Api.Authorization;
using MSLunches.Api.Filters;
using MSLunches.Api.Mapper;
using MSLunches.Api.Middleware;
using MSLunches.Data.EF;
using MSLunches.Domain.Services;
using MSLunches.Domain.Services.Interfaces;
using MSLunches.Infrastructure.AuthZero;
using MSLunches.Infrastructure.RestClient;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;

namespace MSLunches.Api
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<MSLunchesContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // Add framework services.
            services.AddMvc().AddJsonOptions(
                options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            );

            IAuthorizationPolicies authorizationPolicies = new AuthorizationPolicies();
            services.AddSingleton(authorizationPolicies);

            services.AddAuthorization(options =>
            {
                options.AddPolicy(nameof(AuthorizationPolicies.AdminOnly), authorizationPolicies.AdminOnly);
            });

            //Registers the use of a jwt token
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(option =>
            {
                option.Audience = Configuration["auth0:audience"];
                option.Authority = $"https://{Configuration["auth0:domain"]}/";
            });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });

            //Creates the swagger json based on the documented xml/attributes of the endpoints
            services.AddSwaggerGen(c =>
            {
                //Metadata of the api
                c.SwaggerDoc("v1", GetSwaggerDoc());
                var xmlFile = $"{Assembly.GetEntryAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
                c.OperationFilter<ValidateModelResponseOperationFilter>();
            });

            // Register Infrastructure dependencies
            services.AddScoped<IRestClient>(sp => new RestClient($"https://{Configuration["auth0:domain"]}", new HttpClient()));
            services.AddSingleton<IAuthZeroClient>(sp => new AuthZeroClient(sp.GetRequiredService<IRestClient>(), Configuration["auth0:NonInteractiveClientId"], Configuration["auth0:NonInteractiveClientSecret"], Configuration["auth0:domain"]));
            services.AddTransient<IAuthZeroService>(sp => new AuthZeroService(sp.GetRequiredService<IAuthZeroClient>()));

            // Register Services
            services.AddTransient<IMealService>(sp => new MealService(sp.GetRequiredService<MSLunchesContext>()));
            services.AddTransient<IUserLunchService>(sp => new UserLunchService(sp.GetRequiredService<MSLunchesContext>()));
            services.AddTransient<ILunchService>(sp => new LunchService(sp.GetRequiredService<MSLunchesContext>()));
            services.AddTransient<IMealTypeService>(sp => new MealTypeService(sp.GetRequiredService<MSLunchesContext>()));

            // Add automapper
            services.AddAutoMapper(new[] {
                typeof(LunchProfile),
                typeof(MealProfile),
                typeof(UserLunchProfile),
                typeof(MealTypeProfile)
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, MSLunchesContext dbContext)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMiddleware(typeof(ErrorHandlingMiddleware));
            app.UseMiddleware(typeof(AuthorizationMiddleware));

            //Enable swagger midleware
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                //Sets swagger UI route on root, "GET {baseUrl}/ "
                c.RoutePrefix = string.Empty;
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApiCoreMSLunches V1");
            });

            //TODO : Add a list of supported origins on a config
            app.UseCors("AllowAllOrigins");
            app.UseAuthentication();
            app.UseMvc();
            DatabaseMSLunches.Initialize(dbContext);
        }

        /// <summary>
        /// Returns swagger metadata
        /// </summary>
        /// <returns></returns>
        private Info GetSwaggerDoc()
        {
            return new Info
            {
                Title = "WebApiCoreMSLunches",
                Version = "v1",
                Description = "Web Api MSLunches for MS",
                TermsOfService = "https://github.com/MakingSense/WebApiCore-MSLunches",
                Contact = new Contact
                {
                    Name = "Gastón Cerioni",
                    Email = "gcerioni@makingsense.com"
                },
                License = new License
                {
                    Name = "I would love to put one c:",
                    Url = "https://github.com/MakingSense/WebApiCore-MSLunches"
                }
            };
        }
    }
}
