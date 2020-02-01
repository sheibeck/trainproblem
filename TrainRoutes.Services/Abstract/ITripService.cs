using System;
using System.Collections.Generic;
using System.Text;
using TrainRoutes.Domain.Models;

namespace TrainRoutes.Services.Abstract
{
    interface ITripService
    {
        Trip GetShortestTrip(Station origin, Station destination);
        List<Trip> GetRankedTrips(Station origin, Station destination, int? pathCount);
        List<Trip> GetRoundTrips(Station origin);
        List<Trip> GetRoundTripsWithMaxStops(Station origin, int maxStops);
        List<Trip> GetRoundTripsWithMaxDistance(Station origin, int maxDistance);

        List<Trip> GetTripsWithMaxStops(Station origin, Station destination, int maxStops);
        List<Trip> GetTripsWithNumberOfStops(Station origin, Station destination, int numStops);

        int GetTripDistance(List<Route> routes);
    }
}
