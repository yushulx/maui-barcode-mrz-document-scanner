#if ANDROID || IOS
using Dynamsoft.CaptureVisionRouter.Maui;
using Dynamsoft.BarcodeReader.Maui;
#endif
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace BarcodeQrScanner;

public partial class iOSPicturePage : ContentPage
{

    SKBitmap? bitmap;
    bool isDataReady = false;
#if ANDROID || IOS
    private CaptureVisionRouter router = new CaptureVisionRouter();

    CapturedResult? result;
#endif
    public iOSPicturePage(FileResult result)
    {
        InitializeComponent();

        LoadImageWithOverlay(result);
    }

    async private void LoadImageWithOverlay(FileResult fileResult)
    {
        var stream = await fileResult.OpenReadAsync();

        try
        {
            bitmap = SKBitmap.Decode(stream);

#if ANDROID || IOS
            // Decode barcode
            stream = await fileResult.OpenReadAsync();
            byte[] filestream = new byte[stream.Length];
            int offset = 0;
            while (offset < filestream.Length)
            {
                int bytesRead = stream.Read(filestream, offset, filestream.Length - offset);
                if (bytesRead == 0)
                    break;
                offset += bytesRead;
            }

            stream.Close();
            if (offset != filestream.Length)
            {
                throw new IOException("Could not read the entire stream.");
            }

            result = router.Capture(filestream, EnumPresetTemplate.PT_READ_BARCODES);
#endif
            isDataReady = true;
            canvasView.InvalidateSurface();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
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

            float textSize = 28;
            float StrokeWidth = 4;

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
#if ANDROID || IOS
            if (isDataReady)
            {
                if (result != null)
                {
                    ResultLabel.Text = "";
                    DecodedBarcodesResult? barcodesResult = result.DecodedBarcodesResult;
                    if (barcodesResult != null)
                    {
                        var items = barcodesResult.Items;
                        foreach (var barcodeItem in items)
                        {
                            Microsoft.Maui.Graphics.Point[] points = barcodeItem.Location.Points;
                            imageCanvas.DrawText(barcodeItem.Text, (float)points[0].X, (float)points[0].Y, SKTextAlign.Left, font, textPaint);
                            imageCanvas.DrawLine((float)points[0].X, (float)points[0].Y, (float)points[1].X, (float)points[1].Y, skPaint);
                            imageCanvas.DrawLine((float)points[1].X, (float)points[1].Y, (float)points[2].X, (float)points[2].Y, skPaint);
                            imageCanvas.DrawLine((float)points[2].X, (float)points[2].Y, (float)points[3].X, (float)points[3].Y, skPaint);
                            imageCanvas.DrawLine((float)points[3].X, (float)points[3].Y, (float)points[0].X, (float)points[0].Y, skPaint);
                        }
                    }
                }
                else
                {
                    ResultLabel.Text = "No 1D/2D barcode found";
                }
            }
#endif

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