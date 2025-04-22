
using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfAnimatedGif;
using System.Diagnostics;


namespace JARVIS.Visualizer.Views
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _clockTimer;
        private ClientWebSocket _socket;
        private readonly Stopwatch uptime = Stopwatch.StartNew();

        public MainWindow()
        {
            InitializeComponent();
            InitClock();
            InitWebSocket();
            LoadLog();


            StartDiagnosticsTimer();

            // Load the animated GIF
            var gifUri = new Uri("pack://application:,,,/Assets/JARVIS.gif", UriKind.Absolute);
            var gifImage = new BitmapImage();
            gifImage.BeginInit();
            gifImage.UriSource = gifUri;
            gifImage.CacheOption = BitmapCacheOption.OnLoad;
            gifImage.EndInit();

            ImageBehavior.SetAnimatedSource(GifPlayer, gifImage);
        }

        private void InitClock()
        {
            _clockTimer = new DispatcherTimer();
            _clockTimer.Interval = TimeSpan.FromSeconds(1);
            _clockTimer.Tick += (s, e) => ClockText.Text = DateTime.Now.ToString("HH:mm:ss - ddd, MMM dd");
            _clockTimer.Start();
        }

        private async void InitWebSocket()
        {
            _socket = new ClientWebSocket();
            try
            {
                await _socket.ConnectAsync(new Uri("ws://localhost:5005/ws/"), CancellationToken.None);
                ListenForUpdates();
            }
            catch
            {
                AlertsList.Items.Add("Visualizer: Could not connect to JARVIS backend.");
            }
        }

        private async void ListenForUpdates()
        {
            var buffer = new byte[1024];
            while (_socket.State == WebSocketState.Open)
            {
                var result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Dispatcher.Invoke(() => AlertsList.Items.Insert(0, message));
                }
            }
        }

        private void LoadLog()
        {
            if (File.Exists("Config/jarvis_log.txt"))
            {
                var lines = File.ReadAllLines("Config/jarvis_log.txt");
                foreach (var line in lines)
                {
                    LogList.Items.Insert(0, line);
                }
            }
        }

        private void StartDiagnosticsTimer()
        {
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            timer.Tick += (s, e) =>
            {
                // Uptime
                TimeSpan up = uptime.Elapsed;
                UptimeText.Text = $"Uptime: {up.Hours:D2}:{up.Minutes:D2}:{up.Seconds:D2}";

                // CPU Usage
                try
                {
                    var cpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                    cpu.NextValue(); // Call once to initialize
                    System.Threading.Thread.Sleep(100); // Wait for real reading
                    CpuUsageText.Text = $"CPU Usage: {cpu.NextValue():0.0}%";
                }
                catch
                {
                    CpuUsageText.Text = $"CPU Usage: Unavailable";
                }

                // Memory Usage
                var usedMemory = Process.GetCurrentProcess().PrivateMemorySize64 / (1024 * 1024);
                MemoryUsageText.Text = $"Memory: {usedMemory} MB";
            };

            timer.Start();
        }

        private void LightOn_Click(object sender, RoutedEventArgs e) { }
        private void LightOff_Click(object sender, RoutedEventArgs e) { }
        private void SetAC_Click(object sender, RoutedEventArgs e) { }
    }
}
