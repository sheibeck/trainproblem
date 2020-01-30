using Moq;
using System;
using System.Collections.Generic;
using TrainRoutes.Domain.Models;
using TrainRoutes.Domain.Repositories;
using TrainRoutes.Services;
using TrainRoutes.Services.Abstract;
using Xunit;

namespace TrainRoutes.Tests
{
    public class RouteServiceTests : IClassFixture<TrainRoutesTestsFixture>
    {
        private readonly TrainRoutesTestsFixture _fixture;
        private readonly RouteService routeService;

        public RouteServiceTests()
        {
            _fixture = new TrainRoutesTestsFixture();
            routeService = new RouteService(_fixture.MockRepository.Object, _fixture.MockStationService.Object);
        }

        [Fact]
        public void GetTripByStations_Should_Return_Distance_If_Valid()
        {
            _fixture.MockRepository.SetupSequence(r => r.GetRoute(It.IsAny<Station>(), It.IsAny<Station>()))
                .Returns(new Route() { RouteId = 6, Origin = "A", Destination = "D", Distance = 5 })
                .Returns(new Route() { RouteId = 4, Origin = "D", Destination = "C", Distance = 8 });

            var stations = new List<Station>()
            {
                new Station() { StationName = "A" },
                new Station() { StationName = "D" },
                new Station() { StationName = "C" },
            };

            var trip = routeService.GetTripByStations(stations);
            var tripDistance = trip.Distance;
            var expectedDistance = 13;

            Assert.IsType<Trip>(trip);
            Assert.Equal(expectedDistance, tripDistance);
        }

        [Fact]
        public void GetTripByStations_Should_Throw_Exception_If_Invalid()
        {
            _fixture.MockRepository.SetupSequence(r => r.GetRoute(It.IsAny<Station>(), It.IsAny<Station>()))
                .Returns(new Route() { RouteId = 6, Origin = "A", Destination = "D", Distance = 5 })
                .Returns(new Route() { RouteId = 4, Origin = "D", Destination = "C", Distance = 8 });

            var stations = new List<Station>()
            {
                new Station() { StationName = "Z" },
                new Station() { StationName = "Y" },
            };

            Assert.Throws<InvalidOperationException>(() => routeService.GetTripByStations(stations));            
        }
    }

    class TrainRoutesTestsFixture
    {
        public Mock<IRouteRepository> MockRepository { get; }
        public Mock<IStationService> MockStationService { get; }

        public TrainRoutesTestsFixture()
        {
            MockRepository = new Mock<IRouteRepository>();
            MockStationService = new Mock<IStationService>();
            MockStationService.Setup(s => s.GetAllStations()).Returns(new List<Station>()
            {
                new Station() { StationName = "A" },
                new Station() { StationName = "B" },
                new Station() { StationName = "C" },
                new Station() { StationName = "D" },
                new Station() { StationName = "E" },
            });
        }
    }
}
