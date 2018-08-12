using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sketch2Code.Web.Models
{
    public class ContentViewModel
    {
        public byte[] Image { get; set; }
        public String ImageData { get; set; }

        public string CorrelationId { get; set; }
    }
}