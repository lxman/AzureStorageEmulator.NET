using System.ComponentModel;
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

        public ManageStateWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Settings.Action = ActionClear.IsChecked ?? false ? Action.Clear : Action.Backup;
            Settings.Table = SelectTable.IsChecked ?? false;
            Settings.Queue = SelectQueue.IsChecked ?? false;
            Settings.Blob = SelectBlob.IsChecked ?? false;
        }
    }
}
