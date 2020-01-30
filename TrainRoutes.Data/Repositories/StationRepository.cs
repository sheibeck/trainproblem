using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrainRoutes.Domain.Repositories;

namespace TrainRoutes.Data.Repositories
{
    class StationRepository : IStationRepository
    {
        public List<string> GetAllStations()
        {
            return new List<string>()
            {
                "A", "B", "C", "D", "E"
            };
        }               
    }
}
