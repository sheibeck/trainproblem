using System.Collections.Generic;
using System.Linq;

namespace TrainRoutes.Domain.Models
{
    public class Trip
    {
        public Trip(Station origin, Station destination)
        {
            this.Origin = origin;
            this.Destination = destination;
            this.Routes = new List<Route>();
        }

        public Station Origin { get; set; }
        public Station Destination { get; set; }

        public List<Route> Routes { get; set; }

        public int Distance {
            get {
                return this.Routes.Sum(r => r.Distance);
            }
        }

        public string DisplayRoute
        {
            get
            {
                string result = "";
                foreach (var r in this.Routes)
                {
                    result += $"{r.Origin}";
                }
                result += this.Routes.Last().Destination;
                return result;
            }
        }
    }
}
