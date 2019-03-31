using System;
using System.Diagnostics;
using TSIM.Model;
using TSIM.RailroadDatabase;

namespace TSIM
{
    public class Simulation
    {
        public TimeSpan TimeElapsed { get; private set; }

        public SimulationCoordinateSpace CoordSpace { get; private set; }
        public INetworkDatabase Network { get; private set; }
        public IUnitDatabase Units { get; private set; }

        public int?[] currentSegmentByUnitId;
        public SegmentEndpoint[] dirByUnitId;
        public float[] tByUnitId;

        public Simulation(SimulationCoordinateSpace coordSpace, INetworkDatabase network, IUnitDatabase units)
        {
            CoordSpace = coordSpace;
            Network = network;
            Units = units;

            // TODO: init currentSegmentByUnitId, tByUnitId
            currentSegmentByUnitId = new int?[] {1};
            dirByUnitId = new SegmentEndpoint[] {SegmentEndpoint.End};
            tByUnitId = new float[] { 0.5f };
        }

        public void Step(double dt)
        {
            for (var unitIndex = 0; unitIndex < Units.GetNumUnits(); unitIndex++)
            {
                var unit = Units.GetUnitByIndex(unitIndex);

                // Update unit position based on velocity
                // If unit is on rail, it should stay snapped
                var distanceToTravel = unit.Velocity.Length();

                // Find out in which segment we are and how far along
                var segId = currentSegmentByUnitId[unitIndex].Value;
                var seg = Network.GetSegmentById(segId);
                var t = tByUnitId[unitIndex];
                var dir = dirByUnitId[unitIndex];

                while (distanceToTravel > Single.Epsilon)
                {
                    // Find out how far further we can travel in the current segment
                    // For now, assume linear segments (no curvature)
                    // If we are travelling in the positive direction, that will be (1-t)*length
                    // If we are travelling in the negative direction, that will be t*length
                    float travellableDistance;
                    float tDir;

                    var segLength = seg.GetLength();

                    if (dir == SegmentEndpoint.End)
                    {
                        travellableDistance = (1 - t) * segLength;
                        tDir = 1;
                    }
                    else
                    {
                        travellableDistance = t * segLength;
                        tDir = -1;
                    }

                    if (travellableDistance > distanceToTravel)
                    {
                        // Only works if ds/dt is uniform for curve!!!
                        t = (float)(t + tDir * distanceToTravel / segLength * dt);

//                        Console.WriteLine($"Unit {unitIndex} update: continue segment segId={segId} dir={dir} t={t}");
                        break;
                    }
                    else
                    {
                        distanceToTravel -= travellableDistance;

                        // We have reached the end of the current segment. Find a connecting segment
                        var candidates = Network.FindConnectingSegments(segId, dir);

                        if (candidates.Length == 0)
                        {
                            throw new NotImplementedException("Ran out of track and cannot cope.");
                        }

                        if (candidates.Length > 1)
                        {
                            throw new NotImplementedException("Cannot currently handle track splits");
                        }

                        // Only one candidate left. Find out more.
                        if (candidates[0].Segment1 == segId && candidates[0].Ep1 == dir)
                        {
                            segId = candidates[0].Segment2;
                            t = (candidates[0].Ep2 == SegmentEndpoint.Start ? 0 : 1);
                            dir = (candidates[0].Ep2 == SegmentEndpoint.Start ? SegmentEndpoint.End : SegmentEndpoint.Start);
                        }
                        else if (candidates[0].Segment2 == segId && candidates[0].Ep2 == dir)
                        {
                            segId = candidates[0].Segment1;
                            t = (candidates[0].Ep1 == SegmentEndpoint.Start ? 0 : 1);
                            dir = (candidates[0].Ep1 == SegmentEndpoint.Start ? SegmentEndpoint.End : SegmentEndpoint.Start);
                        }
                        else
                        {
                            Trace.Assert(false);
                        }

                        seg = Network.GetSegmentById(segId);
//                        Console.WriteLine($"Unit {unitIndex} update: new segment segId={segId} dir={dir} t={t}");
                    }
                }

                // Update unit position, velocity & orientation based on where we end up

                var (pos, headingDir) = seg.GetPointAndTangent(t, dir);
                unit.Pos = pos;
                unit.Velocity = headingDir * unit.Velocity.Length();
                unit.Orientation = Utility.DirectionVectorToQuaternion(headingDir);

                Units.UpdateUnitByIndex(unitIndex, unit);
                currentSegmentByUnitId[unitIndex] = segId;
                dirByUnitId[unitIndex] = dir;
                tByUnitId[unitIndex] = t;

//                Console.WriteLine($"Unit {unitIndex} update: pos {unit.Pos} velocity {unit.Velocity}");
            }

            TimeElapsed += TimeSpan.FromSeconds(dt);
        }
    }
}
