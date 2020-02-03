using System;
using System.Collections.Generic;
using System.Linq;
using TrainRoutes.Data.Repositories;
using TrainRoutes.Domain.Models;
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

        public List<Station> GetAllStations()
        {
            return repository.GetAllStations();
        }

        public bool StationExists(Station station)
        {
            return GetAllStations().Any(x => x.StationName == station.StationName);
        }
    }
}

