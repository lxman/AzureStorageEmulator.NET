using System.Windows;
using AzureStorageEmulator.NET.Common;
using Action = AzureStorageEmulator.NET.Common.Action;

namespace FrontEnd
{
    /// <summary>
    /// Interaction logic for ManageStateWindow.xaml
    /// </summary>
    public partial class ManageStateWindow : Window
    {
        public PatchCommandSettings Settings { get; } = new();

        public bool Execute { get; private set; }

        public ManageStateWindow()
        {
            InitializeComponent();
            ExecuteButton.IsEnabled = false;
        }

        private void ClickExecuteButton(object sender, RoutedEventArgs e)
        {
            Execute = true;
            Close();
        }

        private void ClickCancelButton(object sender, RoutedEventArgs e)
        {
            Execute = false;
            Close();
        }

        private void ResourceClicked(object sender, RoutedEventArgs e)
        {
            Settings.Table = SelectTable.IsChecked ?? false;
            Settings.Queue = SelectQueue.IsChecked ?? false;
            Settings.Blob = SelectBlob.IsChecked ?? false;
            CheckEnableExecute();
        }

        private void ActionClicked(object sender, RoutedEventArgs e)
        {
            Settings.Action = ActionClear.IsChecked ?? false ? Action.Clear : Action.Backup;
            CheckEnableExecute();
        }

        private void CheckEnableExecute()
        {
            ExecuteButton.IsEnabled = Settings.Action != Action.None && (Settings.Table || Settings.Queue || Settings.Blob);
        }
    }
}
