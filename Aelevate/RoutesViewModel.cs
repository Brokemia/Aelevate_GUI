using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Aelevate {
    public class RoutesViewModel : INotifyPropertyChanged {
        const int MAX_RES = 20;
        const int MIN_RES = 0;
        const float MAX_TILT = 5;
        const float MIN_TILT = 0;
        const int TICK_HZ = 3;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public event PropertyChangedEventHandler PropertyChanged;

        ObservableCollection<Route> routes = new() {
            new() {
                Name = "Reading Classic",
                Segments = new List<RouteSegment> {
                    new ConstantRouteSegment(3, 0f, 3 * 60 * TICK_HZ), // 3 minutes flat ground
                    new ConstantRouteSegment(5, 0f, 2 * 60 * TICK_HZ), // 2 minutes flat ground but difficult
                    new LinearRouteSegment(5, 8, 0f, 2f, 60 * TICK_HZ), // 1 minute slow increase in incline
                    new ConstantRouteSegment(8, 2f, 60 * TICK_HZ), // 1 minute continue up the hill
                    new LinearRouteSegment(8, 4, 2f, 0f, 2 * 60 * TICK_HZ), // 2 minutes hill levels off
                    // 5 minute on the top of the hill with little variations
                    new ConstantRouteSegment(4, 0f, 30 * TICK_HZ), // .5 minutes
                    new ConstantRouteSegment(5, 0f, 30 * TICK_HZ), // .5 minutes
                    new ConstantRouteSegment(6, 0f, 60 * TICK_HZ), // 1 minute
                    new ConstantRouteSegment(5, 0f, 60 * TICK_HZ), // 1 minute
                    new ConstantRouteSegment(4, 0f, 60 * TICK_HZ), // 1 minute
                }
            }, new() {
                Name = "Falling off a cliff but up instead of down",
                Segments = new List<RouteSegment> {
                    new ConstantRouteSegment(5, 0f, TICK_HZ),
                    new ConstantRouteSegment(10, 5f, TICK_HZ)
                }
            }, new() {
                Name = "Tour Down Under",
                Segments = new List<RouteSegment> {
                    new SineRouteSegment(2, 15, 1f, 2f, TICK_HZ * 8, 3 * 60 * TICK_HZ)
                }
            }, new() {
                Name = "Tour Down Under But Big",
                Segments = new List<RouteSegment> {
                    new SineRouteSegment(2, 15, 0f, 5f, TICK_HZ * 8, 3 * 60 * TICK_HZ)
                }
            }, new() {
                Name = "Tour de France",
                Segments = new List<RouteSegment> {
                    new SmartRandomRouteSegment(1, 20, 0f, 5f, TICK_HZ * 6, 3 * 60 * TICK_HZ)
                }
            }
        };

        public ICommand PlayPauseCommand { get; private set; }

        private Route currentRoute;

        public Route CurrentRoute {
            get => currentRoute;
            set {
                currentRoute = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentRoute)));
            }
        }

        private bool locked = true;

        public bool Locked {
            get => locked;
            set {
                locked = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Locked)));
            }
        }

        private float resistance;

        public float Resistance {
            get => resistance;
            set {
                resistance = Math.Clamp(value, MIN_RES, MAX_RES);
                ArduinoComm.Instance.SetResistance((byte)(resistance / MAX_RES * (byte.MaxValue - 1)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Resistance)));
            }
        }
        
        private double tilt;

        public double Tilt {
            get => tilt;
            set {
                tilt = Math.Clamp(value, MIN_TILT, MAX_TILT);
                ArduinoComm.Instance.SetTilt((byte)(tilt / MAX_TILT * (byte.MaxValue - 1)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tilt)));
            }
        }


        private float speed;

        public float Speed {
            get => speed;
            set {
                speed = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Speed)));
            }
        }

        public RoutesViewModel() {
            PlayPauseCommand = new Command(arg => {
                var route = routes.First(r => (int)arg == r.ID);
                if (CurrentRoute != null && CurrentRoute != route) {
                    CurrentRoute.Playing = false;
                }
                route.Playing = !route.Playing;
                Logger.Info(route.Length);
                if (route.Playing) {
                    CurrentRoute = route;
                    Task.Run(() => {
                        if (route.Progress == route.Length) {
                            route.Progress = 0;
                        }
                        while (route.Progress < route.Length) {
                            Resistance = route.CurrentResistance;
                            Tilt = route.CurrentTilt;
                            route.Progress++;
                            Task.Delay(1000 / TICK_HZ).Wait();
                            if (!route.Playing || CurrentRoute != route) return;
                        }

                        route.Playing = false;
                        CurrentRoute = null;
                    });
                } else {
                    CurrentRoute = null;
                }
            });
        }

        public ObservableCollection<Route> Routes { get => routes; set {
                routes = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Routes)));
            }
        }

        public void RefreshRoutes() {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Routes)));
        }
    }
}
