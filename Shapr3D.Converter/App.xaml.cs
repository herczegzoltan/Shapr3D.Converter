using Microsoft.Extensions.DependencyInjection;
using Shapr3D.Converter.Datasource;
using Shapr3D.Converter.View;
using Sharp3D.Converter.Ui.Dialogs;
using System;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Shapr3D.Converter
{
    sealed partial class App : Application
    {
        public IServiceProvider Container { get; }

        public App()
        {
            InitializeComponent();
            Container = ConfigureDependencyInjection();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;

                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                Window.Current.Activate();
            }
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        IServiceProvider ConfigureDependencyInjection()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddTransient<IDialogService, DialogService>();
            serviceCollection.AddTransient<IPersistedStore, PersistedStore>();

            return serviceCollection.BuildServiceProvider();
        }
    }
}
