using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using TrainRoutes.Data.Repositories;
using TrainRoutes.Domain.Models;
using TrainRoutes.Domain.Repositories;
using TrainRoutes.Services;
using TrainRoutes.Services.Abstract;
using Xunit;

namespace TrainRoutes.Tests
{
    public class TripServiceTests : IClassFixture<TrainRoutesTestsFixture>
    {
        private readonly TrainRoutesTestsFixture _fixture;
        private readonly TripService tripService;

        public TripServiceTests()
        {
            _fixture = new TrainRoutesTestsFixture();
            tripService = new TripService(_fixture.MockRepository.Object, _fixture.MockStationService.Object);            
        }

        [Fact]
        public void GetTripDistance_Should_Return_Distance_If_Valid()
        {

            _fixture.MockRepository.SetupSequence(r => r.GetRoute(It.IsAny<Station>(), It.IsAny<Station>()))
                .Returns(new Route() { RouteId = 9, Origin = "A", Destination = "E", Distance = 7 })
                .Returns(new Route() { RouteId = 8, Origin = "E", Destination = "B", Distance = 3 })
                .Returns(new Route() { RouteId = 2, Origin = "B", Destination = "C", Distance = 4 })
                .Returns(new Route() { RouteId = 3, Origin = "C", Destination = "D", Distance = 8 });

       
            var routes = new List<Route>() { 
                new Route() { Origin = "A", Destination = "E" },
                new Route() { Origin = "E", Destination = "B" },
                new Route() { Origin = "B", Destination = "C" },
                new Route() { Origin = "C", Destination = "D" },
            };
          
            var expected = 22;
            var actual = tripService.GetTripDistance(routes);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetTripDistance_Should_Throw_Exception_If_Invalid()
        {
            var routes = new List<Route>() {
                new Route() { Origin = "A", Destination = "E" },
                new Route() { Origin = "E", Destination = "D" },               
            };

            Assert.Throws<Exception>(() => tripService.GetTripDistance(routes));       
        }

        [Fact]
        public void GetShortestTrip_Should_Return_Shortest_Between_Two_Stations()
        {
            _fixture.MockRepository.SetupSequence(r => r.GetRoute(It.IsAny<Station>(), It.IsAny<Station>()))
                .Returns(new Route() { RouteId = 1, Origin = "A", Destination = "B", Distance = 5 })
                .Returns(new Route() { RouteId = 2, Origin = "B", Destination = "C", Distance = 4 }); 
                        

            var origin = new Station("A");
            var destination = new Station("C");
            var trip = tripService.GetShortestTrip(origin, destination);
            var tripDistance = trip.Distance;
            var expectedDistance = 9;

            Assert.IsType<Trip>(trip);
            Assert.Equal(expectedDistance, tripDistance);
        }

        [Fact]
        public void GetTripRoundTripWithMaxStops_Should_Return_Trips_IfValid()
        {
            var origin = new Station("C");
            var destination = new Station("C");
            var trips = tripService.GetRoundTripsWithMaxStops(origin, 3);
            
            var expected = 2;
            var actual = trips.Count;
            
            Assert.Equal(expected, actual);            
        }       
    }

    class TrainRoutesTestsFixture
    {
        public Mock<IRouteRepository> MockRepository { get; }
        public Mock<IStationService> MockStationService { get; }

        public TrainRoutesTestsFixture()
        {
            MockRepository = new Mock<IRouteRepository>();
            MockRepository.Setup(s => s.GetAllRoutes()).Returns(new List<Route>()
            {
                new Route() { RouteId = 1, Origin = "A", Destination = "B", Distance = 5 },
                new Route() { RouteId = 2, Origin = "B", Destination = "C", Distance = 4 },
                new Route() { RouteId = 3, Origin = "C", Destination = "D", Distance = 8 },
                new Route() { RouteId = 4, Origin = "D", Destination = "C", Distance = 8 },
                new Route() { RouteId = 5, Origin = "D", Destination = "E", Distance = 6 },
                new Route() { RouteId = 6, Origin = "A", Destination = "D", Distance = 5 },
                new Route() { RouteId = 7, Origin = "C", Destination = "E", Distance = 2 },
                new Route() { RouteId = 8, Origin = "E", Destination = "B", Distance = 3 },
                new Route() { RouteId = 9, Origin = "A", Destination = "E", Distance = 7 },
            });


            MockStationService = new Mock<IStationService>();
            MockStationService.Setup(s => s.GetAllStations()).Returns(new List<Station>()
            {
                new Station("A"),
                new Station("B"),
                new Station("C"),
                new Station("D"),
                new Station("E")
            });

            
        }
    }
}
