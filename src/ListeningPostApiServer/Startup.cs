﻿using System.Text.Json.Serialization;
using ListeningPostApiServer.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ListeningPostApiServer
{
    /// <summary>
    ///     Startup class handles Dependency Injection Container configuration and Http Pipeline
    ///     configuration. Consumed by a WebHostBuilder.
    /// </summary>
    public class Startup
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Startup" /> class.
        /// </summary>
        /// <param name="configuration">The Configuration Interface injected by the HostBuilder.</param>
        public Startup(IConfiguration configuration) => Configuration = configuration;

        #endregion Constructors

        #region Properties

        /// <summary>
        ///     Exposes the configuration environment.
        /// </summary>
        /// <value>Configuration environment Interface.</value>
        /// <remarks>
        ///     Grants access to the configuration environment as implemented externally. Injected via
        ///     Dependency Injection by the Host Container.
        /// </remarks>
        public IConfiguration Configuration { get; }

        #endregion Properties

        #region Methods

        /// <summary>
        ///     Configures the Dependency Injection Container. (Registers Services).
        /// </summary>
        /// <param name="services">The Service Collection (Container) to configure.</param>
        /// <remarks>
        ///     This method gets called by the runtime. Use this method to add services to the container.
        /// </remarks>
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .ConfigureAppDbContext()
                .ConfigureRepositoryInjection()
                .ConfigureSwagger()
                .ConfigureHttps()
                .ConfigureCors(Configuration);

            services
                .ConfigureMvc().AddJsonOptions(options =>
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve);
        }

        /// <summary>
        ///     Configures the Http Pipeline.
        /// </summary>
        /// <param name="app">An Application Builder interface.</param>
        /// <param name="env">Interface to the Hosting Environment.</param>
        /// <remarks>
        ///     This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </remarks>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app
                    .UseDeveloperExceptionPage();
            else
                // The default HSTS value is 30 days. You may want to change this for production
                // scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();

            app
                .UseHttpsRedirection()
                .UseStaticFiles()
                .UseRouting()
                .UseCors()
                .UseSwagger()
                .UseSwaggerUI(options =>
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Listening Post API v1"))
                .UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }

        #endregion Methods
    }
}