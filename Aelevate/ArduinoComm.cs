using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aelevate {
    public class ArduinoComm {
        private static ArduinoComm _instance;
        public static ArduinoComm Instance => _instance ??= new();
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        SerialPort port;

        private ArduinoComm() { }
        
        public void Start() {
            port = new();
            while (SerialPort.GetPortNames().Length == 0) ;
            port.PortName = SerialPort.GetPortNames()[0];
            port.BaudRate = 9600;
            port.Parity = Parity.Even;
            port.Open();
            Logger.Info("Connected to Arduino");
            while (true) {
                Logger.Info(port.ReadLine());
                Thread.Yield();
            }
        }

    }
}
