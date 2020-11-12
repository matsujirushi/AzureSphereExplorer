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
    /// ExtractWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ExtractWindow : Window
    {
        internal TenantModel CurrTenant;
        internal ProductModel CurrProduct;
        internal DeviceGroupModel CurrDeviceGroup;
        internal List<DeviceGroupModel> DeviceGroups;
        internal List<DeviceModel> Devices;

        public ExtractWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ModelManager modelMgr = ModelManager.GetInstance();

            List<ProductModel> products = await modelMgr.GetProductModels(CurrTenant, false);
            this.DeviceGroups = await modelMgr.GetDeviceGroupModels(CurrTenant, false);
            this.Devices = await modelMgr.GetDeviceModels(CurrTenant, false);

            this.gridProducts.ItemsSource = products;

            if (CurrProduct != null)
            {
                int index = products.IndexOf(CurrProduct);
                this.gridProducts.SelectedIndex = index;

                ViewProduct(index);
                ViewDeviceGroups(CurrProduct);
            }

            if (CurrDeviceGroup != null)
            {

                foreach (ProductModel model in products)
                {
                    if(model.Product == this.CurrDeviceGroup.Product)
                    {
                        CurrProduct = model;
                        break;
                    }
                }

                int indexP = products.IndexOf(CurrProduct);
                this.gridProducts.SelectedIndex = indexP;

                ViewProduct(indexP);
                ViewDeviceGroups(CurrProduct);
                ViewDevices(CurrDeviceGroup);
            }
            modelMgr.NotificationChangeDevice += NotificationChangeDevice;
            modelMgr.NotificationChangeDeviceGroup += NotificationChangeDeviceGroup;
        }

        private void ViewProduct(int index)
        {
            var child = VisualTreeHelper.GetChild(this.gridProducts, 0) as Decorator;
            var scroll = child.Child as ScrollViewer;
            scroll.ScrollToVerticalOffset(index);
        }

        private void ViewDeviceGroupMenu(bool view)
        {
            if (view)
            {
                this.gridDeviceGroups.ContextMenu.IsOpen = false;
                this.gridDeviceGroups.ContextMenu.IsEnabled = true;
                this.gridDeviceGroups.ContextMenu.Width = 300;
                this.gridDeviceGroups.ContextMenu.Height = 70;
            }
            else
            {
                this.gridDeviceGroups.ContextMenu.IsEnabled = false;
                this.gridDeviceGroups.ContextMenu.Width = 0;
                this.gridDeviceGroups.ContextMenu.Height = 0;
            }
        }

        private void ViewDeviceMenu(bool view)
        {
            if (view)
            {
                this.gridDevices.ContextMenu.IsOpen = false;
                this.gridDevices.ContextMenu.IsEnabled = true;
                this.gridDevices.ContextMenu.Width = 300;
                this.gridDevices.ContextMenu.Height = 110;
                gridDevices_ViewItem(true);
            }
            else
            {
                this.gridDevices.ContextMenu.IsEnabled = false;
                this.gridDevices.ContextMenu.Width = 0;
                this.gridDevices.ContextMenu.Height = 0;
                gridDevices_ViewItem(false);
            }
        }

        private void ViewDeviceGroups(ProductModel model)
        {

            List<DeviceGroupModel> newObj = new List<DeviceGroupModel>();

            foreach (DeviceGroupModel groups in this.DeviceGroups)
            {
                if (groups.Product == model.Product)
                {
                    newObj.Add(groups);
                }
            }
            this.gridDeviceGroups.ItemsSource = newObj;
            this.gridDeviceGroups.Items.Refresh();

            int index = newObj.IndexOf(this.CurrDeviceGroup);
            this.gridDeviceGroups.SelectedIndex = index;

            if (newObj.Count > 0)
            {
                ViewDeviceGroupMenu(true);
                ViewDeviceMenu(true);
            }
            else
            {
                ViewDeviceGroupMenu(false);
                ViewDeviceMenu(false);
            }
        }
        private void ViewDevices(DeviceGroupModel model)
        {
            List<DeviceModel> newObj = new List<DeviceModel>();

            foreach (DeviceModel device in this.Devices)
            {
                if(device.DeviceGroup == model.DeviceGroup && device.Product == model.Product)
                {
                    newObj.Add(device);
                }
            }
            this.gridDevices.ItemsSource = newObj;
            this.gridDevices.Items.Refresh();

            if(newObj.Count > 0)
            {
                ViewDeviceMenu(true);
            }
        }

        private async void menuitemDeviceGroupDeployments_Click(object sender, RoutedEventArgs e)
        {
            var model = gridDeviceGroups.SelectedItem as DeviceGroupModel;
            List<DeploymentModel> deploymentModels;

            if (model == null)
            {
                return;
            }
            Cursor = Cursors.Wait;
            try
            {
                ModelManager modelManager = ModelManager.GetInstance();
                deploymentModels = await modelManager.GetDeploymentModels(CurrTenant, model);
            }
            finally
            {
                Cursor = null;
            }

            var dialog = new DeploymentsWindow();
            dialog.Owner = this;
            dialog.Title += $" - {model.Product},{model.DeviceGroup}";

            dialog.CurrentTenantModel = this.CurrTenant;
            dialog.DeploymentModels = deploymentModels;
            dialog.SelectDeviceGroupModel = model;

            var dialogResult = dialog.ShowDialog();
            dialog = null;
        }

        private void menuitemDeviceChangeDeviceGroup_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ChangeDeviceGroupWindow();
            dialog.Owner = this;
            dialog.CurrentTenantModel = this.CurrTenant;
            dialog.CurrDevice = this.gridDevices.SelectedItem as DeviceModel;

            var dialogResult = dialog.ShowDialog();
            dialog = null;
        }

        private void gridProducts_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            ProductModel model = this.gridProducts.SelectedItem as ProductModel;

            if (model == null)
            {
                return;
            }

            this.CurrProduct = model;

            ViewDeviceGroups(model);
        }

        private void gridDeviceGroups_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            DeviceGroupModel model = this.gridDeviceGroups.SelectedItem as DeviceGroupModel;
            ItemCollection items = this.gridDeviceGroups.ContextMenu.Items;

            if (model == null)
            {
                foreach (MenuItem item in items)
                {
                    item.IsEnabled = false;
                }

                this.gridDevices.ItemsSource = null;
                this.gridDevices.Items.Refresh();

                return;
            }
            else
            {
                foreach (MenuItem item in items)
                {
                    item.IsEnabled = true;
                }
            }

            this.CurrDeviceGroup = model;
            ViewDevices(CurrDeviceGroup);
        }

        private void gridDevices_ViewItem(bool view)
        {
            ItemCollection items = this.gridDevices.ContextMenu.Items;

            if (view)
            {
                foreach (MenuItem item in items)
                {
                    item.IsEnabled = true;
                }
            }
            else
            {
                foreach (MenuItem item in items)
                {
                    item.IsEnabled = false;
                }
            }
        }
        private async void NotificationChangeDevice(object sender, EventArgs e)
        {
            ModelManager modelMgr = ModelManager.GetInstance();

            Console.Write("called NotificationChangeDevice()");
            this.Devices = await modelMgr.GetDeviceModels(CurrTenant, false);

            ViewDevices(CurrDeviceGroup);
        }

        private async void NotificationChangeDeviceGroup(object sender, EventArgs e)
        {
            ModelManager modelMgr = ModelManager.GetInstance();

            Console.Write("called NotificationChangeDeviceGroup()");
            this.DeviceGroups = await modelMgr.GetDeviceGroupModels(CurrTenant, false);

            ViewDeviceGroups(CurrProduct);
        }

    }
}
