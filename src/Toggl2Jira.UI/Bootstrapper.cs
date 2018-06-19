using System.IO;
using System.Reflection;
using System.Windows;
using Autofac;
using Microsoft.Extensions.Configuration;
using Prism.Autofac;
using Toggl2Jira.Core;
using Toggl2Jira.Core.Repositories;
using Toggl2Jira.UI.Views;

namespace Toggl2Jira.UI
{
    public class Bootstrapper : AutofacBootstrapper
    {
        protected override void ConfigureContainerBuilder(ContainerBuilder builder)
        {
            base.ConfigureContainerBuilder(builder);
                                  
            var config = ReadConfiguration();
                        
            builder.RegisterInstance(config.JiraConfiguration).AsSelf();
            builder.RegisterInstance(config.TogglConfiguration).AsSelf();
            builder.RegisterInstance(config.WorklogDataConfguration).AsSelf();
            builder.RegisterInstance(config.TempoConfiguration).AsSelf();

            builder
                .RegisterAssemblyTypes(Assembly.GetAssembly(typeof(TogglWorklogRepository)))
                .AsImplementedInterfaces();            

            builder
                .RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .AsSelf();
        }

        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void InitializeShell()
        {
            Application.Current?.MainWindow?.Show();
        }

        private Configuration ReadConfiguration()
        {
            var configFile = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("worklogconfig.json")
               .Build();

            return Configuration.FromEnvironmentConfig(configFile);
        }
    }
}