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
    /// TenantsWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class TenantsWindow : Window
    {
        public List<AzureSphereTenant> Tenants { get; set; }
        public AzureSphereTenant SelectedTenant { get; set; }

        public TenantsWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.gridTenants.ItemsSource = Tenants;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (this.gridTenants.SelectedItem == null)
            {
                MessageBox.Show("Tenant is not selected.", null, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            this.SelectedTenant = (AzureSphereTenant)this.gridTenants.SelectedItem;

            this.DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

    }
}
