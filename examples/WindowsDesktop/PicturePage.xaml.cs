using SkiaSharp;
using SkiaSharp.Views.Maui;
#if WINDOWS
using Dynamsoft.CVR;
using Dynamsoft.DBR;
#endif

namespace BarcodeQrScanner;

public partial class PicturePage : ContentPage
{

    SKBitmap? bitmap;
#if WINDOWS
    private CaptureVisionRouter cvr = new CaptureVisionRouter();
    CapturedResult? result;
#endif
    bool isDataReady = false;
    public PicturePage(string imagepath)
    {
        InitializeComponent();

        try
        {
            bitmap = LoadAndCorrectOrientation(imagepath);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        DecodeFile(imagepath);
    }

    SKBitmap LoadAndCorrectOrientation(string imagePath)
    {
        using var stream = new SKFileStream(imagePath);
        using var codec = SKCodec.Create(stream);

        // Decode the bitmap using the codec.
        SKBitmap bitmap = SKBitmap.Decode(codec);

        // Check the encoded origin (EXIF orientation)
        var origin = codec.EncodedOrigin;
        if (origin == SKEncodedOrigin.TopLeft)
        {
            // No rotation needed
            return bitmap;
        }

        // Create a transformation matrix based on the orientation
        SKMatrix matrix = SKMatrix.CreateIdentity();
        int rotatedWidth = bitmap.Width;
        int rotatedHeight = bitmap.Height;

        switch (origin)
        {
            case SKEncodedOrigin.RightTop:
                matrix = SKMatrix.CreateRotationDegrees(90, 0, 0);
                rotatedWidth = bitmap.Height;
                rotatedHeight = bitmap.Width;
                break;
            case SKEncodedOrigin.BottomRight:
                matrix = SKMatrix.CreateRotationDegrees(180, 0, 0);
                break;
            case SKEncodedOrigin.LeftBottom:
                matrix = SKMatrix.CreateRotationDegrees(270, 0, 0);
                rotatedWidth = bitmap.Height;
                rotatedHeight = bitmap.Width;
                break;
            default:
                break;
        }

        // Create a new bitmap with the correct dimensions
        SKBitmap rotatedBitmap = new SKBitmap(rotatedWidth, rotatedHeight);
        using (var surface = new SKCanvas(rotatedBitmap))
        {
            switch (origin)
            {
                case SKEncodedOrigin.RightTop:
                    surface.Translate(rotatedWidth, 0);
                    break;
                case SKEncodedOrigin.BottomRight:
                    surface.Translate(rotatedWidth, rotatedHeight);
                    break;
                case SKEncodedOrigin.LeftBottom:
                    surface.Translate(0, rotatedHeight);
                    break;
            }
            surface.Concat(matrix);
            surface.DrawBitmap(bitmap, 0, 0);
        }

        return rotatedBitmap;
    }


    async void DecodeFile(string imagepath)
    {
#if WINDOWS
        await Task.Run(() =>
        {
            result = cvr.Capture(imagepath, PresetTemplate.PT_READ_BARCODES);
            isDataReady = true;
            return Task.CompletedTask;
        });
#else
        await Task.Delay(100); // Placeholder for non-Windows platforms
        isDataReady = true;
#endif
        canvasView.InvalidateSurface();
    }

    void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
    {
        if (!isDataReady)
        {
            return;
        }
        SKImageInfo info = args.Info;
        SKSurface surface = args.Surface;
        SKCanvas canvas = surface.Canvas;
        canvas.Clear();

        if (bitmap != null)
        {
            var imageCanvas = new SKCanvas(bitmap);

            float textSize = 18;
            float StrokeWidth = 2;

            if (DeviceInfo.Current.Platform == DevicePlatform.Android || DeviceInfo.Current.Platform == DevicePlatform.iOS)
            {
                textSize = (float)(18 * DeviceDisplay.MainDisplayInfo.Density);
                StrokeWidth = 4;
            }

            SKPaint skPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Blue,
                StrokeWidth = StrokeWidth,
            };

            SKPaint textPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Red,
                StrokeWidth = StrokeWidth,
            };

            SKFont font = new SKFont() { Size = textSize };
            if (isDataReady)
            {
#if WINDOWS
                if (result != null)
                {
                    ResultLabel.Text = "";
                    DecodedBarcodesResult? barcodesResult = result.GetDecodedBarcodesResult();
                    if (barcodesResult != null)
                    {
                        BarcodeResultItem[] items = barcodesResult.GetItems();
                        foreach (BarcodeResultItem barcodeItem in items)
                        {
                            Dynamsoft.Core.Point[] points = barcodeItem.GetLocation().points;
                            imageCanvas.DrawText(barcodeItem.GetText(), points[0][0], points[0][1], SKTextAlign.Left, font, textPaint);
                            imageCanvas.DrawLine(points[0][0], points[0][1], points[1][0], points[1][1], skPaint);
                            imageCanvas.DrawLine(points[1][0], points[1][1], points[2][0], points[2][1], skPaint);
                            imageCanvas.DrawLine(points[2][0], points[2][1], points[3][0], points[3][1], skPaint);
                            imageCanvas.DrawLine(points[3][0], points[3][1], points[0][0], points[0][1], skPaint);
                        }
                    }
                }
                else
                {
                    ResultLabel.Text = "No 1D/2D barcode found";
                }
#else
                ResultLabel.Text = "Barcode scanning not available on this platform";
#endif
            }


            float scale = Math.Min((float)info.Width / bitmap.Width,
                               (float)info.Height / bitmap.Height);
            float x = (info.Width - scale * bitmap.Width) / 2;
            float y = (info.Height - scale * bitmap.Height) / 2;
            SKRect destRect = new SKRect(x, y, x + scale * bitmap.Width,
                                               y + scale * bitmap.Height);

            canvas.DrawBitmap(bitmap, destRect);
        }


    }
}

