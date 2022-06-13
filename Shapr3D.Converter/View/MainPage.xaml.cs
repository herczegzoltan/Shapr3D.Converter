using Microsoft.Extensions.DependencyInjection;
using Shapr3D.Converter.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Shapr3D.Converter.View
{

    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; set; }

        public MainPage()
        {
            InitializeComponent();
            
            var container = ((App)App.Current).Container;
            ViewModel = (MainViewModel)ActivatorUtilities.GetServiceOrCreateInstance(container, typeof(MainViewModel));

            //ViewModel = new MainViewModel();

            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {


            //await ViewModel.InitAsync();
        }
    }
}
