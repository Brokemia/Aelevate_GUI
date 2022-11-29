
namespace Aelevate {
    public class LinearRouteSegment : RouteSegment {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private float resistanceStart;
        private float tiltStart;

        private float resistanceSlope;
        private float tiltSlope;

        public LinearRouteSegment(float resistanceStart, float resistanceEnd, float tiltStart, float tiltEnd, int length) : base(length) {
            this.resistanceStart = resistanceStart;
            this.tiltStart = tiltStart;
            resistanceSlope = (resistanceEnd - resistanceStart) / length;
            tiltSlope = (tiltEnd - tiltStart) / length;
        }
        
        public override float GetResistance(int time) {
            return time * resistanceSlope + resistanceStart;
        }

        public override float GetTilt(int time) {
            return time * tiltSlope + tiltStart;
        }
    }
}
