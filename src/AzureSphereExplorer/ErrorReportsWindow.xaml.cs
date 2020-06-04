using AzureSpherePublicAPI;
using System;
using System.Collections.Generic;
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
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
