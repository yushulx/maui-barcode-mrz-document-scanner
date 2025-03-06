#if ANDROID || IOS
using Dynamsoft.CaptureVisionRouter.Maui;
using Dynamsoft.BarcodeReader.Maui;
#endif
using SkiaSharp;
using System.Diagnostics;
using Microsoft.Maui.Graphics.Platform;

namespace BarcodeQrScanner;

public partial class AndroidPicturePage : ContentPage
{
#if ANDROID || IOS
    private CaptureVisionRouter router = new CaptureVisionRouter();
#endif

    public AndroidPicturePage(FileResult result)
    {
        InitializeComponent();

        LoadImageWithOverlay(result);
    }

    public static (int width, int height) GetImageDimensions(string imagePath)
    {
        using var bitmap = SKBitmap.Decode(imagePath);
        if (bitmap == null)
        {
            return (0, 0);
        }
        return (bitmap.Width, bitmap.Height);
    }

    async private void LoadImageWithOverlay(FileResult result)
    {
        // Get the file path
        var filePath = result.FullPath;
        var stream = await result.OpenReadAsync();

        float originalWidth = 0;
        float originalHeight = 0;

        try
        {
            var image = PlatformImage.FromStream(stream);
            originalWidth = image.Width;
            originalHeight = image.Height;

            // Reset the stream position to the beginning
            stream.Position = 0;
            ImageSource imageSource = ImageSource.FromStream(() => stream);
            PickedImage.Source = imageSource;


#if ANDROID || IOS
            // Decode barcode
            var streamcopy = await result.OpenReadAsync();
            byte[] filestream = new byte[streamcopy.Length];
            int offset = 0;
            while (offset < filestream.Length)
            {
                int bytesRead = streamcopy.Read(filestream, offset, filestream.Length - offset);
                if (bytesRead == 0)
                    break;
                offset += bytesRead;
            }

            streamcopy.Close();
            if (offset != filestream.Length)
            {
                throw new IOException("Could not read the entire stream.");
            }
            CapturedResult capturedResult = router.Capture(filestream, EnumPresetTemplate.PT_READ_BARCODES);
            DecodedBarcodesResult? barcodeResults = null;

            if (capturedResult != null)
            {
                // Get the barcode results
                barcodeResults = capturedResult.DecodedBarcodesResult;
            }

            // Create a drawable with the barcode results
            var drawable = new ImageWithOverlayDrawable(barcodeResults, originalWidth, originalHeight, true);

            // Set drawable to GraphicsView
            OverlayGraphicsView.Drawable = drawable;
            OverlayGraphicsView.Invalidate(); // Redraw the view
#endif
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    private void OnImageSizeChanged(object sender, EventArgs e)
    {
        // Adjust the GraphicsView size to match the Image size
        OverlayGraphicsView.WidthRequest = PickedImage.Width;
        OverlayGraphicsView.HeightRequest = PickedImage.Height;
    }
}