using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using CommunityToolkit.Maui;
using Microsoft.Maui.LifecycleEvents;
using CommunityToolkit.Mvvm.Messaging;

#if WINDOWS
using BarcodeQrScanner.Platforms.Windows;
using BarcodeQrScanner.Controls;
#elif ANDROID || IOS
using Dynamsoft.CameraEnhancer.Maui;
using Dynamsoft.CameraEnhancer.Maui.Handlers;
#endif

namespace BarcodeQrScanner;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder.UseSkiaSharp()
			.UseMauiApp<App>().UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			}).
			ConfigureLifecycleEvents(events =>
							{
#if ANDROID
								events.AddAndroid(android => android
									.OnResume((activity) =>
									{
										NotifyPage("Resume");
									})
									.OnStop((activity) =>
									{
										NotifyPage("Stop");
									}));
#endif
							})
							.ConfigureMauiHandlers(handlers =>
			{
				handlers.AddHandler(typeof(CameraView), typeof(CameraViewHandler));
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}

	private static void NotifyPage(string eventName)
	{
		WeakReferenceMessenger.Default.Send(new LifecycleEventMessage(eventName));
	}
}
