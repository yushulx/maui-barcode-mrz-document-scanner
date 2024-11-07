using Dynamsoft.License.Maui;

namespace mrz_scanner_mobile_maui;

public partial class MainPage : ContentPage
{
    
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnNavigationBtnClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ScanPage());

    }

}
