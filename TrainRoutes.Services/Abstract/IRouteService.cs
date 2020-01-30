using System;
using System.Collections.Generic;
using System.Text;
using TrainRoutes.Domain.Models;

namespace TrainRoutes.Services.Abstract
{
    interface IRouteService
    {
        Trip GetTripByStations(List<Station> stops);

        List<Trip> GetTrips(Station origin, Station destination, int? maxStops);

        List<Trip> GetTripsWithNumberOfStops(Station origin, Station destination, int numStops);

        List<Trip> GetTripShortestDistance(Station origin, Station destination, int? numStops);

        List<Trip> GetAllRoutesForTrip(Station origin, Station destination);
    }
}
