
using Sketch2Code.Core.BoxGeometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sketch2Code.Core.Entities
{
    public class PredictionDetail
    {
        public IList<PredictedObject> PredictedObjects { get; set; }
        public byte[] OriginalImage { get; set; }
        public byte[] PredictionImage { get; set; }
        public IList<GroupBox> GroupBox { get; set; }
        public IList<string> AllClasses
        {
            get
            {
                var result = new List<string>();
                if(this.PredictedObjects!=null && this.PredictedObjects.Any())
                {
                    result = this.PredictedObjects.Select(p => p.ClassName).Distinct().ToList();
                }
                return result;
            }
        }
    }
}
