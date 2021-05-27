using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Autofac;
using Microsoft.Extensions.Configuration;
using Prism.Autofac;
using Prism.Ioc;
using Toggl2Jira.Core;
using Toggl2Jira.Core.Repositories;
using Toggl2Jira.UI.Views;

namespace Toggl2Jira.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication       
    {
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            var config = ReadConfiguration();

            var builder = containerRegistry.GetBuilder();
            
            builder.RegisterInstance(config.JiraConfiguration).AsSelf();
            builder.RegisterInstance(config.TogglConfiguration).AsSelf();
            builder.RegisterInstance(config.WorklogDataConfguration).AsSelf();
            builder.RegisterInstance(config.TempoConfiguration).AsSelf();
                
            builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(TogglWorklogRepository)))
                .AsImplementedInterfaces();            

            builder
                .RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .AsSelf();
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }
        
        protected override void InitializeShell(Window shell)
        {
            shell.Show();
        }

        protected override void InitializeModules()
        {
        }

        private Configuration ReadConfiguration()
        {
            var configFile = new ConfigurationBuilder()
                // .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("worklogconfig.json")
                .Build();

            return Configuration.FromEnvironmentConfig(configFile);
        }
        
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            DispatcherUnhandledException += OnDispatcherUnhandledException;            
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            MessageBox.Show(args.Exception.ToString(), "Unhandled Exception has occured", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        }
    }
}