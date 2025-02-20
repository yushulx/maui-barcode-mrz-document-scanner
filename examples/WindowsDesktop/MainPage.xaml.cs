#if ANDROID || IOS
#elif WINDOWS
using Dynamsoft.License;
#endif

using System.Diagnostics;

namespace BarcodeQrScanner;

public partial class MainPage : ContentPage
{

	public MainPage()
	{
		InitializeComponent();
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
        await Navigation.PushAsync(new CameraPage());
    }

	async Task LoadPhotoAsync(FileResult? photo)
	{
		if (photo == null)
		{
			return;
		}

		await Navigation.PushAsync(new PicturePage(photo.FullPath));
	}
}

