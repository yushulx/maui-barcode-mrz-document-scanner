using Dynamsoft.License.Maui;
namespace AutoNormalize;

public partial class MainPage : ContentPage, ILicenseVerificationListener
{

    public MainPage()
    {
        InitializeComponent();

        // Initialize the license.
        // The license string here is a trial license. Note that network connection is required for this license to work.
        // You can request an extension via the following link: https://www.dynamsoft.com/customer/license/trialLicense?product=dcv&utm_source=samples&package=mobile
        LicenseManager.InitLicense("DLS2eyJoYW5kc2hha2VDb2RlIjoiMjAwMDAxLTE2NDk4Mjk3OTI2MzUiLCJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSIsInNlc3Npb25QYXNzd29yZCI6IndTcGR6Vm05WDJrcEQ5YUoifQ==", this);
    }

    private async void OnCameraClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CameraPage());
    }

    public void OnLicenseVerified(bool isSuccess, string message)
    {
        if (!isSuccess)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                errorMessage.Text = "License initialization failed: " + message;
            });
        }

    }
}


