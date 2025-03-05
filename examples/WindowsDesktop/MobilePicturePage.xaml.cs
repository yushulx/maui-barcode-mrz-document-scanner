#if ANDROID || IOS
using Dynamsoft.CaptureVisionRouter.Maui;
using Dynamsoft.BarcodeReader.Maui;
#endif
using SkiaSharp;
using System.Diagnostics;

namespace BarcodeQrScanner;

public partial class MobilePicturePage : ContentPage
{
#if ANDROID || IOS
    private CaptureVisionRouter? router;
#endif
    string path;
    public MobilePicturePage(string imagepath)
    {
        InitializeComponent();

        LoadImageWithOverlay(imagepath);
    }

    public static (int width, int height) GetImageDimensions(string imagePath)
    {
        using var bitmap = SKBitmap.Decode(imagePath);
        if (bitmap == null)
        {
            throw new Exception("Failed to load image.");
        }
        return (bitmap.Width, bitmap.Height);
    }

    async private void LoadImageWithOverlay(string filePath)
    {
        await Task.Run(() =>
        {
            var dimensions = GetImageDimensions(filePath);
            float originalWidth = dimensions.width;
            float originalHeight = dimensions.height;

            try
            {
                ImageSource imageSource = ImageSource.FromFile(filePath);
                PickedImage.Source = imageSource;

#if ANDROID || IOS
                // Decode barcode
                router = new CaptureVisionRouter();
                CapturedResult capturedResult = router.Capture(filePath, EnumPresetTemplate.PT_READ_BARCODES);
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

#endif
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        });

        OverlayGraphicsView.Invalidate(); // Redraw the view
    }

    private void OnImageSizeChanged(object sender, EventArgs e)
    {
        // Adjust the GraphicsView size to match the Image size
        OverlayGraphicsView.WidthRequest = PickedImage.Width;
        OverlayGraphicsView.HeightRequest = PickedImage.Height;
    }
}