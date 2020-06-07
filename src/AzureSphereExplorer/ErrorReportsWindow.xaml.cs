using AzureSpherePublicAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AzureSphereExplorer
{
    /// <summary>
    /// ErrorReportsWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ErrorReportsWindow : Window
    {
        public List<AzureSphereDeviceInsight> DeviceInsights { get; set; }

        public ErrorReportsWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.gridErrorReports.ItemsSource = from v in DeviceInsights
                                                select new DeviceInsightModel
                                                {
                                                    Context = v,
                                                    DeviceId = v.DeviceId,
                                                    StartTime = v.StartTimestamp.ToLocalTime(),
                                                    EndTime = v.EndTimestamp.ToLocalTime(),
                                                    Description = v.Description,
                                                    EventType = v.EventType,
                                                    EventClass = v.EventClass,
                                                    EventCategory = v.EventCategory,
                                                    EventCount = v.EventCount
                                                };
            var viewErrorReports = CollectionViewSource.GetDefaultView(this.gridErrorReports.ItemsSource);
            this.gridErrorReports.Columns[0].SortDirection = ListSortDirection.Descending;
            viewErrorReports.SortDescriptions.Add(new SortDescription("StartTime", ListSortDirection.Descending));
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void menuitemErrorReportCopy_Click(object sender, RoutedEventArgs e)
        {
            var model = gridErrorReports.SelectedItem as DeviceInsightModel;
            var deviceInsight = model.Context;

            Clipboard.SetText($"{model.StartTime}\t{model.EndTime}\t{model.Description}\t{model.EventCount}\t{model.EventType}\t{model.EventClass}\t{model.EventCategory}\t{model.DeviceId}");
        }
    }
}
