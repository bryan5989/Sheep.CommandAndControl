﻿using System;
using System.IO;
using System.Reflection;
using ListeningPostApiServer.Controllers;
using ListeningPostApiServer.Data;
using ListeningPostApiServer.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace ListeningPostApiServer.Extensions
{
    /// <summary>
    ///     This is a collection of extension methods used during the configuration of the application.
    ///     These configurations are specific to this deployment, but also provide customization support.
    ///     Ideally you would read all of these values from a configuration file, in a ready to
    ///     sell/deploy application.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Configures the application database context.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection ConfigureAppDbContext(this IServiceCollection services) =>
            services.AddDbContext<AppDbContext>(options => options
                .UseInMemoryDatabase("MyDatabaseInMemory")
                .UseLazyLoadingProxies());

        /// <summary>
        ///     Configures the CORS settings.
        /// </summary>
        /// <param name="services">      The services.</param>
        /// <param name="appSettings"></param>
        /// <returns>IServiceCollection.</returns>
        /// <remarks>
        ///     CORS is not a security feature! CORS is a relaxation of security. I used it in this
        ///     project because this is not a production deployment. Do not copy what I did here to a
        ///     production project.
        /// </remarks>
        public static IServiceCollection ConfigureCors(this IServiceCollection services, IConfiguration appSettings) =>
            services.AddCors(options =>
                {
                    options.AddPolicy(CorsPolicyType.DropzoneUpload, builder =>
                        {
                            builder
                                .WithSmartOrigin(services, appSettings)
                                .WithHeaders(
                                    "cache-control",
                                    "x-requested-with"
                                );
                        }
                    );

                    options.AddPolicy(CorsPolicyType.MinimalPost, builder =>
                        {
                            builder
                                .WithSmartOrigin(services, appSettings)
                                .WithHeaders(
                                    "content-type"
                                );
                        }
                    );

                    options.AddPolicy(CorsPolicyType.MinimalGet,
                        builder =>
                        {
                            builder
                                .WithSmartOrigin(services, appSettings);
                        }
                    );
                }
            );

        /// <summary>
        ///     Configures the HTTPS settings.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection ConfigureHttps(this IServiceCollection services) =>
            services
                .AddHttpsRedirection(options =>
                {
                    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
                    options.HttpsPort = 5001;
                });

        /// <summary>
        ///     Configures the MVC options.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns>IMvcBuilder.</returns>
        /// <remarks>
        ///     While MVC is technically middleware (like just about everything in net core) MVC in this
        ///     project represents the last middleware in the request-response pipeline for this project.
        /// </remarks>
        public static IMvcBuilder ConfigureMvc(this IServiceCollection services) =>
            services
                .AddMvc(options => options.EnableEndpointRouting = false);

        /// <summary>
        ///     Configures the repository injection.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection ConfigureRepositoryInjection(this IServiceCollection services) =>
            services
                .AddScoped(typeof(IRepository<>), typeof(GenericRepository<>))
                .AddScoped<DbContext, AppDbContext>();

        /// <summary>
        ///     Configures the swagger.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection ConfigureSwagger(this IServiceCollection services)
        {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            return
                services.AddSwaggerGen(options =>
                    {
                        options.SwaggerDoc("v1", new OpenApiInfo
                        {
                            Title = "Listening Post API",
                            Version = "v1",
                            Description =
                                "A \"simple\" ASP.NET Core Web API for your typical, run-of-the-mill Command & Control Server",
                            Contact = new OpenApiContact
                            {
                                Name = "Bryan Gonzalez",
                                Email = "bgonza868@gmail.com",
                                Url = new Uri("https://github.com/bryan5989")
                            },
                            License = new OpenApiLicense
                            {
                                Name =
                                    "This is really not Licensed for distribution, but for a real project, it would be GNU GPLv3",
                                Url = new Uri("https://www.gnu.org/licenses/gpl-3.0.en.html")
                            }
                        });
                        options.IncludeXmlComments(xmlPath);
                    }
                );
        }
    }
}