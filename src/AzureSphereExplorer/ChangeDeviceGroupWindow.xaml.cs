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
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;

namespace AzureSphereExplorer
{
    /// <summary>
    /// ChangeDeviceGroupWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ChangeDeviceGroupWindow : Window
    {

        internal TenantModel CurrentTenantModel;
        private List<DeviceGroupModel> DeviceGroupModels;
        private List<DeviceModel> DeviceModels;
        internal DeviceModel CurrDevice;
        private List<DeviceModelEx> DeviceModelIces = new List<DeviceModelEx>();

        public ChangeDeviceGroupWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ModelManager modelManager = ModelManager.GetInstance();
            this.DeviceGroupModels = await modelManager.GetDeviceGroupModels(CurrentTenantModel, false);
            this.DeviceModels = await modelManager.GetDeviceModels(CurrentTenantModel, false);

            foreach (DeviceGroupModel group in DeviceGroupModels)
            {
                DeviceGroupBox.Items.Add(group.DeviceGroup + "/" + group.Product);
            }

            foreach (DeviceModel model in this.DeviceModels)
            {
                DeviceModelEx newObj;
                if (model == CurrDevice)
                {
                    newObj = new DeviceModelEx(model, true);
                }
                else
                {
                    newObj = new DeviceModelEx(model, false);
                }
                DeviceModelIces.Add(newObj);
            }
            gridDeviceGroups.ItemsSource = this.DeviceModelIces;
        }

        private void LoadCSV_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();

            dialog.Filter = "CSVファイル(*.csv) | *.csv";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                List<DeviceModelEx> newObj = new List<DeviceModelEx>();
                List<string> deviceIdList = CSVParser.Parse(dialog.FileName);

                if(deviceIdList == null)
                {
                    return;
                }

                foreach (DeviceModel model in DeviceModels)
                {
                    foreach (string id in deviceIdList)
                    {

                        if (id == model.Id)
                        {
                            DeviceModelEx modelex = new DeviceModelEx(model, true);
                            newObj.Add(modelex);
                            break;
                        }
                    }
                }

                this.DeviceModelIces = newObj;
                gridDeviceGroups.ItemsSource = this.DeviceModelIces;
            }
        }
        private async void Change_Click(object sender, RoutedEventArgs e)
        {
            List<DeviceModel> changeDeviceModels = new List<DeviceModel>();
            ModelManager modelManager = ModelManager.GetInstance();
            var index = DeviceGroupBox.SelectedIndex;
            var targetDeviceGroup = DeviceGroupModels[index];
            var req = "\"" + targetDeviceGroup.Context.Id + "\"";

            bool changed = false;

            Cursor = System.Windows.Input.Cursors.Wait;
            this.ChangeButton.IsEnabled = false;
            this.LoadButton.IsEnabled = false;
            this.CloseButton.IsEnabled = false;
            foreach (DeviceModelEx model in this.DeviceModelIces)
            {
                if (model.IsChecked)
                {
                    changeDeviceModels.Add(model.DeviceModel);
                }
            }

            if (changeDeviceModels.Count >= 1)
            {
                if(await modelManager.ChangeDeviceGroup(CurrentTenantModel, targetDeviceGroup, changeDeviceModels, req))
                {
                    changed = true;
                }
            }
            else
            {
                Cursor = null;
                this.ChangeButton.IsEnabled = true;
                this.LoadButton.IsEnabled = true;
                this.CloseButton.IsEnabled = true;
                return;
            }

            if (changed)
            {
                this.DeviceModels = await modelManager.GetDeviceModels(CurrentTenantModel, false);

                foreach (DeviceModel model in this.DeviceModels)
                {
                    DeviceModelEx newObj = GetDeviceModelEx(model.Id);
                    if (newObj != null)
                    {
                        DeviceModelIces.Remove(newObj);
                        newObj.Product = model.Product;
                        newObj.DeviceGroup = model.DeviceGroup;
                        newObj.DeviceModel = model;
                        DeviceModelIces.Add(newObj);
                    }
                }
                gridDeviceGroups.ItemsSource = this.DeviceModelIces;
                this.gridDeviceGroups.Items.Refresh();
            }
            Cursor = null;
            this.ChangeButton.IsEnabled = true;
            this.LoadButton.IsEnabled = true;
            this.CloseButton.IsEnabled = true;
        }

        private DeviceModelEx GetDeviceModelEx(string id)
        {
            foreach(DeviceModelEx model in this.DeviceModelIces)
            {
                if(model.Id == id)
                {
                    return model;
                }
            }
            return null;
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
