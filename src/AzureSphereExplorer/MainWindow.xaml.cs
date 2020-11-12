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
        internal TenantModel CurrentTenantModel;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ModelManager modelMgr = ModelManager.GetInstance();
            if (!await modelMgr.Initialize())
            {
                MessageBox.Show("Failed to authenticate.", null, MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            List<TenantModel> tenantModels = await modelMgr.GetTenantModels();

            if (tenantModels.Count <= 0)
            {
                MessageBox.Show("No azure sphere tenant found.", null, MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }
            else if (tenantModels.Count == 1)
            {
                CurrentTenantModel = tenantModels[0];
            }
            else
            {
                var dialog = new TenantsWindow();
                dialog.Owner = this;
                dialog.TenantModels = tenantModels;

                var dialogResult = dialog.ShowDialog();
                if (!dialogResult.Value)
                {
                    Close();
                    return;
                }
                CurrentTenantModel = dialog.SelectedTenantModel;
            }

            // EventHandler
            var roles = await modelMgr.GetRolesAsync(CurrentTenantModel.Context, modelMgr.GetUsername());
            if (roles.Contains("Administrator")) menuitemUsers.IsEnabled = true;

            modelMgr.NotificationChangeProduct += NotificationChangeProduct;
            modelMgr.NotificationChangeDeviceGroup += NotificationChangeDeviceGroup;
            modelMgr.NotificationChangeDevice += NotificationChangeDevice;

            await RefreshAllGrids();
        }

        private async Task RefreshAllGrids()
        {
            Cursor = Cursors.Wait;
            try
            {
                if (this.CurrentTenantModel != null)
                {
                    ModelManager modelMgr = ModelManager.GetInstance();
                    this.Title = $"Azure Sphere Explorer - {CurrentTenantModel.Tenant}";
                    List<ProductModel> productModel = await modelMgr.GetProductModels(CurrentTenantModel, true);
                    List<DeviceGroupModel> deviceGroupModel = await modelMgr.GetDeviceGroupModels(CurrentTenantModel, true);
                    List<DeviceModel> deviceModels = await modelMgr.GetDeviceModels(CurrentTenantModel, true);

                    this.gridProducts.ItemsSource = productModel;
                    this.gridDeviceGroups.ItemsSource = deviceGroupModel;
                    this.gridDevices.ItemsSource = deviceModels;
                }
            }
            finally
            {
                Cursor = null;
            }
        }

        #region menuitem - Product

        private void menuitemProductCreate_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CreateProductWindow();
            dialog.Owner = this;
            dialog.CurrentTenantModel = this.CurrentTenantModel;

            var dialogResult = dialog.ShowDialog();
            dialog = null;
        }

        private async void menuitemProductDelete_Click(object sender, RoutedEventArgs e)
        {
            var model = gridProducts.SelectedItem as ProductModel;

            if(model == null)
            {
                return;
            }

            var mboxResult = MessageBox.Show(this, $"Do you want to delete the product \"{model.Product}\"?", "Delete Product", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (mboxResult != MessageBoxResult.OK) return;
            Cursor = Cursors.Wait;
            try
            {
                ModelManager modelManager = ModelManager.GetInstance();
                if(await modelManager.DeleteProduct(this.CurrentTenantModel, model))
                {
                    MessageBox.Show("Product delete is success",
                        "OK", MessageBoxButton.OK);
                }
                else
                {
                    MessageBox.Show("Product delete is failure",
                        "Error", MessageBoxButton.OK);
                }

            }
            finally
            {
                Cursor = null;
            }
            Cursor = null;
        }

        private void menuitemProductCopyId_Click(object sender, RoutedEventArgs e)
        {
            var model = gridProducts.SelectedItem as ProductModel;
            if (model == null)
            {
                MessageBox.Show("Product is not selected.", "Error", MessageBoxButton.OK);
                return;
            }

            var product = model.Context;
            Clipboard.SetText(product.Id);
        }

        private void menuitemProductCopyShowCommand_Click(object sender, RoutedEventArgs e)
        {
            var model = gridProducts.SelectedItem as ProductModel;
            if (model == null)
            {
                MessageBox.Show("Product is not selected.", "Error", MessageBoxButton.OK);
                return;
            }

            var product = model.Context;
            Clipboard.SetText($"azsphere prd show -i {product.Id}");
        }

        #endregion

        #region menuitem - DeviceGroup

        private void menuitemDeviceGroupCreate_Click(object sender, RoutedEventArgs e)
        {
            if (this.gridProducts.Items.Count <= 0)
            {
                MessageBox.Show("Product is not exists","Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var dialog = new CreateDeviceGroupWindow();
            dialog.Owner = this;
            dialog.CurrentTenantModel = this.CurrentTenantModel;

            var dialogResult = dialog.ShowDialog();
            dialog = null;
        }

        private async void menuitemDeviceGroupDeployments_Click(object sender, RoutedEventArgs e)
        {
            var model = gridDeviceGroups.SelectedItem as DeviceGroupModel;
            List<DeploymentModel> deploymentModels = new List<DeploymentModel>();

            if (model == null)
            {
                return;
            }

            Cursor = Cursors.Wait;
            try
            {
                ModelManager modelManager = ModelManager.GetInstance();
                deploymentModels = await modelManager.GetDeploymentModels(CurrentTenantModel, model);
            }
            finally
            {
                Cursor = null;
            }

            var dialog = new DeploymentsWindow();
            dialog.Owner = this;
            dialog.Title += $" - {model.Product},{model.DeviceGroup}";
            dialog.CurrentTenantModel = this.CurrentTenantModel;
            dialog.DeploymentModels = deploymentModels;
            dialog.SelectDeviceGroupModel = model;


            var dialogResult = dialog.ShowDialog();
            dialog = null;
        }

        private async void menuitemDeviceGroupDelete_Click(object sender, RoutedEventArgs e)
        {
            var model = gridDeviceGroups.SelectedItem as DeviceGroupModel;

            if (model == null)
            {
                return;
            }

            var mboxResult = MessageBox.Show(this, $"Do you want to delete the device group \"{model.DeviceGroup}\"?", "Delete Device Group", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (mboxResult != MessageBoxResult.OK) return;
            Cursor = Cursors.Wait;

            try
            {
                ModelManager modelManager = ModelManager.GetInstance();
                if (await modelManager.DeleteDeviceGroup(this.CurrentTenantModel, model))
                {
                    MessageBox.Show("DeviceGroup delete is success",
                        "OK", MessageBoxButton.OK);
                }
                else
                {
                    MessageBox.Show("DeviceGroup delete is failure",
                        "Error", MessageBoxButton.OK);
                }

            }
            finally
            {
                Cursor = null;
            }
            Cursor = null;
        }

        private void menuitemDeviceGroupCopyId_Click(object sender, RoutedEventArgs e)
        {
            var model = gridDeviceGroups.SelectedItem as DeviceGroupModel;
            if (model == null)
            {
                MessageBox.Show("DeviceGroup is not selected.", "Error", MessageBoxButton.OK);
                return;
            }
            var deviceGroup = model.Context;

            Clipboard.SetText(deviceGroup.Id);
        }

        private void menuitemDeviceGroupCopyShowCommand_Click(object sender, RoutedEventArgs e)
        {
            var model = gridDeviceGroups.SelectedItem as DeviceGroupModel;
            if (model == null)
            {
                MessageBox.Show("DeviceGroup is not selected.", "Error", MessageBoxButton.OK);
                return;
            }
            var deviceGroup = model.Context;

            Clipboard.SetText($"azsphere dg show -i {deviceGroup.Id}");
        }

        #endregion

        #region menuitem - Device

        private void menuitemDeviceChangeDeviceGroup_Click(object sender, RoutedEventArgs e)
        {
            if (this.gridDevices.Items.Count <= 0)
            {
                MessageBox.Show("Device is not exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (this.gridDeviceGroups.Items.Count <= 0)
            {
                MessageBox.Show("DeviceGroup is not exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var dialog = new ChangeDeviceGroupWindow();
            dialog.Owner = this;
            dialog.CurrentTenantModel = this.CurrentTenantModel;

            dialog.CurrDevice = this.gridDevices.SelectedItem as DeviceModel;
            var dialogResult = dialog.ShowDialog();
            dialog = null;
        }

        private void menuitemDeviceCopyId_Click(object sender, RoutedEventArgs e)
        {
            var model = gridDevices.SelectedItem as DeviceModel;
            if (model == null)
            {
                MessageBox.Show("Device is not selected.", "Error", MessageBoxButton.OK);
                return;
            }
            var device = model.Context;

            Clipboard.SetText(device.Id);
        }

        private void menuitemDeviceCopyShowCommand_Click(object sender, RoutedEventArgs e)
        {
            var model = gridDevices.SelectedItem as DeviceModel;
            if (model == null)
            {
                MessageBox.Show("Device is not selected.", "Error", MessageBoxButton.OK);
                return;
            }
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
            List<UserModel> users;
            Cursor = Cursors.Wait;
            try
            {
                ModelManager modelManager = ModelManager.GetInstance();
                users = await modelManager.GetUsersModels(this.CurrentTenantModel);
            }
            finally
            {
                Cursor = null;
            }

            var dialog = new UsersWindow();
            dialog.Owner = this;
            dialog.UsersModels = users;
            var dialogResult = dialog.ShowDialog();
            dialog = null;
        }

        private async void menuitemErrorReports_Click(object sender, RoutedEventArgs e)
        {
            List<DeviceInsightModel> deviceInsightModels = new List<DeviceInsightModel>();

            Cursor = Cursors.Wait;
            try
            {

                ModelManager modelManager = ModelManager.GetInstance();
                deviceInsightModels = await modelManager.GetDeviceInsightModels(this.CurrentTenantModel);
            }
            finally
            {
                Cursor = null;
            }

            var dialog = new ErrorReportsWindow();
            dialog.Owner = this;
            dialog.DeviceInsightModels = deviceInsightModels;

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

        private async void NotificationChangeProduct(object sender, EventArgs e)
        {
            ModelManager modelMgr = ModelManager.GetInstance();

            Console.Write("called NotificationCreateProduct()");
            List<ProductModel> productModel = await modelMgr.GetProductModels(CurrentTenantModel, false);

            this.gridProducts.ItemsSource = productModel;
            this.gridProducts.Items.Refresh();
        }

        private async void NotificationChangeDeviceGroup(object sender, EventArgs e)
        {
            ModelManager modelMgr = ModelManager.GetInstance();

            Console.Write("called NotificationDeviceGroup()");
            List<DeviceGroupModel> deviceGroupModel = await modelMgr.GetDeviceGroupModels(CurrentTenantModel, false);

            this.gridDeviceGroups.ItemsSource = deviceGroupModel;
            this.gridDeviceGroups.Items.Refresh();
        }
        private async void NotificationChangeDevice(object sender, EventArgs e)
        {
            ModelManager modelMgr = ModelManager.GetInstance();

            Console.Write("called NotificationChangeDevice()");
            List<DeviceModel> deviceModels = await modelMgr.GetDeviceModels(CurrentTenantModel, false);

            this.gridDevices.ItemsSource = deviceModels;
            this.gridDevices.Items.Refresh();
        }
    }
}
