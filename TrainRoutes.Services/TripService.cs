using QuickGraph;
using QuickGraph.Algorithms;
using QuickGraph.Algorithms.Observers;
using QuickGraph.Algorithms.RankedShortestPath;
using QuickGraph.Algorithms.Search;
using QuickGraph.Algorithms.ShortestPath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
                return trips;
            }
        }

        private List<Trip> GetAllTrips(Station origin, Station destination, int? pathCount = 1)
        {
            var edgeCost = AlgorithmExtensions.GetIndexer(routeDistance);
            // Positive or negative weights
            TryFunc<string, System.Collections.Generic.IEnumerable<Edge<string>>> tryGetPath = routeGraph.ShortestPathsBellmanFord(edgeCost, origin.StationName);

            IEnumerable<Edge<string>> path;
            if (tryGetPath(destination.StationName, out path))
            {
                Console.Write("Path found from {0} to {1}: {0}", origin.StationName, destination.StationName);
                foreach (var e in path) { Console.Write(" > {0}", e.Target); }
                Console.WriteLine();
            }
            else { Console.WriteLine("No path found from {0} to {1}."); }

            return null;
        }


        private List<Trip> GetRoundTrips(Station origin)
        {
            var stationsLeadingBackToOrigin = routeRepository.GetAllRoutes().Where(r => r.Destination == origin.StationName);

            var trips = new List<Trip>();
            // now find all the paths from origin to these stations
            foreach (var r in stationsLeadingBackToOrigin)
            {
                var originOfReturnTrip = new Station(r.Origin);
                var tripBackToOrigin = GetRankedTrips(originOfReturnTrip, origin, 1);

                if(tripBackToOrigin !=null)
                foreach (var rtrip in tripBackToOrigin)
                {
                    var tripFromOrigin = GetRankedTrips(origin, originOfReturnTrip, 1);

                    if (tripFromOrigin != null)
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


        public List<Trip> GetAllPaths(Station origin, Station destination)
        {
            //reset the list of trips
            var tripList = new List<Trip>();

            Dictionary<string, bool> isVisited = new Dictionary<string, bool>();
            foreach(var v in routeGraph.Vertices)
            {
                isVisited.Add(v, false);
            };

            List<string> pathList = new List<string>();
            

            // add source to path[]  
            pathList.Add(origin.StationName);

            var maxDistance = 30;

            // Call recursive utility  
            recursePaths(origin.StationName, destination.StationName, isVisited, pathList, tripList, maxDistance);

            //now take the trip list and find the final route and see if that route also leads to the destination            
            if (tripList != null)
            {
                //keep riding until we reach go over the max distance               
                foreach (var t in tripList)
                {                                        
                    var additionalTrips = new List<Trip>();
                    recursePaths(t.Routes.Last().Destination, destination.StationName, isVisited, pathList, additionalTrips, maxDistance);

                    if (additionalTrips.Count() > 0)
                    {
                        foreach (var at in additionalTrips)
                        {
                            foreach (var ar in at.Routes)
                            {
                                if (t.Distance + ar.Distance < maxDistance) {
                                    t.Routes.AddRange(at.Routes);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }                
            }

            return tripList;
        }

        
        private void recursePaths(string u, string d, Dictionary<string, bool> isVisited, List<string> localPathList, List<Trip> tripList, int maxDistance)
        {
            // Mark the current node            
            isVisited[u] = true;

            if (u.Equals(d))
            {
                var trip = new Trip(new Station(localPathList.First()), new Station(localPathList.Last()));                
                for (var i = 0; i <= localPathList.Count() - 2; i++)
                {
                    var route = routeRepository.GetRoute(new Station(localPathList[i]), new Station(localPathList[i + 1]));
                    if (route != null)
                    {
                        if (trip.Distance + route.Distance < maxDistance)
                        {
                            trip.Routes.Add(route);
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        // if the route isn't valid then abandon this trip
                        trip = null;
                        break;
                    }
                }

                if (trip != null) {
                    tripList.Add(trip);
                }

                // if match found then no need  
                // to traverse more till depth  
                isVisited[u] = false;
                return;
            }

            // Recur for all the vertices  
            // adjacent to current vertex  
            foreach (string i in routeGraph.Vertices)
            {
                if (!isVisited[i])
                {
                    // store current node  
                    // in path[]  
                    localPathList.Add(i);
                    recursePaths(i, d, isVisited,
                                        localPathList, tripList, maxDistance);

                    // remove current node  
                    // in path[]  
                    localPathList.Remove(i);
                }
            }

            // Mark the current node  
            isVisited[u] = false;
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

            ComeOnRideThatTrain(origin, destination, maxDistance, trips, TripFilterEnum.DistanceLessThan);

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
            
            ComeOnRideThatTrain(origin, destination, numStops, trips, TripFilterEnum.ExactStops);

            

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

        private enum TripFilterEnum
        {
            ExactStops,            
            DistanceLessThan,
        }

        /// <summary>
        /// We already have a trip that ends at the destination, but we will keep riding the train 
        /// from the destination back to the destination until we find what we need or eliminate all the routes
        /// </summary>
        /// <param name="trip"></param>
        private void ComeOnRideThatTrain(Station origin, Station destination, int numToMatch, List<Trip> trips, TripFilterEnum filter)
        {           
            if (filter == TripFilterEnum.ExactStops)
            {
                //keep riding the train until every trip is over or is a match
                while (trips.Count > 0 && trips.Any(t => t.Routes.Count < numToMatch))
                {
                    var index = 0;
                    foreach (var t in trips)
                    {
                        //if we already at the number, we don't need to ride again
                        if (t.Routes.Count != numToMatch)
                        {
                            //otherwise, come on ride that train!
                            var rt2 = GetRoundTrips(destination).OrderBy(r => r.Routes.Count()).ToList();
                            var match = -1;
                            foreach (var r in rt2)
                            {
                                if (t.Routes.Count + r.Routes.Count < numToMatch)
                                {
                                    match = index;
                                }
                                else if (t.Routes.Count + r.Routes.Count == numToMatch)
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
                trips.RemoveAll(t => t.Routes.Count > numToMatch);
            }

            if (filter == TripFilterEnum.DistanceLessThan)
            {                
                var index = 0;
                var tripsToAdd = new List<Trip>();
                //keep riding the train until we bring every route up to a maximum that does not exceed the numToMatch

                //TODO: Keep running this until every new trip would be over the maximum

                while (trips.Count > 0 && index < trips.Count)
                {                    
                    foreach (var t in trips)
                    {
                        //if we already at the number in excess then don't check it
                        if (t.Distance < numToMatch)
                        {
                            //otherwise, come on ride that train!
                            var rt2 = GetRoundTrips(destination).OrderBy(r => r.Distance).ToList();                            
                            foreach (var r in rt2)
                            {
                                if (t.Distance + r.Distance < numToMatch)
                                {
                                    //Add a new a whole new trip
                                    var newTrip = new Trip(origin, destination);
                                    newTrip.Routes.AddRange(t.Routes);
                                    newTrip.Routes.AddRange(r.Routes);
                                    tripsToAdd.Add(newTrip);
                                }                                
                            }
                        }
                        index++;
                    }
                }

                if (tripsToAdd.Count > 0) {
                    trips.AddRange(tripsToAdd);
                }
                //weed out any trips that were already over our max
                trips.RemoveAll(t => t.Distance >= numToMatch);
            }
        }       
    }
}
