using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using Autofac;
using Microsoft.Extensions.Configuration;
using Prism.Autofac;
using Toggl2Jira.Core;
using Toggl2Jira.Core.Model;
using Toggl2Jira.Core.Repositories;
using Toggl2Jira.Core.Services;
using Toggl2Jira.UI.Views;

namespace Toggl2Jira.UI
{
    public class Bootstrapper : AutofacBootstrapper
    {
        protected override void ConfigureContainerBuilder(ContainerBuilder builder)
        {
            base.ConfigureContainerBuilder(builder);
            
            var configValues = new Dictionary<string, string>()
            {
                
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configValues)
                .Build();
            
            var config = Configuration.FromEnvironmentConfig(configuration);
                        
            builder.RegisterInstance(config.JiraConfiguration).AsSelf();
            builder.RegisterInstance(config.TogglConfiguration).AsSelf();
            builder.RegisterInstance(config.WorklogDataConfguration).AsSelf();

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
    }
}