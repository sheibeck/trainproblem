using QuickGraph;
using QuickGraph.Algorithms;
using QuickGraph.Algorithms.RankedShortestPath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        private void CreateGraphs()
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
        private Trip GetShortestTrip(Station origin, Station destination)
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

        private List<Trip> GetRankedTrips(Station origin, Station destination, int? pathCount = 1)
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
            else
            {
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
                return trips;
            }
        }

        private List<Trip> GetRoundTrips(Station origin)
        {
            //try to get back to the origin from any other destination;            
            var stations = stationService.GetAllStations().Where(s => s.StationName != origin.StationName).ToList();
            var routesCount = routeRepository.GetAllRoutes().Count();

            var trips = new List<Trip>();
                                  
            // now find all the paths from origin to these stations
            foreach (var r in stations)
            {
                // we don't care for the shortest necessarily, but we want every path, so account for longer paths
                for (var m = 0; m < routesCount; m++)
                {
                    var originOfReturnTrip = r;
                    var tripBackToOrigin = GetRankedTrips(originOfReturnTrip, origin, m);

                    if (tripBackToOrigin != null)
                        foreach (var rtrip in tripBackToOrigin)
                        {
                            var tripFromOrigin = GetRankedTrips(origin, originOfReturnTrip, m);

                            if (tripFromOrigin != null)
                                foreach (var otrip in tripFromOrigin)
                                {
                                    var trip = new Trip(origin, originOfReturnTrip);
                                    trip.Routes.AddRange(otrip.Routes);
                                    trip.Routes.AddRange(rtrip.Routes);

                                    //if this trip doesn't exist, then add it
                                    if (!trips.Any(x => x.DisplayRoute == trip.DisplayRoute))
                                    {
                                        trips.Add(trip);
                                    }
                                }
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

        public List<Route> GetRouteFromString(string trip)
        {
            var route = new List<Route>();
            if (trip.Length > 1)
            {                
                for (var c = 0; c < trip.Length-1; c++)
                {
                    var r = new Route();
                    r.Origin = trip[c].ToString().ToUpper();
                    r.Destination = trip[c + 1].ToString().ToUpper();
                    route.Add(r);
                }
            }
            else
            {
                throw ERR_NO_ROUTE;
            }
            return route;
        }

#endregion Helpers


        public int GetTripDistance(List<Route> routes)
        {
            var distance = 0;
            foreach (var r in routes)
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


        public List<Trip> GetTripsWithMaxStops(Station origin, Station destination, int maxStops)
        {
            List<Trip> trips;
            if (origin.StationName == destination.StationName)
            {
                trips = GetRoundTrips(origin).Where(t => t.Routes.Count <= maxStops).ToList();
            }
            else
            {
                trips = GetRankedTrips(origin, destination, maxStops);
            }

            var tripsAdded = -1;
            //keep riding the train until we bring every route up to a maximum that does not exceed the numToMatch            
            while (trips.Count > 0 && tripsAdded != 0)
            {
                tripsAdded = 0;
                var tripsToAdd = new List<Trip>();

                foreach (var t in trips)
                {
                    //if we're less than the distance we need, check again for more routes
                    if (t.Routes.Count < maxStops)
                    {
                        //otherwise, come on ride that train!
                        var rt2 = GetRoundTrips(destination).OrderBy(r => r.Routes.Count).ToList();
                        foreach (var r in rt2)
                        {
                            if (t.Routes.Count + r.Routes.Count <= maxStops)
                            {
                                //Add a new a whole new trip
                                var newTrip = new Trip(origin, destination);
                                newTrip.Routes.AddRange(t.Routes);
                                newTrip.Routes.AddRange(r.Routes);

                                //if this trip doesn't exist, then add it
                                if (!trips.Any(x => x.DisplayRoute == newTrip.DisplayRoute)
                                    && !tripsToAdd.Any(x => x.DisplayRoute == newTrip.DisplayRoute))
                                {
                                    tripsToAdd.Add(newTrip);
                                }                                
                            }
                        }
                    }                  
                }

                trips.AddRange(tripsToAdd);
                tripsAdded = tripsToAdd.Count;
            }
       

            //weed out any trips that were already over our max
            trips.RemoveAll(t => t.Routes.Count > maxStops);
            trips = trips.OrderBy(t => t.Routes.Count).ToList();

            if (trips == null)
            {
                throw ERR_NO_ROUTE;
            }
            else
            {
                return trips;
            }
        }


        public List<Trip> GetTripsWithDistanceLessThan(Station origin, Station destination, int maxDistance)
        {
            List<Trip> trips;
            if (origin.StationName == destination.StationName)
            {
                trips = GetRoundTrips(origin).Where(t => t.Distance < maxDistance).ToList();
            }
            else
            {
                trips = GetRankedTrips(origin, destination, 1).Where(t => t.Distance < maxDistance).ToList();
            }
            
            var tripsAdded = -1;            
            //keep riding the train until we bring every route up to a maximum that does not exceed the numToMatch
            //run the loop at least once, then keep running it until we don't add any more trips
            while (trips.Count > 0 && tripsAdded != 0)
            {
                tripsAdded = 0;
                var tripsToAdd = new List<Trip>();
                foreach (var t in trips)
                {
                    //if we're less than the distance we need, check again for more routes
                    //if (t.Distance < maxDistance)
                    {
                        //otherwise, come on ride that train!
                        var rt2 = GetRoundTrips(destination).OrderBy(r => r.Distance).ToList();                        
                        foreach (var r in rt2)
                        {
                            if (t.Distance + r.Distance < maxDistance)
                            {
                                //Add a new a whole new trip
                                var newTrip = new Trip(origin, destination);
                                newTrip.Routes.AddRange(t.Routes);
                                newTrip.Routes.AddRange(r.Routes);

                                //if this trip doesn't exist, then add it
                                if (!trips.Any(x => x.DisplayRoute == newTrip.DisplayRoute)
                                    && !tripsToAdd.Any(x => x.DisplayRoute == newTrip.DisplayRoute)) {
                                    tripsToAdd.Add(newTrip);
                                }
                            }
                        }
                    }                    
                }

                trips.AddRange(tripsToAdd);
                tripsAdded = tripsToAdd.Count;
            }
         
            //weed out any trips that were already over our max
            trips.RemoveAll(t => t.Distance >= maxDistance);
            trips = trips.OrderBy(t => t.Routes.Count).ToList();

            if (trips == null)
            {
                throw ERR_NO_ROUTE;
            }
            else
            {
                return trips;
            }
        }


        public List<Trip> GetTripsWithNumberOfStops(Station origin, Station destination, int numStops)
        {
            List<Trip> trips;
            if (origin.StationName == destination.StationName)
            {
                trips = GetRoundTrips(origin).Where(t => t.Routes.Count() <= numStops).ToList();
            }
            else
            {
                trips = GetRankedTrips(origin, destination, numStops).Where(t => t.Routes.Count() <= numStops).ToList();
            }

            //keep riding the train until every trip is over or is a match
            while (trips.Count > 0 && trips.Any(t => t.Routes.Count < numStops))
            {
                var index = 0;
                foreach (var t in trips)
                {
                    //if we already at the number, we don't need to ride again
                    if (t.Routes.Count != numStops)
                    {
                        //otherwise, come on ride that train!
                        var rt2 = GetRoundTrips(destination).OrderBy(r => r.Routes.Count()).ToList();
                        var match = -1;
                        foreach (var r in rt2)
                        {
                            if (t.Routes.Count + r.Routes.Count < numStops)
                            {
                                match = index;
                            }
                            else if (t.Routes.Count + r.Routes.Count == numStops)
                            {
                                match = index;
                                t.Routes.AddRange(r.Routes);
                                break;
                            }
                            else
                            {
                                //if we haven't found a lesser match, add this one and bomb out
                                if (match == -1)
                                {
                                    t.Routes.AddRange(r.Routes);
                                    break;
                                }
                                else
                                {
                                    t.Routes.AddRange(rt2[match].Routes);
                                }
                            }
                        }
                    }
                    index++;
                }
            }

            //weed out any trips that are not matches
            trips.RemoveAll(t => t.Routes.Count > numStops);
            trips = trips.OrderBy(t => t.Routes.Count).ToList();

            if (trips == null)
            {
                throw ERR_NO_ROUTE;
            }
            else
            {
                return trips;
            }
        }

        public Trip GetTripWithShortestDistance(Station origin, Station destination)
        {
            List<Trip> trips;
            if (origin.StationName == destination.StationName)
            {
                trips = GetRoundTrips(origin).OrderBy(t => t.Distance).ToList();
            }
            else
            {
                trips = GetRankedTrips(origin, destination, 1).OrderBy(t => t.Distance).ToList();
            }

            if (trips == null)
            {
                throw ERR_NO_ROUTE;
            }
            else
            {
                return trips.FirstOrDefault();
            }

        }
    }
}
