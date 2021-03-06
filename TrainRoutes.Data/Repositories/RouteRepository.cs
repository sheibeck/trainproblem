﻿
using System.Collections.Generic;
using System.Linq;
using TrainRoutes.Domain.Models;
using TrainRoutes.Domain.Repositories;

namespace TrainRoutes.Data.Repositories
{
    public class RouteRepository : IRouteRepository
    {      
        //NOTE: Need to drop in a DbContext and hit a real database

        public IEnumerable<Route> GetAllRoutes()
        {
            return new List<Route>() {
                new Route() { RouteId = 1, Origin = "A", Destination = "B", Distance = 5 },
                new Route() { RouteId = 2, Origin = "B", Destination = "C", Distance = 4 },
                new Route() { RouteId = 3, Origin = "C", Destination = "D", Distance = 8 },
                new Route() { RouteId = 4, Origin = "D", Destination = "C", Distance = 8 },
                new Route() { RouteId = 5, Origin = "D", Destination = "E", Distance = 6 },
                new Route() { RouteId = 6, Origin = "A", Destination = "D", Distance = 5 },
                new Route() { RouteId = 7, Origin = "C", Destination = "E", Distance = 2 },
                new Route() { RouteId = 8, Origin = "E", Destination = "B", Distance = 3 },
                new Route() { RouteId = 9, Origin = "A", Destination = "E", Distance = 7 },                
            };
        }

        public Route GetRoute(Station origin, Station destination)
        {
            return GetAllRoutes().Where(r => r.Origin == origin.StationName && r.Destination == destination.StationName).FirstOrDefault();
        }
    }
}
