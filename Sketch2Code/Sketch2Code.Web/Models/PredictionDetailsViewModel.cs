using Sketch2Code.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sketch2Code.Web.Models
{
    public class PredictionDetailsViewModel
    {
        public string Name { get; set; }
        public String Image { get; set; }
        public IList<byte[]> SlicedImages { get; set; }
        public PredictionDetail Detail { get; set; }
    }
}