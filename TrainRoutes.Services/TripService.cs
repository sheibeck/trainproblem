using QuickGraph;
using QuickGraph.Algorithms;
using QuickGraph.Algorithms.Observers;
using QuickGraph.Algorithms.RankedShortestPath;
using QuickGraph.Algorithms.ShortestPath;
using System;
using System.Collections.Generic;
using System.Linq;
using TrainRoutes.Domain.Models;
using TrainRoutes.Domain.Repositories;
using TrainRoutes.Services.Abstract;

namespace TrainRoutes.Services
{
    public class TripService : ITripService
    {
        public AdjacencyGraph<string, Edge<string>> routeGraph;
        public BidirectionalGraph<string, Edge<string>> routeGraphBi = new BidirectionalGraph<string, Edge<string>>();
        public Dictionary<Edge<string>, double> routeDistance;   
        
        public Exception ERR_NO_ROUTE = new Exception("NO SUCH ROUTE.");

        IStationService stationService;
        IRouteRepository routeRepository;

        public List<Route> existingRoutes;
        
        public TripService(IRouteRepository RouteRepository, IStationService StationService)
        {
            routeRepository = RouteRepository;
            stationService = StationService;

            CreateGraphs();
        }

        public void CreateGraphs()
        {
            routeGraph = new AdjacencyGraph<string, Edge<string>>(true);
                        
            foreach (Station s in stationService.GetAllStations())
            {
                routeGraph.AddVertex(s.StationName);
                routeGraphBi.AddVertex(s.StationName);
            }

            var allRoutes = routeRepository.GetAllRoutes();
            routeDistance = new Dictionary<Edge<string>, double>(allRoutes.Count());
            foreach (Route r in allRoutes)
            {
                var e = new Edge<string>(r.Origin, r.Destination);
                routeGraph.AddEdge(e);
                routeGraphBi.AddEdge(e);
                routeDistance.Add(e, r.Distance);
            }
        }
               
        #region PathFinding
        public Trip GetShortestTrip(Station origin, Station destination)
        {
            var edgeCost = AlgorithmExtensions.GetIndexer(routeDistance);
            var tryGetPath = routeGraph.ShortestPathsDijkstra(edgeCost, origin.StationName);

            IEnumerable<Edge<string>> path;
            if (!tryGetPath(destination.StationName, out path))
            {              
                return null;
            }
            else
            {
                var trip = new Trip(origin, destination);
                foreach (var e in path)
                {
                    trip.Routes.Add(GetRouteFromEdge(e));
                }
                return trip;
            }
        }

        public List<Trip> GetRankedTrips(Station origin, Station destination, int? pathCount = 1)
        {        
            var totalRoutes = routeRepository.GetAllRoutes().Count();

            //find every possible route            
            var edgeCost = AlgorithmExtensions.GetIndexer(routeDistance);
            var foundPaths = new HoffmanPavleyRankedShortestPathAlgorithm<string, Edge<string>>(routeGraphBi, edgeCost);
            foundPaths.SetRootVertex(origin.StationName);
            foundPaths.SetGoalVertex(destination.StationName);
            foundPaths.ShortestPathCount = (int)pathCount;
            foundPaths.Compute();            

            if (foundPaths.ComputedShortestPathCount == 0)
            {
                return null;
            }
            else {
                var trips = new List<Trip>();
                foreach (var path in foundPaths.ComputedShortestPaths)
                {
                    var trip = new Trip(origin, destination);
                    foreach (var edge in path)
                    {
                        trip.Routes.Add(GetRouteFromEdge(edge));
                    }
                    trips.Add(trip);
                }
                return trips.ToList();
            }
        }

        public List<Trip> GetRoundTrips(Station origin)
        {
            var stationsLeadingBackToOrigin = routeRepository.GetAllRoutes().Where(r => r.Destination == origin.StationName);

            var trips = new List<Trip>();
            // now find all the paths from origin to these stations
            foreach (var r in stationsLeadingBackToOrigin)
            {
                var originOfReturnTrip = new Station(r.Origin);
                var tripBackToOrigin = GetRankedTrips(originOfReturnTrip, origin, 1);
                foreach (var rtrip in tripBackToOrigin)
                {
                    var tripFromOrigin = GetRankedTrips(origin, originOfReturnTrip, 1);

                    foreach (var otrip in tripFromOrigin)
                    {
                        var trip = new Trip(origin, originOfReturnTrip);
                        trip.Routes.AddRange(otrip.Routes);
                        trip.Routes.AddRange(rtrip.Routes);
                        trips.Add(trip);
                    }
                }
            }

            return trips;
        }

        #endregion PathFinding

        #region Helpers
        private Route GetRouteFromEdge(Edge<string> edge)
        {
            return routeRepository.GetRoute(new Station(edge.Source), new Station(edge.Target));
        }
        #endregion Helpers
        
        
        public int GetTripDistance(List<Route> routes)
        {
            var distance = 0;
            foreach(var r in routes)
            {
                var route = routeRepository.GetRoute(new Station(r.Origin), new Station(r.Destination));
                if (route != null)
                {
                    distance += route.Distance;
                }
                else
                {
                    throw ERR_NO_ROUTE;
                }
            }

            return distance;
        }


        public List<Trip> GetRoundTripsWithMaxStops(Station origin, int maxStops)
        {
            var trips = GetRoundTrips(origin).Where(t => t.Routes.Count <= maxStops);

            if (trips == null)
            {
                throw ERR_NO_ROUTE;
            }
            else
            {
                return trips.ToList();
            }
        }


        public List<Trip> GetTripsWithMaxStops(Station origin, Station destination, int maxStops)
        {
            var trips = GetRankedTrips(origin, destination, maxStops).Where(t => t.Distance <= maxStops);

            if (trips == null)
            {
                throw ERR_NO_ROUTE;
            }
            else
            {
                return trips.ToList();
            }
        }


        public List<Trip> GetRoundTripsWithMaxDistance(Station origin, int maxDistance)
        {
            var trips = GetRoundTrips(origin).Where(t => t.Distance < maxDistance);

            if (trips == null)
            {
                throw ERR_NO_ROUTE;
            }
            else
            {
                return trips.ToList();
            }
        }

      
        public List<Trip> GetTripsWithNumberOfStops(Station origin, Station destination, int numStops)
        {
            var trips = GetRankedTrips(origin, destination, numStops).Where(t => t.Distance == numStops).ToList();

            if (trips == null)
            {
                throw ERR_NO_ROUTE;
            }
            else
            {
                return trips;
            }
        }

        
    }
}
