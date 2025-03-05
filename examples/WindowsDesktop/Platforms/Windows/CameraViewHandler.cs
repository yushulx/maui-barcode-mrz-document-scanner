using Microsoft.UI.Xaml.Controls;
using Windows.Media.Capture;
using Windows.Media.Core;
using Microsoft.Maui.Handlers;
using BarcodeQrScanner.Controls;
using Windows.Media.Capture.Frames;
using Windows.Graphics.Imaging;
using System.Diagnostics;
using Dynamsoft.CVR;
using Dynamsoft.Core;
using System.Runtime.InteropServices.WindowsRuntime;

namespace BarcodeQrScanner.Platforms.Windows
{
    public class CameraViewHandler : ViewHandler<CameraView, MediaPlayerElement>
    {
        private MediaCapture _mediaCapture;
        private MediaFrameReader? _frameReader;
        private bool _isPreviewing;
        private MediaPlayerElement platformView;
        private CaptureVisionRouter cvr = new CaptureVisionRouter();

        public static IPropertyMapper<CameraView, CameraViewHandler> PropertyMapper = new PropertyMapper<CameraView, CameraViewHandler>(ViewMapper)
        {
        };
        public static CommandMapper<CameraView, CameraViewHandler> CommandMapper = new(ViewCommandMapper)
        {
            [nameof(ICameraHandler.StartPreview)] = MapStartPreview,
            [nameof(ICameraHandler.StopPreview)] = MapStopPreview
        };

        private static void MapStartPreview(CameraViewHandler handler, CameraView view, object? arg)
        {
            handler.StartPreview();
        }

        private static void MapStopPreview(CameraViewHandler handler, CameraView cameraView, object? arg)
        {
            handler.StopPreview();
        }

        public CameraViewHandler() : base(PropertyMapper, CommandMapper)
        {
        }

        protected override MediaPlayerElement CreatePlatformView() => new MediaPlayerElement();

        protected override void ConnectHandler(MediaPlayerElement platformView)
        {
            base.ConnectHandler(platformView);
            this.platformView = platformView;
            StartPreview();
        }

        protected override void DisconnectHandler(MediaPlayerElement platformView)
        {
            CleanupCamera();
            base.DisconnectHandler(platformView);
        }

        private void OnFrameAvailable(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
        {
            using (var frame = sender.TryAcquireLatestFrame())
            {
                if (frame?.VideoMediaFrame?.SoftwareBitmap == null) return;

                var bitmap = SoftwareBitmap.Convert(
                    frame.VideoMediaFrame.SoftwareBitmap,
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Premultiplied
                );

                ProcessFrame(bitmap);
            }
        }

        private void ProcessFrame(SoftwareBitmap bitmap)
        {
            using (bitmap)
            {
                using (SoftwareBitmap grayscale = SoftwareBitmap.Convert(bitmap, BitmapPixelFormat.Gray8, BitmapAlphaMode.Ignore))
                {
                    byte[] buffer = new byte[grayscale.PixelWidth * grayscale.PixelHeight];
                    grayscale.CopyToBuffer(buffer.AsBuffer());
                    ImageData data = new ImageData(buffer, grayscale.PixelWidth, grayscale.PixelHeight, grayscale.PixelWidth, EnumImagePixelFormat.IPF_GRAYSCALED);
                    CapturedResult? result = cvr.Capture(data, PresetTemplate.PT_READ_BARCODES);

                    try
                    {
                        VirtualView.NotifyResultReady(result, bitmap.PixelWidth, bitmap.PixelHeight);
                    }
                    catch (Exception e)
                    {

                    }
                }
            }
        }

        public void StartPreview()
        {
            if (!_isPreviewing)
                _ = InitializeCameraAsync();
        }

        public void StopPreview()
        {
            if (_isPreviewing)
                CleanupCamera();
        }

        private async Task InitializeCameraAsync()
        {
            try
            {
                _mediaCapture = new MediaCapture();

                var allSourceGroups = MediaFrameSourceGroup.FindAllAsync().GetAwaiter().GetResult();

                var settings = new MediaCaptureInitializationSettings
                {
                    //SourceGroup = allSourceGroups.FirstOrDefault(),
                    MemoryPreference = MediaCaptureMemoryPreference.Cpu,
                    StreamingCaptureMode = StreamingCaptureMode.Video
                };
                await _mediaCapture.InitializeAsync(settings);

                var frameSource = _mediaCapture.FrameSources.FirstOrDefault(source => source.Value.Info.MediaStreamType == MediaStreamType.VideoRecord
                                                                                                  && source.Value.Info.SourceKind == MediaFrameSourceKind.Color).Value;
                if (frameSource != null)
                {
                    if (VirtualView != null)
                    {
                        VirtualView.UpdateResolution((int)frameSource.CurrentFormat.VideoFormat.Width, (int)frameSource.CurrentFormat.VideoFormat.Height);
                    }
                    MediaFrameFormat? frameFormat;
                    frameFormat = frameSource.SupportedFormats.OrderByDescending(f => f.VideoFormat.Width * f.VideoFormat.Height).FirstOrDefault();

                    if (frameFormat != null)
                    {
                        await frameSource.SetFormatAsync(frameFormat);
                        platformView.AutoPlay = true;
                        platformView.Source = MediaSource.CreateFromMediaFrameSource(frameSource);

                        _frameReader = await _mediaCapture.CreateFrameReaderAsync(frameSource);
                        _frameReader.AcquisitionMode = MediaFrameReaderAcquisitionMode.Realtime;
                        if (_frameReader != null)
                        {
                            _frameReader.FrameArrived += OnFrameAvailable;
                            await _frameReader.StartAsync();
                        }

                        _isPreviewing = true;

                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Camera init failed: {ex}");
            }
        }

        private void CleanupCamera()
        {
            if (_frameReader != null)
            {
                _frameReader.FrameArrived -= OnFrameAvailable;
                _frameReader?.StopAsync().AsTask().Wait();
                _frameReader?.Dispose();
                _mediaCapture?.Dispose();
                _isPreviewing = false;
                _frameReader = null;
            }
        }
    }
}
