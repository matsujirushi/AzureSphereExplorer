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
    /// CreateProductWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class CreateProductWindow : Window
    {
        internal TenantModel CurrentTenantModel;

        public CreateProductWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private async void Create_Click(object sender, RoutedEventArgs e)
        {
            ModelManager modelManager = ModelManager.GetInstance();
            JObject newObj = new JObject();

            newObj.Add("CreateDefaultGroups", new JValue(DefaultGroupToggleBotton.IsChecked));
            newObj.Add("Description", new JValue(DescriptionBox.Text));
            newObj.Add("Name", new JValue(ProductNameBox.Text));

            List<ProductModel> products = await modelManager.GetProductModels(this.CurrentTenantModel, false);

            foreach(ProductModel model in products)
            {
                if(model.Product == ProductNameBox.Text)
                {
                    MessageBox.Show("Product is already exists",
                        "Error", MessageBoxButtons.OK);
                    return;
                }
            }

            Cursor = System.Windows.Input.Cursors.Wait;
            this.CreateButton.IsEnabled = false;
            this.CloseButton.IsEnabled = false;

            if (await modelManager.CreateProductGroup(CurrentTenantModel, newObj.ToString()))
            {
                MessageBox.Show("Create Product is success.",
                "Ok", MessageBoxButtons.OK);
            }
            else
            {
                MessageBox.Show("Create Product is failure.",
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

        private void DefaultGroupToggleBotton_Click(object sender, RoutedEventArgs e)
        {
            if (DefaultGroupToggleBotton.IsChecked ?? true)
            {
                DefaultGroupToggleBotton.Content = "Yes";
            }
            else
            {
                DefaultGroupToggleBotton.Content = "No";
            }
        }
    }
}
