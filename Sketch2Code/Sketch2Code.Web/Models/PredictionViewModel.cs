using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sketch2Code.Web.Models
{
    public class PredictionViewModel
    {
        public string PredictionName { get; private set; }
        
        public DateTime LastModified { get; private set; }
        public PredictionViewModel(string name, DateTime? lastModified)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");

            if (!lastModified.HasValue)
                throw new ArgumentNullException("lastModified");

            if (DateTime.MinValue == lastModified)
                throw new ArgumentOutOfRangeException("lastModified");

            PredictionName = name;
            LastModified = lastModified.Value;
        }
    }
}