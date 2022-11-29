
using Microsoft.Maui.Controls.PlatformConfiguration;
using System.Collections.Generic;

namespace Aelevate {
    public class SmartRandomRouteSegment : RouteSegment {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private float[] resistances;
        private float[] tilts;
        
        public SmartRandomRouteSegment(float resistanceMin, float resistanceMax, float tiltMin, float tiltMax, int frequency, int length) : base(length) {
            Random rand = new();

            double[] slopeAt = new double[length / frequency + 2];
            for (int i = 0; i < slopeAt.Length; i++) {
                slopeAt[i] = rand.NextDouble() * 2 - 1;
            }

            resistances = new float[length];
            for(int i = 0; i < length; i++) {
                resistances[i] = resistanceMin + (samplePerlin(slopeAt, i / (float)frequency) + 0.5f) * (resistanceMax - resistanceMin);
            }

            // Tilts
            for (int i = 0; i < slopeAt.Length; i++) {
                slopeAt[i] = rand.NextDouble() * 2 - 1;
            }

            tilts = new float[length];
            for (int i = 0; i < length; i++) {
                tilts[i] = tiltMin + (samplePerlin(slopeAt, i / (float)frequency) + 0.5f) * (tiltMax - tiltMin);
            }
        }

        private float samplePerlin(double[] slopes, float x) {
            int lo = (int)x;
            int hi = lo + 1;
            float dist = x - lo;
            double loSlope = slopes[lo];
            double hiSlope = slopes[hi];
            double loPos = loSlope * dist;
            double hiPos = -hiSlope * (1 - dist);
            double u = dist * dist * (3.0 - 2.0 * dist);  // cubic curve
            return (float)((loPos * (1 - u)) + (hiPos * u));  // interpolate
        }
        
        public override float GetResistance(int time) {
            return resistances[time];
        }

        public override float GetTilt(int time) {
            return tilts[time];
        }
    }
}
