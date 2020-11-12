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
    /// CreateDeviceGroupWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class CreateDeviceGroupWindow : Window
    {
        internal TenantModel CurrentTenantModel;
        private List<ProductModel> ProductModels;

        public CreateDeviceGroupWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ModelManager modelManager = ModelManager.GetInstance();
            ProductModels = await modelManager.GetProductModels(CurrentTenantModel, true);
            // プロダクトのリストを作成
            foreach (ProductModel product in ProductModels)
            {
                ProductBox.Items.Add(product.Product);
            }
        }

        private async void Create_Click(object sender, RoutedEventArgs e)
        {
            var index = ProductBox.SelectedIndex;

            ModelManager modelManager = ModelManager.GetInstance();
            JObject newObj = new JObject();

            newObj.Add("Description", new JValue(DescriptionBox.Text));
            newObj.Add("Name", new JValue(DeviceGroupNameBox.Text));
            newObj.Add("OsFeedType", new JValue(OsFeedTypeBox.SelectedIndex));
            newObj.Add("ProductId", new JValue(ProductModels[index].Context.Id));
            newObj.Add("UpdatePolicy", new JValue(UpdatePolicyBox.SelectedIndex));

            List<DeviceGroupModel> groups = await modelManager.GetDeviceGroupModels(this.CurrentTenantModel, false);

            foreach (DeviceGroupModel model in groups)
            {
                if (model.Product == ProductModels[index].Product
                    && model.DeviceGroup == DeviceGroupNameBox.Text)
                {
                    MessageBox.Show("DeviceGroup is already exists",
                        "Error", MessageBoxButtons.OK);
                    return;
                }
            }

            Cursor = System.Windows.Input.Cursors.Wait;
            this.CreateButton.IsEnabled = false;
            this.CloseButton.IsEnabled = false;

            if (await modelManager.CreateDeviceGroup(CurrentTenantModel, newObj.ToString()))
            {
                MessageBox.Show("Create DeviceGroup is success.",
                "Ok", MessageBoxButtons.OK);
            }
            else
            {
                MessageBox.Show("Create DeviceGroup is failure",
                    "Error", MessageBoxButtons.OK);
            }
            Cursor = null;
            this.CreateButton.IsEnabled = true;
            this.CloseButton.IsEnabled = true;

        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
