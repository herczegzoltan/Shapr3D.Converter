using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shapr3D.Converter.Services;
using Shapr3D.Converter.View;
using Sharp3D.Converter.DataAccess.Data;
using Sharp3D.Converter.DataAccess.Repository;
using Sharp3D.Converter.DataAccess.Repository.IRepository;
using Sharp3D.Converter.Ui.Dialogs;
using System;
using System.IO;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using Windows.Storage.Pickers;
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

        /// <summary>
        /// Configure all the required services/classes
        /// </summary>
        /// <returns></returns>
        IServiceProvider ConfigureDependencyInjection()
        {
            var serviceCollection = new ServiceCollection();

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var configuration = builder.Build();

            serviceCollection.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(configuration.GetSection("SQLite").Value));

            serviceCollection.AddTransient<IDialogService, DialogService>(); // req
            serviceCollection.AddSingleton<IUnitOfWork, UnitOfWork>();
            serviceCollection.AddSingleton<IFileConverterService, FileConverterService>();
            serviceCollection.AddSingleton<IFileReaderService, FileReaderService>();
            serviceCollection.AddSingleton<ResourceLoader>();
            serviceCollection.AddSingleton<FileOpenPicker>();
            serviceCollection.AddSingleton<FileSavePicker>();

            return serviceCollection.BuildServiceProvider();
        }
    }
}
