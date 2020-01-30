using System;
using System.Collections.Generic;
using System.Linq;
using TrainRoutes.Data.Repositories;
using TrainRoutes.Domain.Repositories;
using TrainRoutes.Services.Abstract;

namespace TrainRoutes.Services
{
    public class StationService: IStationService
    {
        private readonly IStationRepository repository;

        public StationService(IStationRepository Repository) : base()
        {
            repository = Repository;
        }

        public List<string> GetAllStations()
        {
            return repository.GetAllStations();
        }

        public bool StationExists(string stationName)
        {
            return GetAllStations().Any(x => x.Contains(stationName));
        }
    }
}

