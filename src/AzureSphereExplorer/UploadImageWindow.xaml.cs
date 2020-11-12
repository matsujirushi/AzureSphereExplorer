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
using System.Windows.Forms;
using AzureSpherePublicAPI;
using System.Net.Http;
using MessageBox = System.Windows.Forms.MessageBox;

namespace AzureSphereExplorer
{
    /// <summary>
    /// UploadImageWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class UploadImageWindow : Window
    {
        private DeploymentsWindow deployWindow;

        public UploadImageWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            deployWindow = (DeploymentsWindow)this.Owner;
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();

            dialog.Filter = "イメージパッケージ(*.imagepackage) | *.imagepackage";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FilePath.Text = dialog.FileName;
                this.UploadButton.IsEnabled = true;
            }
        }

        private async void Upload_Click(object sender, RoutedEventArgs e)
        {
            Cursor = System.Windows.Input.Cursors.Wait;
            this.SelectButton.IsEnabled = false;
            this.UploadButton.IsEnabled = false;
            this.DeployButton.IsEnabled = false;
            this.CloseButton.IsEnabled = false;

            try
            {
                ModelManager modelManager = ModelManager.GetInstance();
                System.IO.FileStream fs = new System.IO.FileStream(FilePath.Text, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                byte[] bs = new byte[fs.Length];
                fs.Read(bs, 0, bs.Length);
                fs.Close();

                if (await modelManager.ImageUpload(deployWindow.CurrentTenantModel, bs))
                {
                    MessageBox.Show("Upload image is success.",
                    "Ok", MessageBoxButtons.OK);
                }
                else
                {
                    MessageBox.Show("Image is already upload.",
                    "Ok", MessageBoxButtons.OK);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }
            Cursor = null;
            this.SelectButton.IsEnabled = true;
            this.UploadButton.IsEnabled = true;
            this.DeployButton.IsEnabled = true;
            this.CloseButton.IsEnabled = true;

        }

        private async void Deploy_Click(object sender, RoutedEventArgs e)
        {
            bool isUploadButtonChecked = this.UploadButton.IsEnabled;
            Cursor = System.Windows.Input.Cursors.Wait;
            this.SelectButton.IsEnabled = false;
            this.UploadButton.IsEnabled = false;
            this.DeployButton.IsEnabled = false;
            this.CloseButton.IsEnabled = false;
            try
            {
                ModelManager modelManager = ModelManager.GetInstance();
                DeviceGroupModel dev = deployWindow.SelectDeviceGroupModel;
                string req = ImageIdList.Text;

                if (await modelManager.Deployment(deployWindow.CurrentTenantModel, dev, req))
                {
                    MessageBox.Show("Deployment is success.",
                    "Ok", MessageBoxButtons.OK);
                }
                else
                {
                    MessageBox.Show("Deployment is failure.",
                    "Error", MessageBoxButtons.OK);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Cursor = null;
            this.SelectButton.IsEnabled = true;
            this.UploadButton.IsEnabled = isUploadButtonChecked;
            this.DeployButton.IsEnabled = true;
            this.CloseButton.IsEnabled = true;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }


    }
}
