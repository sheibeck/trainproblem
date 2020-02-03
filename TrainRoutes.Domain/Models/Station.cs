using System;
using System.Collections.Generic;
using System.Text;

namespace TrainRoutes.Domain.Models
{
    public class Station
    {   
        public Station(string stationName)
        {
            this.StationName = stationName.ToUpper();
        }

        public string StationName { get; set; }
    }
}
