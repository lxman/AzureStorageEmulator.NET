using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using AzureStorageEmulator.NET.Common;
using Microsoft.Win32;
using static SimpleExec.Command;

namespace FrontEnd
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<string> Logs { get; } = [];

        private readonly HttpServer.HttpServer _httpServer = new();
        private readonly BlockingCollection<string> _logSink = [];
        private readonly int _port;
        private ListBox? _resultDisplay;

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
            ManageStateButton.IsEnabled = false;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Process[] runningProcesses = Process.GetProcesses();
            foreach (Process process in runningProcesses)
            {
                if (process.ProcessName == "AzureStorageEmulator.NET")
                    process.Kill();
            }
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
                    RunAsync(dlg.FileName, ["true", $"{_port}"], createNoWindow: true);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    throw;
                }
                ManageStateButton.IsEnabled = true;
            }
        }

        private void ManageButtonClick(object sender, RoutedEventArgs e)
        {
            ManageStateWindow window = new();
            window.ShowDialog();
            if (!window.Execute)
            {
                return;
            }

            PatchCommandSettings settings = window.Settings;
            Dispatcher.Invoke(() => SendPersistRequest(settings));
        }

        private async Task SendPersistRequest(PatchCommandSettings settings)
        {
            HttpClient client = new();
            HttpResponseMessage response;
            HttpRequestMessage request = new(HttpMethod.Patch, "http://127.0.0.1:10010/api/status/snapshot")
            {
                Content = new StringContent(JsonSerializer.Serialize(settings), Encoding.UTF8, "application/json")
            };
            try
            {
                response = await client.SendAsync(request);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                client.Dispose();
            }
            _resultDisplay = new ListBox { ItemsSource = new ObservableCollection<string>(new[] { response.StatusCode.ToString() }) };
            Grid.SetColumn(_resultDisplay, 1);
            Grid.SetRow(_resultDisplay, 2);
            DisplayGrid.Children.Add(_resultDisplay);
            Grid.SetColumnSpan(ManageStateButton, 1);
        }
    }
}