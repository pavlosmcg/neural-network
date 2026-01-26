using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DigitRecognitionDemo.Pages;

public partial class Home : ComponentBase, IAsyncDisposable
{
    private ElementReference drawingCanvas;
    private IJSObjectReference? canvasModule;
    private string? resultMessage;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            canvasModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./js/canvas.js");
            
            await canvasModule.InvokeVoidAsync("initializeCanvas", "drawingCanvas");
        }
    }

    private async Task RecognizeDigit()
    {
        if (canvasModule != null)
        {
            // Get the canvas image data as a base64 string
            var imageData = await canvasModule.InvokeAsync<string>("getCanvasImageData", "drawingCanvas");
            
            // Process the image data into a 28x28 bitmap
            var bitmap = await ProcessImageToBitmap(imageData);
            
            // For now, just show a message that processing is complete
            resultMessage = $"Canvas processed into 28x28 bitmap with {bitmap.Length} pixels. Recognition will be wired up next!";
        }
    }

    private async Task ClearCanvas()
    {
        if (canvasModule != null)
        {
            await canvasModule.InvokeVoidAsync("clearCanvas", "drawingCanvas");
            resultMessage = null;
        }
    }

    private async Task<double[]> ProcessImageToBitmap(string base64ImageData)
    {
        // Remove the data URL prefix if present
        var base64Data = base64ImageData;
        if (base64Data.Contains(','))
        {
            base64Data = base64Data.Substring(base64Data.IndexOf(',') + 1);
        }

        // Convert base64 to byte array
        var imageBytes = Convert.FromBase64String(base64Data);

        // Use JS interop to resize and process the image to 28x28 grayscale
        if (canvasModule != null)
        {
            var pixelData = await canvasModule.InvokeAsync<double[]>(
                "processImageTo28x28", "drawingCanvas");
            
            return pixelData;
        }

        return Array.Empty<double>();
    }

    public async ValueTask DisposeAsync()
    {
        if (canvasModule != null)
        {
            await canvasModule.DisposeAsync();
        }
    }
}
