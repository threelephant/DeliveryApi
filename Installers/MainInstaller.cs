using System;
using Delivery.Filters;
using Delivery.Models;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Installers
{
    public class MainInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddControllers(options =>
                {
                    options.Filters.Add<ValidationFilter>();
                })
                .AddFluentValidation(mvcConfiguration => 
                    mvcConfiguration.RegisterValidatorsFromAssemblyContaining<Startup>());

            services.AddDbContext<deliveryContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
                options.LogTo(Console.WriteLine);
            });
            
            services.AddCors();
        }
    }
}