using AzureSpherePublicAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// UsersWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class UsersWindow : Window
    {
        public List<AzureSphereUser> Users { get; set; }

        public UsersWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.gridUsers.ItemsSource = from v in Users
                                               select new UserModel
                                               {
                                                   Context = v,
                                                   User = v.DisplayName,
                                                   Mail = v.Mail,
                                                   Roles = string.Join(",", v.Roles)
                                               };
            var viewUsers = CollectionViewSource.GetDefaultView(this.gridUsers.ItemsSource);
            this.gridUsers.Columns[0].SortDirection = ListSortDirection.Descending;
            viewUsers.SortDescriptions.Add(new SortDescription("User", ListSortDirection.Descending));
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

    }
}
