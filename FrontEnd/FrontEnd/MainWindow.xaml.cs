using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Windows;
using Microsoft.Win32;
using static SimpleExec.Command;

namespace FrontEnd
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<string> Logs { get; private set; } = [];

        private readonly HttpServer.HttpServer _httpServer = new();
        private readonly BlockingCollection<string> _logSink = [];
        private readonly int _port;

        public MainWindow()
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            IEnumerable<int> usedTcpPorts = properties.GetActiveTcpConnections().Select(c => c.LocalEndPoint.Port);
            IEnumerable<int> usedUdpPorts = properties.GetActiveUdpListeners().Select(u => u.Port);
            IEnumerable<int> usedPorts = usedTcpPorts.Concat(usedUdpPorts);
            _port = Enumerable.Range(49152, 65535 - 49152).Except(usedPorts).First();
            ThreadPool.QueueUserWorkItem(_ => Logger());
            _httpServer.Start(IPAddress.Loopback, _port, _logSink);
            InitializeComponent();
            DataContext = this;
            ((INotifyCollectionChanged)LogView.ItemsSource).CollectionChanged +=
                (s, e) =>
                {
                    if (e.Action ==
                        NotifyCollectionChangedAction.Add)
                    {
                        LogView.ScrollIntoView(LogView.Items[^1]);
                    }
                };
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _httpServer.Stop();
        }

        private void Logger()
        {
            foreach (string log in _logSink.GetConsumingEnumerable())
                Dispatcher.Invoke(() => Logs.Add(log));
        }

        private void StartButtonClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new()
            {
                DefaultExt = ".exe",
                Filter = "EXE Files (*.exe)|*.exe"
            };

            bool? result = dlg.ShowDialog();

            if (result.HasValue && result.Value)
            {
                try
                {
                    RunAsync(dlg.FileName, ["true", $"{_port}"]);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    throw;
                }
            }
        }
    }
}