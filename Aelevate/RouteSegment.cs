using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aelevate {
    public abstract class RouteSegment {
        // Thirds of a second
        public int Length { get; private set; }

        public RouteSegment(int length) {
            Length = length;
        }

        public abstract float GetResistance(int time);

        public abstract float GetTilt(int time);
    }
}
