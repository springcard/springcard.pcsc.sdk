using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PcscDiag3.Configurations;
using PcscDiag3.Controls.Main;
using PcscDiag3.IoC;
using SpringCard.LibCs.Windows.ApplicationConfiguration;
using System;
using System.Threading.Tasks;

namespace PcscDiag3;

public partial class App : Application
{
    private readonly IServiceProvider _services;

    public App()
    {
        var defaultApplicationConfiguration = new ApplicationOptions();

        var configuration = new ConfigurationBuilder()
            .AddSpringCardJsonFile([defaultApplicationConfiguration], false, true)
            .Build();

        _services = new ServiceCollection()
            .AddCommonServices()
            .AddSpringCardLogger(LogLevel.Trace)
            .ConfigureSpringCardOptions<ApplicationOptions>(configuration)
            .BuildServiceProvider();
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var logger = _services.GetRequiredService<ILogger<App>>();
            logger.LogInformation(string.Empty);
            logger.LogInformation("Application started");

            ConfigureLogs(logger);

            var viewModel = _services.GetRequiredService<MainWindowViewModel>();

            var mainWindow = new MainWindow
            {
                DataContext = viewModel
            };

            viewModel.MainWindow = mainWindow;

            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureLogs(ILogger<App> logger)
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            if (e.ExceptionObject is Exception ex)
            {
                logger.Log(LogLevel.Error, ex, "AppDomain.CurrentDomain.UnhandledException");
            }
            else
            {
                logger.LogError("AppDomain.CurrentDomain.UnhandledException: {ExceptionObject}", e.ExceptionObject);
            }
        };

        TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            logger.Log(LogLevel.Error, e.Exception, "TaskScheduler.UnobservedTaskException");
            e.SetObserved();
        };

        Dispatcher.UIThread.UnhandledException += (sender, e) =>
        {
            logger.Log(LogLevel.Error, e.Exception, "Dispatcher.UIThread.UnhandledException");
            e.Handled = true;
        };
    }
}
