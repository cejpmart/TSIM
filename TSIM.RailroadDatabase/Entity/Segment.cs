using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TSIM.Model;

namespace TSIM.RailroadDatabase.Entity
{
    public class Segment
    {
        public int SegmentId { get; set; }
        public SegmentType Type { get; set; }
        public ICollection<SegmentControlPoint> ControlPoints { get; set; }

        private Segment()
        {
        }

        public Segment(Model.Segment segment)
        {
            Type = segment.Type;
            ControlPoints = new List<SegmentControlPoint>();

            foreach (var cp in segment.ControlPoints)
            {
                ControlPoints.Add(new Entity.SegmentControlPoint(cp));
            }
        }

        public Model.Segment ToModel()
        {
            var cp = ControlPoints.ToArray();

            Trace.Assert(cp.Length == 2);
            return new Model.Segment(Type, cp[0].ToVector3(), cp[1].ToVector3());
        }
    }
}
