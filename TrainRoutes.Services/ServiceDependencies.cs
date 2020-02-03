using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using TrainRoutes.Data.Repositories;
using TrainRoutes.Domain.Repositories;

namespace TrainRoutes.Services
{
    public static class ServiceDependencies
    {      
        public static IServiceCollection AddServiceDependencies(this IServiceCollection services)
        {
            services.AddSingleton<IRouteRepository, RouteRepository>();
            services.AddSingleton<IStationRepository, StationRepository>();

            return services;
        }       
    }
}
