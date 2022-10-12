namespace Aelevate;

public partial class MainPage : ContentPage {
	const int MAX_DIFF = 10;
	const int MIN_DIFF = 0;
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

	int _difficulty = 0;
    int Difficulty {
		get => _difficulty;
		set {
			_difficulty = value;
            difficultyDisplay.Text = value.ToString();
			Tilt = _difficulty / 2f;
			Resistance = _difficulty * 2;
        }
	}

	public MainPage() {
		InitializeComponent();
		Difficulty = 0;
	}

	private void DifficultyUp(object sender, EventArgs e) {
		Difficulty = Math.Min(Difficulty + 1, MAX_DIFF);
	}

    private void DifficultyDown(object sender, EventArgs e) {
        Difficulty = Math.Max(Difficulty - 1, MIN_DIFF);
    }

    private void ResistanceUp(object sender, EventArgs e) {
        Resistance = Math.Min(Resistance + 1, MAX_RES);
    }

    private void ResistanceDown(object sender, EventArgs e) {
        Resistance = Math.Max(Resistance - 1, MIN_RES);
    }

    private void TiltUp(object sender, EventArgs e) {
        Tilt = Math.Min(Tilt + 0.5f, MAX_TILT);
    }

    private void TiltDown(object sender, EventArgs e) {
        Tilt = Math.Max(Tilt - 0.5f, MIN_TILT);
    }

    private void AngleChanged(object sender, ValueChangedEventArgs e) {
		Tilt = (int)e.NewValue;
	}
}

