using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TrainRoutes.Domain.Models;

namespace TrainRoutes.Services.Abstract
{
    public interface ITripService
    {
        int GetTripDistance(List<Route> routes);
        List<Trip> GetTripsWithMaxStops(Station origin, Station destination, int maxStops);
        List<Trip> GetTripsWithDistanceLessThan(Station origin, Station destination, int maxDistance);
        List<Trip> GetTripsWithNumberOfStops(Station origin, Station destination, int numStops);
        Trip GetTripWithShortestDistance(Station origin, Station destination);
        List<Route> GetRouteFromString(string trip);
    }
}
