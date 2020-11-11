using AzureSpherePublicAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
                Cursor = Cursors.Wait;
                try
                {
                    await Api.AuthenticationAsync(cancellationTokenSource.Token);
                }
                finally
                {
                    Cursor = null;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to authenticate.", null, MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            List<AzureSphereTenant> tenants;
            Cursor = Cursors.Wait;
            try
            {
                tenants = await Api.GetTenantsAsync(cancellationTokenSource.Token);
            }
            finally
            {
                Cursor = null;
            }
            if (tenants.Count <= 0)
            {
                MessageBox.Show("No azure sphere tenant found.", null, MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }
            else if (tenants.Count == 1)
            {
                Tenant = tenants[0];
            }
            else
            {
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
            }

            var roles = await Api.GetRolesAsync(Tenant, Api.Username, cancellationTokenSource.Token);
            if (roles.Contains("Administrator")) menuitemUsers.IsEnabled = true;

            await RefreshAllGrids();
        }

        private async Task RefreshAllGrids()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            Cursor = Cursors.Wait;
            try
            {
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
            finally
            {
                Cursor = null;
            }
        }

        #region menuitem - Product

        private async void menuitemProductDelete_Click(object sender, RoutedEventArgs e)
        {
            var model = gridProducts.SelectedItem as ProductModel;
            var product = model.Context;

            var mboxResult = MessageBox.Show(this, $"Do you want to delete the product \"{product.Name}\"?", "Delete Product", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (mboxResult != MessageBoxResult.OK) return;

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            await Api.DeleteProductAsync(Tenant, product, cancellationTokenSource.Token);

            await RefreshAllGrids();
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

        #endregion

        #region menuitem - DeviceGroup

        private async void menuitemDeviceGroupDeployments_Click(object sender, RoutedEventArgs e)
        {
            var model = gridDeviceGroups.SelectedItem as DeviceGroupModel;
            var deviceGroup = model.Context;

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            List<AzureSphereDeployment> deployments;
            Cursor = Cursors.Wait;
            try
            {
                deployments = await Api.GetDeploymentsAsync(Tenant, deviceGroup, cancellationTokenSource.Token);
            }
            finally
            {
                Cursor = null;
            }

            var dialog = new DeploymentsWindow();
            dialog.Owner = this;
            dialog.Title += $" - {model.Product},{model.DeviceGroup}";
            dialog.Deployments = deployments;
            var dialogResult = dialog.ShowDialog();
            dialog = null;
        }

        private async void menuitemDeviceGroupDelete_Click(object sender, RoutedEventArgs e)
        {
            var model = gridDeviceGroups.SelectedItem as DeviceGroupModel;
            var deviceGroup = model.Context;

            var mboxResult = MessageBox.Show(this, $"Do you want to delete the device group \"{deviceGroup.Name}\"?", "Delete Device Group", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (mboxResult != MessageBoxResult.OK) return;

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            await Api.DeleteDeviceGroupAsync(Tenant, deviceGroup, cancellationTokenSource.Token);

            await RefreshAllGrids();
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

        #endregion

        #region menuitem - Device

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

        #endregion

        private async void menuitemReload_Click(object sender, RoutedEventArgs e)
        {
            await RefreshAllGrids();
        }

        private async void menuitemUsers_Click(object sender, RoutedEventArgs e)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            List<AzureSphereUser> users;
            Cursor = Cursors.Wait;
            try
            {
                users = await Api.GetUsersAsync(Tenant, cancellationTokenSource.Token);
            }
            finally
            {
                Cursor = null;
            }

            var dialog = new UsersWindow();
            dialog.Owner = this;
            dialog.Users = users;
            var dialogResult = dialog.ShowDialog();
            dialog = null;
        }

        private async void menuitemErrorReports_Click(object sender, RoutedEventArgs e)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            List<AzureSphereDeviceInsight> deviceInsights;
            Cursor = Cursors.Wait;
            try
            {
                deviceInsights = await Api.GetDeviceInsightsAsync(Tenant, cancellationTokenSource.Token);
            }
            finally
            {
                Cursor = null;
            }

            var dialog = new ErrorReportsWindow();
            dialog.Owner = this;
            dialog.DeviceInsights = deviceInsights;
            var dialogResult = dialog.ShowDialog();
            dialog = null;
        }

        private void menuitemAbout_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AboutWindow();
            dialog.Owner = this;
            var dialogResult = dialog.ShowDialog();
            dialog = null;
        }
    }
}
