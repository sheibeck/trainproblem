using Moq;
using System;
using System.Collections.Generic;
using TrainRoutes.Domain.Models;
using TrainRoutes.Domain.Repositories;
using TrainRoutes.Services;
using Xunit;

namespace TrainRoutes.Tests
{
    public class StationServiceTests : IClassFixture<StationServiceTestsFixture>
    {
        private readonly StationServiceTestsFixture _fixture;
        private readonly StationService stationService;

        public StationServiceTests()
        {
            _fixture = new StationServiceTestsFixture();
            stationService = new StationService(_fixture.MockRepository.Object);
        }

        [Fact]
        public void GetAllStations_Should_Return_ListOfStations()
        {
            var result = stationService.GetAllStations();
            var actual = result.Count;
            var expected = 5;

            Assert.IsType<List<Station>>(result);
            Assert.Equal(expected, actual);
        }        
    }

    class StationServiceTestsFixture
    {
        public Mock<IStationRepository> MockRepository { get; }

        public StationServiceTestsFixture()
        {
            MockRepository = new Mock<IStationRepository>();
            MockRepository.Setup(s => s.GetAllStations()).Returns(new List<Station>()
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

