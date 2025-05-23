
using Dynamsoft.CVR;

namespace BarcodeQrScanner.Controls
{
    public class ResultReadyEventArgs : EventArgs
    {
        public ResultReadyEventArgs(CapturedResult? result, int previewWidth, int previewHeight)
        {
            Result = result;
            PreviewWidth = previewWidth;
            PreviewHeight = previewHeight;
        }

        public CapturedResult? Result { get; private set; }
        public int PreviewWidth { get; private set; }
        public int PreviewHeight { get; private set; }
    }

    public class CameraView : View
    {
        public event EventHandler<ResultReadyEventArgs>? ResultReady;

        public void NotifyResultReady(CapturedResult? result, int previewWidth, int previewHeight)
        {
            if (ResultReady != null)
            {
                ResultReady(this, new ResultReadyEventArgs(result, previewWidth, previewHeight));
            }
        }

        public void UpdateResolution(int width, int height)
        {
            WidthRequest = width;
            HeightRequest = height;
        }

        public void StartPreview() => Handler?.Invoke(nameof(ICameraHandler.StartPreview));
        public void StopPreview() => Handler?.Invoke(nameof(ICameraHandler.StopPreview));
    }

    public interface ICameraHandler : IViewHandler
    {
        void StartPreview();
        void StopPreview();
    }
}



