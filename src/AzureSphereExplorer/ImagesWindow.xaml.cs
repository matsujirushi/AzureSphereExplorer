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

namespace AzureSphereExplorer
{
    /// <summary>
    /// ImagesWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ImagesWindow : Window
    {
        public List<AzureSphereImage> Images { get; set; }

        public ImagesWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.gridImages.ItemsSource = from v in Images
                                               select new ImageModel
                                               {
                                                   Context = v,
                                                   Image = v.Name,
                                                   Description = v.Description,
                                                   ImageType = v.ImageType
                                               };
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void menuitemImageCopyId_Click(object sender, RoutedEventArgs e)
        {
            var model = gridImages.SelectedItem as ImageModel;
            var image = model.Context;

            Clipboard.SetText(image.Id);
        }

        private void menuitemImageCopyShowCommand_Click(object sender, RoutedEventArgs e)
        {
            var model = gridImages.SelectedItem as ImageModel;
            var image = model.Context;

            Clipboard.SetText($"azsphere img show -i {image.Id}");
        }
    }
}
