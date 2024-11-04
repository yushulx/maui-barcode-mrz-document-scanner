using Dynamsoft.Core.Maui;
using Dynamsoft.CaptureVisionRouter.Maui;
using Dynamsoft.DocumentNormalizer.Maui;
using Dynamsoft.CameraEnhancer.Maui;
using Dynamsoft.Utility.Maui;

namespace AutoNormalize;

public partial class CameraPage : ContentPage, ICapturedResultReceiver, ICompletionListener
{
    public static CameraEnhancer enhancer;
    CaptureVisionRouter router;

    public CameraPage()
    {
        InitializeComponent();
        enhancer = new CameraEnhancer();
        router = new CaptureVisionRouter();
        router.SetInput(enhancer);
        router.AddResultReceiver(this);
        var filter = new MultiFrameResultCrossFilter();
        // filter.EnableResultCrossVerification(EnumCapturedResultItemType.CRIT_NORMALIZED_IMAGE, true);
        // router.AddResultFilter(filter);
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (this.Handler != null)
        {
            enhancer.SetCameraView(camera);
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        isReady = false;
        await Permissions.RequestAsync<Permissions.Camera>();
        enhancer?.Open();
        router?.StartCapturing(EnumPresetTemplate.PT_DETECT_AND_NORMALIZE_DOCUMENT, this);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        enhancer?.Close();
        router?.StopCapturing();
    }

    bool isReady = false;

    public void OnNormalizedImagesReceived(NormalizedImagesResult result)
    {
        if (result?.Items?.Count > 0 && isReady)
        {
            router?.StopCapturing();
            enhancer?.ClearBuffer();
            var data = result.Items[0].ImageData;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PushAsync(new ImagePage(data));
            });
        }
    }

    private void OnCaptureButtonClicked(object sender, EventArgs e)
    {
        isReady = true;
    }

    public void OnFailure(int errorCode, string errorMessage)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            DisplayAlert("Error", errorMessage, "OK");
        });
    }
}
