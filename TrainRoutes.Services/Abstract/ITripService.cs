using System;
using System.Collections.Generic;
using System.Text;
using TrainRoutes.Domain.Models;

namespace TrainRoutes.Services.Abstract
{
    interface ITripService
    {
        int GetTripDistance(List<Route> routes);
        List<Trip> GetTripsWithMaxStops(Station origin, Station destination, int maxStops);
        List<Trip> GetTripsWithDistanceLessThan(Station origin, Station destination, int maxDistance);
        List<Trip> GetTripsWithNumberOfStops(Station origin, Station destination, int numStops);
        Trip GetTripWithShortestDistance(Station origin, Station destination);        
    }
}
