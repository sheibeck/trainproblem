using System;
using System.Collections.Generic;
using System.Text;
using TrainRoutes.Domain.Models;

namespace TrainRoutes.Domain.Repositories
{
    public interface IStationRepository
    {
        List<Station> GetAllStations();       
    }
}
