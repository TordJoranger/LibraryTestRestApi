using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Library.API.Services;
using Library.API.Entities;
using Microsoft.EntityFrameworkCore;
using Library.API.Helpers;
using Library.API.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Library.API
    {
    public class Startup
        {
        private static IConfiguration _configuration;

        public Startup(IConfiguration configuration)
            {
            _configuration=configuration;
            }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
            {
            services.AddMvc();

            // register the DbContext on the container, getting the connection string from
            // appSettings (note: use this during development; in a production environment,
            // it's better to store the connection string in an environment variable)
            var connectionString = _configuration["connectionStrings:libraryDBConnectionString"];
            services.AddTransient<IPropertyMappingService, PropertyMappingService>();
            services.AddDbContext<LibraryContext>(o => o.UseSqlServer(connectionString));
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper, UrlHelper>(implementationFactory =>
            {
                var actionContext = implementationFactory.GetService<IActionContextAccessor>().ActionContext;
                return new UrlHelper(actionContext);
            });
            // register the repository
            services.AddScoped<ILibraryRepository, LibraryRepository>();
            }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
            ILoggerFactory loggerFactory, LibraryContext libraryContext)
            {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug(LogLevel.Information);

            if (env.IsDevelopment())
                {
                app.UseDeveloperExceptionPage();
                }
            else
                {
                app.UseExceptionHandler(appBuilder =>
                {
                    app.Run(async context =>
                    {
                        var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                        if (exceptionHandlerFeature!=null)
                            {
                            var logger = loggerFactory.CreateLogger("Global exception logger");
                            logger.LogError(500, exceptionHandlerFeature.Error, exceptionHandlerFeature.Error.Message);
                            }

                        context.Response.StatusCode=500;
                        await context.Response.WriteAsync("An unexpected fault happened. Try again later");
                    });
                });
                }

            libraryContext.EnsureSeedDataForContext();

            app.UseMvc();

            AutoMapper.Mapper.Initialize(config =>
            {
                config.CreateMap<Author, AuthorDto>()
                .ForMember(dest => dest.Name,
                    opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.Age,
                    opt => opt.MapFrom(src => src.DateOfBirth.GetCurrentAge()));

                config.CreateMap<Book, BookDto>();
                config.CreateMap<AuthorForCreationDto, Author>();
                config.CreateMap<BookForCreationDto, Book>();
                config.CreateMap<BookForUpdateDto, Book>();
                config.CreateMap<Book, BookForUpdateDto>();
            });
            }
        }
    }
