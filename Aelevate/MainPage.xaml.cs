
using NLog;

namespace Aelevate;

public partial class MainPage : ContentPage {
    const int MAX_RES = 20;
    const int MIN_RES = 0;
    const float MAX_TILT = 5;
    const float MIN_TILT = 0;

    int _resistance = 0;
	int Resistance {
		get => _resistance;
		set {
			_resistance = value;
			resistanceDisplay.Text = string.Format("{0}", _resistance);
            speedGraph.AddSpeed(_resistance);
            speedGraphView.Invalidate();

        }
	}

	float _tilt = 0;
	float Tilt {
		get => _tilt;
		set {
			_tilt = value;
            bikeDisplay.Rotation = -value * 3;
			tiltDisplay.Text = string.Format(" {0:F1}°", _tilt);
        }
	}

    bool locked = true;

	public MainPage() {
        var config = new NLog.Config.LoggingConfiguration();

        // Targets where to log to: File and Console
        var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "file.txt" };
        var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

        // Rules for mapping loggers to targets            
        config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

        // Apply config           
        LogManager.Configuration = config;
        new Thread(new ThreadStart(ArduinoComm.Instance.Start)).Start();
		InitializeComponent();
		Tilt = 0;
        Resistance = 0;
        LockUnlock(null, null);
	}

	//private void DifficultyUp(object sender, EventArgs e) {
	//	Difficulty = Math.Min(Difficulty + 1, MAX_DIFF);
	//}

 //   private void DifficultyDown(object sender, EventArgs e) {
 //       Difficulty = Math.Max(Difficulty - 1, MIN_DIFF);
 //   }

    private void ResistanceUp(object sender, EventArgs e) {
        Resistance = Math.Min(Resistance + 1, MAX_RES);
        if (locked) {
            Tilt = Resistance / 4f;
        }
    }

    private void ResistanceDown(object sender, EventArgs e) {
        Resistance = Math.Max(Resistance - 1, MIN_RES);
        if (locked) {
            Tilt = Resistance / 4f;
        }
    }

    private void TiltUp(object sender, EventArgs e) {
        Tilt = Math.Min(Tilt + 0.5f, MAX_TILT);
        if (locked) {
            Resistance = (int)(Tilt * 4);
        }
    }

    private void TiltDown(object sender, EventArgs e) {
        Tilt = Math.Max(Tilt - 0.5f, MIN_TILT);
        if (locked) {
            Resistance = (int)(Tilt * 4);
        }
    }

    private void LockUnlock(object sender, EventArgs e) {
        locked = !locked;
        lockButton.IsVisible = locked;
        unlockButton.IsVisible = !locked;
        if(locked) {
            Resistance = (int)(Tilt * 4);
        }
    }
}

