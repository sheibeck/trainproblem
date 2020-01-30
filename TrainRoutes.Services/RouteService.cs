using System;
using System.Collections.Generic;
using System.Linq;
using TrainRoutes.Domain.Models;
using TrainRoutes.Domain.Repositories;
using TrainRoutes.Services.Abstract;

namespace TrainRoutes.Services
{
    public class RouteService : IRouteService
    {
        IStationService stationService;
        IRouteRepository routeRepository;

        public List<Route> existingRoutes;
        
        public RouteService(IRouteRepository RouteRepository, IStationService StationService)
        {
            routeRepository = RouteRepository;
            stationService = StationService;
        }

        public Trip GetTripByStations(List<Station> stations)
        {
            if (stations == null || stations.Count < 2)
            {
                throw new Exception("A trip must have an origin and a destination.");
            }
            
            var trip = new Trip(stations.First(), stations.Last());

            //iterate over each station in the list creating a route to the next
            // station listed. Don't go past the end of the end of the list, i.e. count-2
            // since we are always using the i and i+1
            for(int i = 0; i <= stations.Count-2; i++)
            {
                var origin = stations[i];
                var destination = stations[i + 1];

                var route = routeRepository.GetRoute(origin, destination);
                if (route == null)
                {
                    throw new Exception($"Route with origin {origin} to destination {destination} doest not exist.");
                }

                trip.Routes.Add(route);
            }

            return trip;
        }

        public List<Trip> GetTrips(Station origin, Station destination, int? maxStops)
        {
            throw new NotImplementedException();
        }

        public List<Trip> GetTripsWithNumberOfStops(Station origin, Station destination, int numStops)
        {
            throw new NotImplementedException();
        }

        public List<Trip> GetTripShortestDistance(Station origin, Station destination, int? numStops)
        {
            throw new NotImplementedException();
        }

        public List<Trip> GetAllRoutesForTrip(Station origin, Station destination)
        {
            throw new NotImplementedException();
        }      
    }
}
