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

        private ArduinoComm() { }
        
        public void Start() {
            while (true) {
                try {
                    Routine();
                } catch (Exception) { }
            }
        }

        public void Routine() {
            port = new();
            while (SerialPort.GetPortNames().Length == 0) Thread.Sleep(10);
            port.PortName = SerialPort.GetPortNames()[0];
            port.BaudRate = 9600;
            port.Parity = Parity.Even;
            port.Open();
            Logger.Info("Connected to Arduino");
            // Stop arduino data output
            new ArduinoCommand { ID = 'P' }.Write(port);
            Thread.Sleep(500);
            // Exhaust any waiting data
            port.ReadExisting();
            // Start arduino data output
            new ArduinoCommand { ID = 'p' }.Write(port);

            while (true) {
                lock (queuedCommands) {
                    while (queuedCommands.Count > 0) {
                        queuedCommands.Dequeue().Write(port);
                    }
                }
                if (port.BytesToRead > 0) {
                    byte[] bytes = new byte[sizeof(ulong)];
                    port.Read(bytes, 0, bytes.Length);
                    ulong encoder = BitConverter.ToUInt64(bytes);
                    port.Read(bytes, 0, bytes.Length);
                    ulong time = BitConverter.ToUInt64(bytes);
                }
                Thread.Yield();
            }
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
