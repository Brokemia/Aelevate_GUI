using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aelevate {
    public class Route : INotifyPropertyChanged {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public event PropertyChangedEventHandler PropertyChanged;

        private static int nextID = 0;
        public int ID { get; private set; } = nextID++;

        private string name;

        public string Name {
            get => name;
            set {
                name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        private bool playing;

        public bool Playing {
            get => playing;
            set {
                playing = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Playing)));
            }
        }

        private int progress;

        // Every unit of progress is a third of a second
        public int Progress {
            get => progress;
            set {
                progress = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));
            }
        }

        public int Length => Segments.Sum(s => s.Length);

        private List<RouteSegment> segments;

        public List<RouteSegment> Segments {
            get => segments;
            set {
                segments = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Segments)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Length)));
            }
        }

        public float CurrentResistance {
            get {
                int cumulative = 0;
                foreach(var current in Segments) {
                    int before = cumulative;
                    cumulative += current.Length;
                    if (cumulative >= Progress) {
                        return current.GetResistance(Progress - before);
                    }
                }
                return 0;
            }
        }

        public float CurrentTilt {
            get {
                int cumulative = 0;
                foreach (var current in Segments) {
                    int before = cumulative;
                    cumulative += current.Length;
                    if (cumulative >= Progress) {
                        Logger.Info(Progress + " - " + before + " = " + (Progress - before) + ": " + current.GetType() + " " + current.GetTilt(Progress - before));
                        return current.GetTilt(Progress - before);
                    }
                }
                return 0;
            }
        }
    }
}
