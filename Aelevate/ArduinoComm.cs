using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aelevate {
    public class ArduinoComm {
        public class ArduinoCommand {
            public char ID;
            public virtual void Write(SerialPort port) {
                port.Write(new byte[] { 255, (byte)ID }, 0, 2);
            } 
        }

        public class SetResistanceCommand : ArduinoCommand {
            // Value from 0 to 254
            public byte Resistance;
            public SetResistanceCommand(byte res) {
                ID = 'r';
                Resistance = res;
            }

            public override void Write(SerialPort port) {
                base.Write(port);
                port.Write(new byte[] { Resistance }, 0, 1);
            }
        }

        public class SetTiltCommand : ArduinoCommand {
            // Value from 0 to 254
            public byte Tilt;
            public SetTiltCommand(byte tilt) {
                ID = 'a';
                Tilt = tilt;
            }

            public override void Write(SerialPort port) {
                base.Write(port);
                port.Write(new byte[] { Tilt }, 0, 1);
            }
        }

        private static ArduinoComm _instance;
        public static ArduinoComm Instance => _instance ??= new();
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        // Must lock while using
        private Queue<ArduinoCommand> queuedCommands = new();

        SerialPort port;

        public SpeedGraph SpeedGraph { get; set; }
        public MainPage View { get; set; }

        private ArduinoComm() { }
        
        public void Start() {
            while (true) {
                try {
                    Routine();
                } catch (Exception e) {
                    Logger.Error(e.StackTrace);
                }
            }
        }

        bool firstSpeedData = true;
        uint lastEncoder;
        uint lastTime;

        object speedLock = new();
        float speed;

        public void Routine() {
            //int test = 0;
            //while(true) {
            //    SpeedGraph.Speed = 10 + 5 * (float)Math.Sin(test / 20);
            //    View.InvalidateSpeedGraph();
            //    test++;
            //    Thread.Sleep(100);
            //}

            port = new();
            while (SerialPort.GetPortNames().Length == 0) Thread.Sleep(10);
            port.PortName = SerialPort.GetPortNames()[0];
            port.BaudRate = 9600;
            port.Parity = Parity.Even;
            port.Open();
            Logger.Info("Connected to Arduino");
            // Stop arduino data output
            new ArduinoCommand { ID = 'y' }.Write(port);
            Thread.Sleep(500);
            // Exhaust any waiting data
            port.ReadExisting();
            // Start arduino data output
            new ArduinoCommand { ID = 'p' }.Write(port);

            firstSpeedData = true;

            Task.Run(() => {
                while(true) {
                    Thread.Sleep(700);
                    lock(speedLock) {
                        SpeedGraph.AddSpeed(speed);
                        View.InvalidateSpeedGraph();
                        speed = 0;
                    }
                }
            });

            while (true) {
                lock (queuedCommands) {
                    while (queuedCommands.Count > 0) {
                        queuedCommands.Dequeue().Write(port);
                    }
                }
                //if(port.BytesToRead > 0)
                //    Logger.Info(port.BytesToRead);
                if (port.BytesToRead > 0) {
                    uint encoder = ReadUInt();
                    uint time = ReadUInt();
                    if(!firstSpeedData) {
                        const double rotationsPerRotation = 23.25f;
                        const double wheelRad = 26 / 24f; // In feet
                        const double wheelCircumference = 2 * wheelRad * Math.PI;
                        double deltaEncoder = (encoder - lastEncoder) / rotationsPerRotation * wheelCircumference;
                        uint deltaTime = time - lastTime;
                        Logger.Info("Miles: " + (deltaEncoder / 5200) + " per hours: " + ((double)deltaTime / (1000 * 60 * 60)));
                        // In miles per hour
                        lock (speedLock) {
                            speed = (float)deltaEncoder / deltaTime * 1000 * 60 * 60 / 5280;
                        }
                    }
                    firstSpeedData = false;
                    lastEncoder = encoder;
                    lastTime = time;
                }
                Thread.Yield();
            }
        }

        private uint ReadUInt() {
            byte[] bytes = new byte[sizeof(uint)];
            string log = "";
            for(int i = 0; i < bytes.Length; i++) {
                while (port.BytesToRead <= 0) Thread.Yield();
                bytes[i] = (byte)port.ReadByte();
                log += bytes[i] + " ";
            }
            
            return BitConverter.ToUInt32(bytes);
        }

        public void ResetTrainer() {
            lock (queuedCommands) {
                queuedCommands.Enqueue(new ArduinoCommand() { ID = 's' });
            }
        }

        /// <summary>
        /// Queues a command to set the resistance for the trainer
        /// </summary>
        /// <param name="resistance">a resistance value scaled from 0 to 254 (inclusive)</param>
        public void SetResistance(byte resistance) {
            if(resistance > 254) {
                throw new ArgumentOutOfRangeException(nameof(resistance), "Resistance must be between 0 and 254");
            }
            lock (queuedCommands) {
                queuedCommands.Enqueue(new SetResistanceCommand(resistance));
            }
        }

        /// <summary>
        /// Queues a command to set the tilt for the trainer
        /// </summary>
        /// <param name="tilt">a tilt value scaled from 0 to 254 (inclusive)</param>
        public void SetTilt(byte tilt) {
            if(tilt > 254) {
                throw new ArgumentOutOfRangeException(nameof(tilt), "Tilt must be between 0 and 254");
            }
            lock (queuedCommands) {
                queuedCommands.Enqueue(new SetTiltCommand(tilt));
            }
        }

    }
}
