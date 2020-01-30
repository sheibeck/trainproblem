using System;
using System.Collections.Generic;
using System.Text;

namespace TrainRoutes.Services.Abstract
{
    interface IRouteService
    {
        int GetRouteDistance(string route);
    }
}
