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
    /// DeploymentsWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class DeploymentsWindow : Window
    {
        public List<AzureSphereDeployment> Deployments { get; set; }

        public DeploymentsWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.gridDeployments.ItemsSource = Deployments;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
