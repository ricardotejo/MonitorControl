using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.ComponentModel;
using System;

namespace MonitorControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        MonitorManager manager;

        public List<MonitorData> Monitors = null;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            manager = new MonitorManager();
            manager.Initialize();

            Monitors = manager.Monitors
                .SelectMany(a => a.physicalMonitors.Select(b => new MonitorData(b, a.rect)))
                .OrderBy(r => r.PositionX)
                .ToList();

            MonitorList.ItemsSource = Monitors;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            MonitorData monitor = ((MonitorData)fe.DataContext);

            monitor.SwitchPower();
            manager.ChangePower(monitor.Ref.hPhysicalMonitor, monitor.IsPoweredOn);


        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            MonitorData monitor = ((MonitorData)fe.DataContext);

            monitor.Ref.BrightnessLevel = (uint)e.NewValue;

            manager.ChangeBrightness(monitor.Ref.hPhysicalMonitor, monitor.BrightnessLevel);
        }
    }

    public class MonitorData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public MonitorData(PhysicalMonitor physicalMonitor, Rect rect)
        {
            this.Ref = physicalMonitor;
            IsLandspace = (rect.Right - rect.Left) / (float)(rect.Bottom - rect.Top) > 1.0f;
            PositionX = rect.Left;
        }

        public PhysicalMonitor Ref { get; private set; }
        public string DeviceName { get { return Ref.IsEnabled ? Ref.DeviceName : "Generic Monitor"; } }
        public bool IsEnabled { get { return Ref.IsEnabled; } }
        public bool IsPoweredOn { get { return Ref.IsEnabled ? Ref.IsPoweredOn : false; } }
        public uint BrightnessLevel { get { return Ref.IsEnabled ? Ref.BrightnessLevel : 0; } }

        public bool IsLandspace { get; private set; }
        public int PositionX { get; private set; }
        public int DisplayWidth { get { return IsLandspace ? 96 : 30; } }
        public string PowerText { get { return IsPoweredOn ? "OFF" : "ON"; } }

        internal void SwitchPower()
        {
            this.Ref.IsPoweredOn = !this.Ref.IsPoweredOn;
            PropertyChanged(this, new PropertyChangedEventArgs("PowerText"));
        }
    }

}
