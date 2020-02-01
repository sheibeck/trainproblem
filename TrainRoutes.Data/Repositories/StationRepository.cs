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

        //NOTE: Need to drop in a DbContext and hit a real database

        public List<Station> GetAllStations()
        {
            return new List<Station>()
            {
                new Station("A"),
                new Station("B"),
                new Station("C"),
                new Station("D"),
                new Station("E")
            };
        }
    }
}
