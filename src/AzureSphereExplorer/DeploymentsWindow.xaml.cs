using AzureSpherePublicAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace AzureSphereExplorer
{
    /// <summary>
    /// DeploymentsWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class DeploymentsWindow : Window
    {
        internal TenantModel CurrentTenantModel;
        internal List<DeploymentModel> DeploymentModels;
        internal DeviceGroupModel SelectDeviceGroupModel;

        public DeploymentsWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.gridDeployments.ItemsSource = DeploymentModels;

            var viewDeployments = CollectionViewSource.GetDefaultView(this.gridDeployments.ItemsSource);
            this.gridDeployments.Columns[0].SortDirection = ListSortDirection.Descending;
            viewDeployments.SortDescriptions.Add(new SortDescription("CurrentDeploymentDate", ListSortDirection.Descending));
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Upload_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new UploadImageWindow();
            ModelManager modelManager = ModelManager.GetInstance();
            modelManager.NotificationChangeDeployment += NotificationChangeDeployment;

            dialog.Owner = this;
            dialog.Title += $" - {SelectDeviceGroupModel.DeviceGroup}";
            var dialogResult = dialog.ShowDialog();
            dialog = null;
        }

        private async void NotificationChangeDeployment(object sender, EventArgs e)
        {
            ModelManager modelMgr = ModelManager.GetInstance();

            Console.Write("called NotificationChangeDeployment()");

            List<DeploymentModel> deploymentModels = await modelMgr.GetDeploymentModels(CurrentTenantModel, this.SelectDeviceGroupModel);

            this.gridDeployments.ItemsSource = deploymentModels;
            var viewDeployments = CollectionViewSource.GetDefaultView(this.gridDeployments.ItemsSource);
            this.gridDeployments.Columns[0].SortDirection = ListSortDirection.Descending;
            viewDeployments.SortDescriptions.Add(new SortDescription("CurrentDeploymentDate", ListSortDirection.Descending));
            this.gridDeployments.Items.Refresh();
        }
        private async void menuitemImages_Click(object sender, RoutedEventArgs e)
        {
            var model = gridDeployments.SelectedItem as DeploymentModel;
            var imageModels = new List<ImageModel>();

            Cursor = Cursors.Wait;

            try
            {
                ModelManager modelManager = ModelManager.GetInstance();
                imageModels = await modelManager.GetImageModels(this.CurrentTenantModel, model);
            }
            finally
            {
                Cursor = null;
            }

            var dialog = new ImagesWindow();
            dialog.Owner = this;
            dialog.imageModels = imageModels;

            var dialogResult = dialog.ShowDialog();
            dialog = null;
        }
    }
}
