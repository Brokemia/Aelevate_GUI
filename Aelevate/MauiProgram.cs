using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using CommunityToolkit.Maui;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.UI;
using Microsoft.UI.Windowing;

namespace Aelevate;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

        builder.ConfigureLifecycleEvents(events =>
        {
            events.AddWindows(wndLifeCycleBuilder =>
            {
                wndLifeCycleBuilder.OnWindowCreated(window =>
                {
                    window.ExtendsContentIntoTitleBar = false;
                    IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                    WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
                    var _appWindow = AppWindow.GetFromWindowId(myWndId);
                    _appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
                });
            });
        });

#if DEBUG
        builder.Services.AddLogging(configure =>
        {
			configure.SetMinimumLevel(LogLevel.Trace);
			configure.AddNLog();
        });
#endif

        return builder.Build();
	}
}
