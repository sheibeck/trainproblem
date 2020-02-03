using System;
using System.Collections.Generic;
using System.Text;
using TrainRoutes.Domain.Models;

namespace TrainRoutes.Domain.Repositories
{
    public interface IRouteRepository
    {
        IEnumerable<Route> GetAllRoutes();

        Route GetRoute(Station origin, Station destination);
    }
}
