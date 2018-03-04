using System;
using System.Windows;
using System.Windows.Threading;

namespace Toggl2Jira.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application       
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            var bootstrapper = new Bootstrapper();
            bootstrapper.Run();

            DispatcherUnhandledException += OnDispatcherUnhandledException;            
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            
        }
    }
}