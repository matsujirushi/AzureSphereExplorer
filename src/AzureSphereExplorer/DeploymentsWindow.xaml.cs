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
using System.Windows.Shapes;

namespace AzureSphereExplorer
{
    /// <summary>
    /// DeploymentsWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class DeploymentsWindow : Window
    {
        public List<AzureSphereDeployment> Deployments { get; set; }

        private MainWindow _ParentWindow { get { return (MainWindow)this.Owner; } }

        public DeploymentsWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.gridDeployments.ItemsSource = from v in Deployments
                                               select new DeploymentModel
                                               {
                                                   Context = v,
                                                   CurrentDeploymentDate = v.DeploymentDateUtc.ToLocalTime(),
                                                   NumberOfImages = v.DeployedImages.Count
                                               };
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private async void menuitemImages_Click(object sender, RoutedEventArgs e)
        {
            var model = gridDeployments.SelectedItem as DeploymentModel;
            var deployment = model.Context;

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            var images = new List<AzureSphereImage>();
            foreach (var imageId in deployment.DeployedImages)
            {
                images.Add(await _ParentWindow.Api.GetImageAsync(_ParentWindow.Tenant, imageId, cancellationTokenSource.Token));
            }

            var dialog = new ImagesWindow();
            dialog.Owner = this;
            dialog.Images = images;
            var dialogResult = dialog.ShowDialog();
            dialog = null;
        }
    }
}
