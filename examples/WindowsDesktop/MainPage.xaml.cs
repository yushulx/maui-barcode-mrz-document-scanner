#if ANDROID || IOS
using Dynamsoft.License.Maui;
#endif

using System.Diagnostics;

namespace BarcodeQrScanner;

public partial class MainPage : ContentPage
{
#if ANDROID || IOS
	class LicenseVerificationListener : ILicenseVerificationListener
	{
		public void OnLicenseVerified(bool isSuccess, string message)
		{
			if (!isSuccess)
			{
				Debug.WriteLine(message);
			}
		}
	}
#endif

	public MainPage()
	{
		InitializeComponent();

#if ANDROID || IOS
		LicenseManager.InitLicense("DLS2eyJoYW5kc2hha2VDb2RlIjoiMjAwMDAxLTE2NDk4Mjk3OTI2MzUiLCJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSIsInNlc3Npb25QYXNzd29yZCI6IndTcGR6Vm05WDJrcEQ5YUoifQ==", new LicenseVerificationListener());
#endif
	}

	private async void OnFileButtonClicked(object sender, EventArgs e)
	{
		try
		{

			FileResult? photo = null;
			if (DeviceInfo.Current.Platform == DevicePlatform.WinUI || DeviceInfo.Current.Platform == DevicePlatform.MacCatalyst)
			{
				photo = await FilePicker.PickAsync();
			}
			else if (DeviceInfo.Current.Platform == DevicePlatform.Android || DeviceInfo.Current.Platform == DevicePlatform.iOS)
			{
				photo = await MediaPicker.CapturePhotoAsync();
			}
			await LoadPhotoAsync(photo);
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"CapturePhotoAsync THREW: {ex.Message}");
		}
	}

	private async void OnCameraButtonClicked(object sender, EventArgs e)
	{
		if (DeviceInfo.Current.Platform == DevicePlatform.Android || DeviceInfo.Current.Platform == DevicePlatform.iOS)
		{
			await Navigation.PushAsync(new AndroidCameraPage());
		}
		else if (DeviceInfo.Current.Platform == DevicePlatform.iOS)
		{
			await Navigation.PushAsync(new iOSCameraPage());
		}
		else
		{
			await Navigation.PushAsync(new CameraPage());
		}

	}

	async Task LoadPhotoAsync(FileResult? photo)
	{
		if (photo == null)
		{
			return;
		}

		if (DeviceInfo.Current.Platform == DevicePlatform.Android)
		{
			await Navigation.PushAsync(new AndroidPicturePage(photo));
		}
		else if (DeviceInfo.Current.Platform == DevicePlatform.iOS)
		{
			await Navigation.PushAsync(new iOSPicturePage(photo));
		}
		else
		{
			await Navigation.PushAsync(new PicturePage(photo.FullPath));
		}
	}
}

