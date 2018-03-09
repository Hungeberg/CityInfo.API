using CityInfo.API.Entities;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.IO;

namespace CityInfo.API
{
    public class Startup
    {
        public static IConfiguration Configuration { get; private set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Asp.Net configuration...
            // add MVC support and add support for XML output
            services.AddMvc()
                .AddMvcOptions(o => o.OutputFormatters.Add(
                    new XmlDataContractSerializerOutputFormatter())); // Support XML output.

            /*.AddJsonOptions(o => {
                // Change JSON output format.
                if (o.SerializerSettings.ContractResolver != null)
                {
                    var resolver = o.SerializerSettings.ContractResolver
                        as DefaultContractResolver;
                    resolver.NamingStrategy = null; // Use object names as is.
                }
            });*/

            // Swagger API documentation service...
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "CityInfo API", Version = "v1" });
                
                // Set the comments path for the Swagger JSON and UI.
                var basePath = AppContext.BaseDirectory;
                var xmlPath = Path.Combine(basePath, "CityInfo.API.xml");
                c.IncludeXmlComments(xmlPath);
            });

            // Custom service IoC registration...
#if DEBUG
            services.AddTransient<IMailService, LocalMailService>();
#else
            services.AddTransient<IMailService, CloudMailService>();
#endif
            // Database setup and IoC registration...
            var connectionString = Configuration["connectionStrings:cityInfoDBConnectionString"];
            services.AddDbContext<CityInfoContext>(o => o.UseSqlServer(connectionString));

            // Repository service IoC registration...
            // we use scoped creation here so the repo is created once pr. request.
            services.AddScoped<ICityInfoRepository, CityInfoRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
            CityInfoContext cityInfoContext)
        {
            // 3 environments are available default: Development, Staging and Production
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else // Staging or Production..
            {
                app.UseExceptionHandler();
            }

            // Seed database if needed...
            cityInfoContext.EnsureSeedDataForContext();

            // Show html output for status codes...
            app.UseStatusCodePages();

            // Configure AutoMapper for mapping between database and dto objects...
            AutoMapper.Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<City, CityWithoutPointsOfInterestDto>();
                cfg.CreateMap<City, CityDto>();
                cfg.CreateMap<PointOfInterest, PointOfInterestDto>();
                cfg.CreateMap<PointOfInterestForCreationDto, PointOfInterest>();
                cfg.CreateMap<PointOfInterestForUpdateDto, PointOfInterest>();
                cfg.CreateMap<PointOfInterest, PointOfInterestForUpdateDto>();
            });

            app.UseMvc();

            // Configure Swagger API documentation...
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CityInfo API V1");
            });
        }
    }
}
