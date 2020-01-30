using System;
using System.Collections.Generic;
using System.Text;

namespace TrainRoutes.Services.Abstract
{
    interface IStationService
    {
        List<string> GetAllStations();
        bool StationExists(string stationName);
    }
}
