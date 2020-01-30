using System;
using System.Collections.Generic;
using System.Text;
using TrainRoutes.Domain.Models;

namespace TrainRoutes.Services.Abstract
{
    public interface IStationService
    {
        List<Station> GetAllStations();
        bool StationExists(Station station);
    }
}
