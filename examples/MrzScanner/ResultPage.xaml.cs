namespace mrz_scanner_mobile_maui;

using Dynamsoft.Core.Maui;
using SkiaSharp;
using System.Runtime.InteropServices;

public partial class ResultPage : ContentPage
{
    public ResultPage(Dictionary<String, String> labelMap, ImageData imageData)
    {
        InitializeComponent();
        if (labelMap.Count > 0)
        {
            VerticalLayout.Add(ChildView("Document Type:", labelMap["Document Type"]));
            VerticalLayout.Add(ChildView("Document Number:", labelMap["Document Number"]));
            VerticalLayout.Add(ChildView("Full Name:", labelMap["Name"]));
            VerticalLayout.Add(ChildView("Sex:", labelMap["Sex"].First().ToString().ToUpper()));
            VerticalLayout.Add(ChildView("Age:", labelMap["Age"]));
            VerticalLayout.Add(ChildView("Issuing State:", labelMap["Issuing State"]));
            VerticalLayout.Add(ChildView("Nationality:", labelMap["Nationality"]));
            VerticalLayout.Add(ChildView("Date of Birth(YYYY-MM-DD):", labelMap["Date of Birth(YY-MM-DD)"]));
            VerticalLayout.Add(ChildView("Date of Expiry(YYYY-MM-DD):", labelMap["Date of Expiry(YY-MM-DD)"]));

            var imageControl = new Image
            {
                HeightRequest = 400,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            };
            imageControl.Source = imageData.ToImageSource();
            VerticalLayout.Add(imageControl);
        }
    }

    IView ChildView(string label, string text)
    {
        return new VerticalStackLayout
            {
                new Label
                {
                    Text = label,
                    TextColor = Color.FromArgb("AAAAAA"),
                    FontSize = 16,
                    Padding = new Thickness(0, 20, 0, 0),
                },
                new Entry
                {
                    Text = text,
                    TextColor = Colors.White,
                    FontSize = 16,
                    BackgroundColor = Colors.Transparent, // Optional: makes the entry look more like a label
                    IsReadOnly = false, // Allows text editing
                }
            };

    }

    private async void LoadImageAsync(ImageData imageData)
    {

        byte[] buffer = imageData.Bytes.ToArray();

        // byte[] buffer = imageData.Bytes.ToArray();
        int width = imageData.Width;
        int height = imageData.Height;
        int stride = imageData.Stride;
        var imageControl = new Image
        {
            HeightRequest = 400,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
        };

        SKColorType colorType = SKColorType.Rgba8888;
        if (imageData.Format == EnumImagePixelFormat.IPF_ARGB_8888)
        {
            colorType = SKColorType.Rgba8888;
        }
        else if (imageData.Format == EnumImagePixelFormat.IPF_RGB_888)
        {
            colorType = SKColorType.Rgb888x;
        }
        else if (imageData.Format == EnumImagePixelFormat.IPF_BGR_888)
        {
            colorType = SKColorType.Bgra8888;
        }

        SKBitmap bitmap = new SKBitmap();
        SKImageInfo info = new SKImageInfo(width, height, colorType);

        byte[] rgbaData;
        if (imageData.Format == EnumImagePixelFormat.IPF_RGB_888)
        {
            rgbaData = new byte[width * height * 4]; // RGB888 to RGBA8888 conversion
            for (int i = 0, j = 0; i < buffer.Length; i += 3, j += 4)
            {
                rgbaData[j] = buffer[i + 2];
                rgbaData[j + 1] = buffer[i + 1];
                rgbaData[j + 2] = buffer[i];
                rgbaData[j + 3] = 255;
            }

            stride = width * 4;
        }
        else
        {
            rgbaData = buffer;
        }

        GCHandle handle = GCHandle.Alloc(rgbaData, GCHandleType.Pinned);
        try
        {
            IntPtr ptr = handle.AddrOfPinnedObject();
            bitmap.InstallPixels(info, ptr, stride);

            using (var image = SKImage.FromBitmap(bitmap))
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            {
                // Create a memory stream and write image data
                var stream = new MemoryStream();
                data.SaveTo(stream);
                stream.Seek(0, SeekOrigin.Begin);

                // Assign ImageSource, creating a new stream each time it's accessed
                imageControl.Source = ImageSource.FromStream(() =>
                stream);
            }
        }
        finally
        {
            handle.Free();
        }

        // Update the UI on the main thread after the conversion is complete
        MainThread.BeginInvokeOnMainThread(() =>
        {
            VerticalLayout.Add(imageControl);
        });
    }

}
