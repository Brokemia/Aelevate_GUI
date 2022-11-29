
using NLog;

namespace Aelevate;

public partial class MainPage : ContentPage {
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
        LockUnlock(null, null);
	}

    private void ResistanceUp(object sender, EventArgs e) {
        routesModel.Resistance++;
        if (routesModel.Locked) {
            routesModel.Tilt = routesModel.Resistance / 4f;
        }
    }

    private void ResistanceDown(object sender, EventArgs e) {
        routesModel.Resistance--;
        if (routesModel.Locked) {
            routesModel.Tilt = routesModel.Resistance / 4f;
        }
    }

    private void TiltUp(object sender, EventArgs e) {
        routesModel.Tilt += 0.5f;
        if (routesModel.Locked) {
            routesModel.Resistance = (int)(routesModel.Tilt * 4);
        }
    }

    private void TiltDown(object sender, EventArgs e) {
        routesModel.Tilt -= 0.5f;
        if (routesModel.Locked) {
            routesModel.Resistance = (int)(routesModel.Tilt * 4);
        }
    }

    private void LockUnlock(object sender, EventArgs e) {
        routesModel.Locked = !routesModel.Locked;
        if(routesModel.Locked) {
            routesModel.Resistance = (int)(routesModel.Tilt * 4);
        }
    }
}

