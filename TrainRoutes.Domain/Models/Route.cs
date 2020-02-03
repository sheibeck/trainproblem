using System;
using System.Collections.Generic;
using System.Text;

namespace TrainRoutes.Domain.Models
{
    public class Route
    {
        public int RouteId { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public int Distance { get; set; }       
    }
}
