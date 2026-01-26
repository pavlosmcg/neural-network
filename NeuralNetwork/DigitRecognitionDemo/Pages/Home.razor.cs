using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Globalization;
using NeuralNet;
using NeuralNet.Persistence;

namespace DigitRecognitionDemo.Pages;

public partial class Home : ComponentBase, IAsyncDisposable
{
    private ElementReference drawingCanvas;
    private IJSObjectReference? canvasModule;
    private string? resultMessage;
    private NetworkModel? networkModel;
    private Network? network;

    protected override async Task OnInitializedAsync()
    {
        await LoadNetwork();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            canvasModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./js/canvas.js");
            
            await canvasModule.InvokeVoidAsync("initializeCanvas", "drawingCanvas");
        }
    }

    private async Task LoadNetwork()
    {
        try
        {
            // Fetch the network.json file from wwwroot/data/
            networkModel = await HttpClient.GetFromJsonAsync<NetworkModel>("data/network.json");
            
            if (networkModel != null)
            {
                // Create the neural network with the loaded model
                var activation = new SigmoidActivation();
                var outputList = Enumerable.Range(0, 10)
                    .Select(i => i.ToString(CultureInfo.InvariantCulture))
                    .ToList();
                
                network = new Network(activation, 784, outputList, networkModel);
                
                resultMessage = "Neural network loaded successfully! Draw a digit and click Recognize.";
            }
            else
            {
                resultMessage = "Failed to load neural network.";
            }
        }
        catch (Exception ex)
        {
            resultMessage = $"Error loading network: {ex.Message}";
        }
    }

    private async Task RecognizeDigit()
    {
        if (network == null)
        {
            resultMessage = "Neural network not loaded yet. Please wait...";
            return;
        }

        if (canvasModule != null)
        {
            try
            {
                // Process the image data into a 28x28 bitmap
                var bitmap = await ProcessImageToBitmap();
                
                // Update the network with the input pixels
                network.UpdateNetwork(bitmap.ToList());
                
                // Get the most likely digit
                string result = network.GetMostLikelyAnswer();
                
                resultMessage = $"I think it looks like a {result}";
            }
            catch (Exception ex)
            {
                resultMessage = $"Error recognizing digit: {ex.Message}";
            }
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

    private async Task<double[]> ProcessImageToBitmap()
    {
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
