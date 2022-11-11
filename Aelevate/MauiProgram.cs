using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace Aelevate;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
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
