

using Font = Microsoft.Maui.Graphics.Font;

namespace Aelevate {
    public class SpeedGraph : IDrawable {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private const int SPEED_COUNT = 20;
        private float maxSpeed = 0;
        private readonly List<float> pastSpeeds = new() { 0 }; // TODO fix

        public void AddSpeed(float speed) {
            pastSpeeds.Insert(0, speed);
            if(pastSpeeds.Count > SPEED_COUNT) {
                pastSpeeds.RemoveAt(SPEED_COUNT);
            }
            maxSpeed = pastSpeeds.Max();
        }

        public void Draw(ICanvas canvas, RectF dirtyRect) {
            canvas.Font = new Font("Consolas");
            canvas.FontSize = 20;
            canvas.FontColor = Color.FromRgb(10, 220, 10);
            canvas.DrawString(string.Format("{0,-2} mph", pastSpeeds[0]), dirtyRect.Left + dirtyRect.Width * 0.02f, dirtyRect.Top + dirtyRect.Height * 0.17f, HorizontalAlignment.Left);
            if (maxSpeed > 0) {
                canvas.StrokeColor = Color.FromRgb(10, 220, 10);
                canvas.StrokeSize = 2.5f;
                float xStep = dirtyRect.Width / (SPEED_COUNT - 1);
                float yStep = dirtyRect.Height / (maxSpeed * 1.3f);
                // Draw lines between up to 20 recent speeds
                for (int i = 0; i < SPEED_COUNT && i < pastSpeeds.Count - 1; i++) {
                    canvas.DrawLine(
                        dirtyRect.Left + xStep * i, dirtyRect.Bottom - yStep * pastSpeeds[i],
                        dirtyRect.Left + xStep * (i + 1), dirtyRect.Bottom - yStep * pastSpeeds[i + 1]
                    );
                }
            }

            canvas.StrokeSize = 5;
            canvas.StrokeColor = Color.FromRgb(60, 150, 60);
            canvas.DrawRectangle(dirtyRect);
        }
    }
}
