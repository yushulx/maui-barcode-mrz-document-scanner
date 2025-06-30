using Dynamsoft.DBR;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using BarcodeQrScanner.Controls;
using Dynamsoft.CVR;
namespace BarcodeQrScanner;

public partial class CameraPage : ContentPage
{
    private static object _lockObject = new object();
    private int imageWidth;
    private int imageHeight;
    private CapturedResult? result;

    public CameraPage()
    {
        InitializeComponent();
    }

    private void OnStopButtonClicked(object sender, EventArgs e)
    {
        CameraView.StopPreview();
    }

    private void OnStartButtonClicked(object sender, EventArgs e)
    {
        CameraView.StartPreview();
    }

    void OnOverlayPaint(object sender, SKPaintSurfaceEventArgs args)
    {
        SKImageInfo info = args.Info;
        SKCanvas canvas = args.Surface.Canvas;
        canvas.Clear();

        if (imageWidth == 0 || imageHeight == 0 || result == null)
            return;

        float scaleX = (float)info.Width / imageWidth;
        float scaleY = (float)info.Height / imageHeight;
        float scale = Math.Max(scaleX, scaleY);

        SKPaint rectPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Blue,
            StrokeWidth = 2,
            IsAntialias = true
        };

        SKPaint textPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.Red,
            TextSize = 18,
            IsAntialias = true
        };

        SKFont font = new SKFont() { Size = 18 };
        float x = (info.Width - imageWidth) / 2;
        float y = (info.Height - imageHeight) / 2;

        if (x < 0) x = 0;
        if (y < 0) y = 0;

        lock (_lockObject)
        {
            var barcodesResult = result.GetDecodedBarcodesResult();
            if (barcodesResult == null) return;

            var items = barcodesResult.GetItems();
            foreach (var item in items)
            {
                var points = item.GetLocation().points;

                var scaledPoints = points.Select(p => new SKPoint(p[0] + x, p[1] + y)).ToArray();
                for (int i = 0; i < 4; i++)
                {
                    var p1 = scaledPoints[i];
                    var p2 = scaledPoints[(i + 1) % 4];
                    canvas.DrawLine(p1, p2, rectPaint);
                }

                canvas.DrawText(item.GetText(), scaledPoints[0].X, scaledPoints[0].Y - 5, textPaint);
            }
        }
    }


    private void OnResultReady(object sender, ResultReadyEventArgs e)
    {
        lock (_lockObject)
        {
            imageWidth = e.PreviewWidth;
            imageHeight = e.PreviewHeight;
            result = e.Result;
            Overlay.InvalidateSurface();
        }
    }
}