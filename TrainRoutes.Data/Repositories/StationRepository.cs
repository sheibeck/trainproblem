using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrainRoutes.Domain.Models;
using TrainRoutes.Domain.Repositories;

namespace TrainRoutes.Data.Repositories
{
    class StationRepository : IStationRepository
    {
        public List<Station> GetAllStations()
        {
            return new List<Station>()
            {
                new Station() { StationName = "A" },
                new Station() { StationName = "B" },
                new Station() { StationName = "C" },
                new Station() { StationName = "D" },
                new Station() { StationName = "E" },
            };
        }
    }
}
