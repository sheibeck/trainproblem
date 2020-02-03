using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TrainRoutes.Domain.Models;

namespace TrainRoutes.UI.Data
{
    public class RouteDistanceModel
    {

        [Required]
        //[StringLength(1, ErrorMessage = "You must specify at least 2 stations.")]        
        public string Trip { get; set; }
    }
}
