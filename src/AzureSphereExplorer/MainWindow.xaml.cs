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
        private AzureSphereAPI _Api = new AzureSphereAPI();
        private AzureSphereTenant _Tenant;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await _Api.AuthenticationAsync(cancellationTokenSource.Token);
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to authenticate.", null, MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            var tenants = await _Api.GetTenantsAsync(cancellationTokenSource.Token);
            var dialog = new TenantsWindow();
            dialog.Owner = this;
            dialog.Tenants = tenants;
            var dialogResult = dialog.ShowDialog();
            if (!dialogResult.Value)
            {
                Close();
                return;
            }
            _Tenant = dialog.SelectedTenant;
            dialog = null;

            this.Title = $"Azure Sphere Explorer - {_Tenant.Name}";

            var products = await _Api.GetProductsAsync(_Tenant, cancellationTokenSource.Token);
            var deviceGroups = await _Api.GetDeviceGroupsAsync(_Tenant, cancellationTokenSource.Token);
            var devices = await _Api.GetDevicesAsync(_Tenant, cancellationTokenSource.Token);

            this.gridProducts.ItemsSource = from v in products
                                            select new { 
                                                Product = v.Name,
                                                v.Description
                                            };

            this.gridDeviceGroups.ItemsSource = from v in deviceGroups
                                                join p in products on v.ProductId equals p.Id
                                                select new DeviceGroupModel
                                                {
                                                    Context = v,
                                                    Product = p.Name,
                                                    DeviceGroup = v.Name,
                                                    Description = v.Description,
                                                    OsFeedType = v.OsFeedTypeStr,
                                                    UpdatePolicy = v.UpdatePolicyStr,
                                                    CurrentDeploymentDateUtc = v.CurrentDeployment?.DeploymentDateUtc
                                                };

            this.gridDevices.ItemsSource = from v in devices
                                           join dg in deviceGroups on v.DeviceGroupId equals dg.Id into gj
                                           from dg_ in gj.DefaultIfEmpty()
                                           join p in products on dg_?.ProductId equals p.Id into gj2
                                           from p_ in gj2.DefaultIfEmpty()
                                           select new { 
                                               Product = p_?.Name,
                                               DeviceGroup = dg_?.Name,
                                               ChipSku = v.ChipSkuStr,
                                               DeviceId = v.Id
                                           };
        }

        private void menuitemDeviceGroupCopyId_Click(object sender, RoutedEventArgs e)
        {
            var model = gridDeviceGroups.SelectedItem as DeviceGroupModel;
            var deviceGroup = model.Context;

            Clipboard.SetText(deviceGroup.Id);
        }

        private async void menuitemDeviceGroupDeployments_Click(object sender, RoutedEventArgs e)
        {
            var model = gridDeviceGroups.SelectedItem as DeviceGroupModel;
            var deviceGroup = model.Context;

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            var deployments = await _Api.GetDeploymentsAsync(_Tenant, deviceGroup, cancellationTokenSource.Token);

            var dialog = new DeploymentsWindow();
            dialog.Owner = this;
            dialog.Title += $" - {model.Product},{model.DeviceGroup}";

            dialog.Deployments = deployments;
            var dialogResult = dialog.ShowDialog();
            dialog = null;
        }
    }
}
