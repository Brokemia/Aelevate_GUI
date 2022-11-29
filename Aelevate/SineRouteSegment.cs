
namespace Aelevate {
    public class SineRouteSegment : RouteSegment {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private float resistanceCenter;
        private float resistanceRange;

        private float tiltCenter;
        private float tiltRange;

        // Cycles per "tick"
        private float frequency;

        public SineRouteSegment(float resistanceMin, float resistanceMax, float tiltMin, float tiltMax, int wavelength, int length) : base(length) {
            resistanceCenter = (resistanceMax + resistanceMin) / 2;
            resistanceRange = (resistanceMax - resistanceMin) / 2;
            tiltCenter = (tiltMax + tiltMin) / 2;
            tiltRange = (tiltMax - tiltMin) / 2;
            frequency = (float)(2 * Math.PI * (1f / wavelength));
        }
        
        public override float GetResistance(int time) {
            return resistanceRange * (float)Math.Sin(time * frequency) + resistanceCenter;
        }

        public override float GetTilt(int time) {
            return tiltRange * (float)Math.Sin(time * frequency) + tiltCenter;
        }
    }
}
