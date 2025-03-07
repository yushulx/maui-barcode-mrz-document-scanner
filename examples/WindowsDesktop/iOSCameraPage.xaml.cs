
namespace BarcodeQrScanner;

#if ANDROID || IOS
using Dynamsoft.Core.Maui;
using Dynamsoft.CaptureVisionRouter.Maui;
using Dynamsoft.BarcodeReader.Maui;
using Dynamsoft.CameraEnhancer.Maui;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using SkiaSharp;
using SkiaSharp.Views.Maui;

public partial class iOSCameraPage : ContentPage, ICapturedResultReceiver, ICompletionListener
{
    private CameraEnhancer? enhancer = null;
    private CaptureVisionRouter router;
    private int imageWidth = 0;
    private int imageHeight = 0;
    private static object _lockObject = new object();
    private DecodedBarcodesResult? _barcodeResult = null;
    private CameraView CameraPreview;

    public iOSCameraPage()
    {
        InitializeComponent();

        canvasView.PaintSurface += OnCanvasViewPaintSurface;

        if (DeviceInfo.Platform == DevicePlatform.Android ||
                DeviceInfo.Platform == DevicePlatform.iOS)
        {
            CameraPreview = new Dynamsoft.CameraEnhancer.Maui.CameraView();
            MainGrid.Children.Insert(0, CameraPreview);
        }

        enhancer = new CameraEnhancer();
        router = new CaptureVisionRouter();
        router.SetInput(enhancer);
        router.AddResultReceiver(this);

        WeakReferenceMessenger.Default.Register<LifecycleEventMessage>(this, (r, message) =>
        {
            if (message.EventName == "Resume")
            {
                if (this.Handler != null && enhancer != null)
                {
                    enhancer.Open();
                }
            }
            else if (message.EventName == "Stop")
            {
                enhancer?.Close();
            }
        });
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (this.Handler != null && enhancer != null)
        {
            enhancer.SetCameraView(CameraPreview);
            enhancer.Open();
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Permissions.RequestAsync<Permissions.Camera>();
        router?.StartCapturing(EnumPresetTemplate.PT_READ_BARCODES, this);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        enhancer?.Close();
        router?.StopCapturing();
    }

    public void OnCapturedResultReceived(CapturedResult result)
    {

    }

    public void OnDecodedBarcodesReceived(DecodedBarcodesResult result)
    {
        if (imageWidth == 0 && imageHeight == 0)
        {
            IntermediateResultManager manager = router.GetIntermediateResultManager();
            ImageData data = manager.GetOriginalImage(result.OriginalImageHashId);

            imageWidth = data.Width;
            imageHeight = data.Height;
        }

        lock (_lockObject)
        {
            _barcodeResult = result;
            CameraPreview.GetDrawingLayer(EnumDrawingLayerId.DLI_DBR).Visible = false;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                canvasView.InvalidateSurface();
            });
        }

    }

    public void OnSuccess()
    {
        Debug.WriteLine("success");
    }

    public void OnFailure(int errorCode, string errorMessage)
    {
        Debug.WriteLine(errorMessage);
    }

    void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
    {
        double width = canvasView.Width;
        double height = canvasView.Height;

        var mainDisplayInfo = DeviceDisplay.MainDisplayInfo;
        var orientation = mainDisplayInfo.Orientation;
        var rotation = mainDisplayInfo.Rotation;
        var density = mainDisplayInfo.Density;

        width *= density;
        height *= density;

        double scale, widthScale, heightScale, scaledWidth, scaledHeight;
        double previewWidth, previewHeight;
        if (orientation == DisplayOrientation.Portrait)
        {
            previewWidth = imageWidth;
            previewHeight = imageHeight;
        }
        else
        {
            previewWidth = imageHeight;
            previewHeight = imageWidth;
        }

        widthScale = previewWidth / width;
        heightScale = previewHeight / height;
        scale = widthScale < heightScale ? widthScale : heightScale;
        scaledWidth = previewWidth / scale;
        scaledHeight = previewHeight / scale;

        SKImageInfo info = args.Info;
        SKSurface surface = args.Surface;
        SKCanvas canvas = surface.Canvas;

        canvas.Clear();

        SKPaint skPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Blue,
            StrokeWidth = 4,
        };

        SKPaint textPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Red,
            // TextSize = (float)(18 * density),
            StrokeWidth = 4,
        };

        float textSize = 18;
        SKFont font = new SKFont() { Size = textSize };

        lock (_lockObject)
        {
            if (_barcodeResult != null)
            {
                DecodedBarcodesResult? barcodesResult = _barcodeResult;
                if (barcodesResult != null)
                {
                    var items = barcodesResult.Items;
                    if (items != null)
                    {
                        foreach (var barcodeItem in items)
                        {
                            Microsoft.Maui.Graphics.Point[] points = barcodeItem.Location.Points;

                            float x1 = (float)(points[0].X / scale);
                            float y1 = (float)(points[0].Y / scale);
                            float x2 = (float)(points[1].X / scale);
                            float y2 = (float)(points[1].Y / scale);
                            float x3 = (float)(points[2].X / scale);
                            float y3 = (float)(points[2].Y / scale);
                            float x4 = (float)(points[3].X / scale);
                            float y4 = (float)(points[3].Y / scale);

                            if (widthScale < heightScale)
                            {
                                y1 = (float)(y1 - (scaledHeight - height) / 2);
                                y2 = (float)(y2 - (scaledHeight - height) / 2);
                                y3 = (float)(y3 - (scaledHeight - height) / 2);
                                y4 = (float)(y4 - (scaledHeight - height) / 2);
                            }
                            else
                            {
                                x1 = (float)(x1 - (scaledWidth - width) / 2);
                                x2 = (float)(x2 - (scaledWidth - width) / 2);
                                x3 = (float)(x3 - (scaledWidth - width) / 2);
                                x4 = (float)(x4 - (scaledWidth - width) / 2);
                            }

                            canvas.DrawText(barcodeItem.Text, x1, y1 - 10, SKTextAlign.Left, font, textPaint);
                            canvas.DrawLine(x1, y1, x2, y2, skPaint);
                            canvas.DrawLine(x2, y2, x3, y3, skPaint);
                            canvas.DrawLine(x3, y3, x4, y4, skPaint);
                            canvas.DrawLine(x4, y4, x1, y1, skPaint);
                        }
                    }

                }
            }
        }
    }
}

#endif