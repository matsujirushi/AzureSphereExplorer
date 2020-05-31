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
        internal AzureSphereAPI Api = new AzureSphereAPI();
        internal AzureSphereTenant Tenant;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await Api.AuthenticationAsync(cancellationTokenSource.Token);
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to authenticate.", null, MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            var tenants = await Api.GetTenantsAsync(cancellationTokenSource.Token);
            var dialog = new TenantsWindow();
            dialog.Owner = this;
            dialog.Tenants = tenants;
            var dialogResult = dialog.ShowDialog();
            if (!dialogResult.Value)
            {
                Close();
                return;
            }
            Tenant = dialog.SelectedTenant;
            dialog = null;

            this.Title = $"Azure Sphere Explorer - {Tenant.Name}";

            var products = await Api.GetProductsAsync(Tenant, cancellationTokenSource.Token);
            var deviceGroups = await Api.GetDeviceGroupsAsync(Tenant, cancellationTokenSource.Token);
            var devices = await Api.GetDevicesAsync(Tenant, cancellationTokenSource.Token);

            this.gridProducts.ItemsSource = from v in products
                                            select new ProductModel
                                            { 
                                                Context = v,
                                                Product = v.Name,
                                                Description = v.Description
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
                                                    CurrentDeploymentDate = v.CurrentDeployment?.DeploymentDateUtc.ToLocalTime()
                                                };

            this.gridDevices.ItemsSource = from v in devices
                                           join dg in deviceGroups on v.DeviceGroupId equals dg.Id into gj
                                           from dg_ in gj.DefaultIfEmpty()
                                           join p in products on dg_?.ProductId equals p.Id into gj2
                                           from p_ in gj2.DefaultIfEmpty()
                                           select new DeviceModel
                                           { 
                                               Context = v,
                                               Product = p_?.Name,
                                               DeviceGroup = dg_?.Name,
                                               ChipSku = v.ChipSkuStr,
                                               Id = v.Id
                                           };
        }

        private void menuitemProductCopyId_Click(object sender, RoutedEventArgs e)
        {
            var model = gridProducts.SelectedItem as ProductModel;
            var product = model.Context;

            Clipboard.SetText(product.Id);
        }

        private void menuitemProductCopyShowCommand_Click(object sender, RoutedEventArgs e)
        {
            var model = gridProducts.SelectedItem as ProductModel;
            var product = model.Context;

            Clipboard.SetText($"azsphere prd show -i {product.Id}");
        }

        private async void menuitemDeviceGroupDeployments_Click(object sender, RoutedEventArgs e)
        {
            var model = gridDeviceGroups.SelectedItem as DeviceGroupModel;
            var deviceGroup = model.Context;

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            var deployments = await Api.GetDeploymentsAsync(Tenant, deviceGroup, cancellationTokenSource.Token);

            var dialog = new DeploymentsWindow();
            dialog.Owner = this;
            dialog.Title += $" - {model.Product},{model.DeviceGroup}";
            dialog.Deployments = deployments;
            var dialogResult = dialog.ShowDialog();
            dialog = null;
        }

        private void menuitemDeviceGroupCopyId_Click(object sender, RoutedEventArgs e)
        {
            var model = gridDeviceGroups.SelectedItem as DeviceGroupModel;
            var deviceGroup = model.Context;

            Clipboard.SetText(deviceGroup.Id);
        }

        private void menuitemDeviceGroupCopyShowCommand_Click(object sender, RoutedEventArgs e)
        {
            var model = gridDeviceGroups.SelectedItem as DeviceGroupModel;
            var deviceGroup = model.Context;

            Clipboard.SetText($"azsphere dg show -i {deviceGroup.Id}");
        }

        private void menuitemDeviceCopyId_Click(object sender, RoutedEventArgs e)
        {
            var model = gridDevices.SelectedItem as DeviceModel;
            var device = model.Context;

            Clipboard.SetText(device.Id);
        }

        private void menuitemDeviceCopyShowCommand_Click(object sender, RoutedEventArgs e)
        {
            var model = gridDevices.SelectedItem as DeviceModel;
            var device = model.Context;

            Clipboard.SetText($"azsphere dev show -i {device.Id}");
        }

    }
}
