using AzureSpherePublicAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AzureSphereExplorer
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            var api = new AzureSphereAPI();
            try
            {
                await api.AuthenticationAsync(cancellationTokenSource.Token);
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to authenticate.", null, MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            var tenants = await api.GetTenantsAsync(cancellationTokenSource.Token);
            var dialog = new TenantsWindow();
            dialog.Owner = this;
            dialog.Tenants = tenants;
            var dialogResult = dialog.ShowDialog();
            if (!dialogResult.Value)
            {
                Close();
                return;
            }
            var tenant = dialog.SelectedTenant;
            dialog = null;

            this.Title = $"Azure Sphere Explorer - {tenant.Name}";

            var products = await tenant.GetProductsAsync(cancellationTokenSource.Token);
            var deviceGroups = await tenant.GetDeviceGroupsAsync(cancellationTokenSource.Token);
            var devices = await tenant.GetDevicesAsync(cancellationTokenSource.Token);

            this.gridProducts.ItemsSource = from v in products
                                            select new { v.Name, v.Description };

            this.gridDeviceGroups.ItemsSource = from v in deviceGroups
                                                join p in products on v.ProductId equals p.Id
                                                select new { v.Name, v.Description, v.OsFeedTypeStr, v.UpdatePolicyStr, ProductName = p.Name };

            this.gridDevices.ItemsSource = devices;

            //this.gridDevices.ItemsSource = from v in devices
            //                               join dg in deviceGroups on v.DeviceGroupId equals dg.Id
            //                               select new { v.Id, dg.Name };
        }

    }
}
