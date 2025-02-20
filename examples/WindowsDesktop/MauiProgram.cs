using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
#if WINDOWS
using BarcodeQrScanner.Platforms.Windows;
#endif
using BarcodeQrScanner.Controls;

namespace BarcodeQrScanner;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder.UseSkiaSharp()
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			}).ConfigureMauiHandlers(handlers =>
            {
#if WINDOWS
handlers.AddHandler(typeof(CameraView), typeof(CameraPreviewHandler));
#endif
            }); ;

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
