using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AzureSphereExplorer
{
    /// <summary>
    /// AboutWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var assembly = Assembly.GetExecutingAssembly();
            T GetCustomAttribute<T>() where T : Attribute => (T)Attribute.GetCustomAttribute(assembly, typeof(T));

            var title = GetCustomAttribute<AssemblyTitleAttribute>().Title;
            var copyright = GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
            var version = GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

            txtTitle.Text = title;
            txtVersion.Text = $"Version {version}";
            txtCopyright.Text = copyright;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
