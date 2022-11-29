namespace Aelevate {
    public class ConstantRouteSegment : RouteSegment {
        public float Resistance { get; set; }
        public float Tilt { get; set; }

        public ConstantRouteSegment(float resistance, float tilt, int length) : base(length) {
            Resistance = resistance;
            Tilt = tilt;
        }
        
        public override float GetResistance(int time) {
            return Resistance;
        }

        public override float GetTilt(int time) {
            return Tilt;
        }
    }
}
