using Dynamsoft.Core.Maui;
using Dynamsoft.CaptureVisionRouter.Maui;
using Dynamsoft.DocumentNormalizer.Maui;
namespace AutoNormalize;

public partial class ImagePage : ContentPage
{
    ImageData data;
    CaptureVisionRouter cvr;
    private NormalizedImageResultItem _item = null;

    public ImagePage(ImageData data)
    {
        InitializeComponent();
        this.data = data;
        this.cvr = new CaptureVisionRouter();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        normalize(EnumImageColourMode.ICM_Colour);
    }

    private void OnButtonClicked(object sender, EventArgs e)
    {
        Button button = sender as Button;
        if (button != null)
        {
            if (button.Text == "gray")
            {
                normalize(EnumImageColourMode.ICM_GRAYSCALE);
            }
            else if (button.Text == "color")
            {
                normalize(EnumImageColourMode.ICM_Colour);
            }
            else if (button.Text == "binary")
            {
                normalize(EnumImageColourMode.ICM_BINARY);
            }
        }
    }

    private void normalize(EnumImageColourMode type)
    {
        var name = EnumPresetTemplate.PT_NORMALIZE_DOCUMENT;
        var settings = cvr.GetSimplifiedSettings(name);
        settings.DocumentSettings.ColourMode = type;
        cvr.UpdateSettings(name, settings);
        var result = cvr.Capture(data, name);
        if (result?.Items?.Count > 0 && result.Items[0].Type == EnumCapturedResultItemType.CRIT_NORMALIZED_IMAGE)
        {
            _item = (NormalizedImageResultItem)result.Items[0];
            image.Source = _item.ImageData.ToImageSource();
        }
    }

    public async Task<bool> CheckAndRequestStoragePermission()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.StorageWrite>();
        }

        return status == PermissionStatus.Granted;
    }

    private async void OnShareButtonClicked(object sender, EventArgs e)
    {
        if (_item == null)
        {
            await DisplayAlert("Error", "Image is not shareable.", "OK");
            return;
        }

        var imageSource = _item.ImageData.ToImageSource();
        // Check if the image is a StreamImageSource (commonly used for sharing)
        if (imageSource is StreamImageSource streamImageSource)
        {
            // Convert the ImageSource to a stream for sharing
            var stream = await streamImageSource.Stream(CancellationToken.None);

            // Save to a temporary file for sharing
            var tempFile = Path.Combine(FileSystem.CacheDirectory, "shared_image.jpg");
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                await File.WriteAllBytesAsync(tempFile, memoryStream.ToArray());
            }

            // Share the image file
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Share Image",
                File = new ShareFile(tempFile)
            });
        }
        else
        {
            await DisplayAlert("Error", "Image is not shareable.", "OK");
        }
    }
}
