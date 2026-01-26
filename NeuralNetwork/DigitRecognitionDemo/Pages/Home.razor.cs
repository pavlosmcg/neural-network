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
    private bool isNetworkLoading = true;
    private bool isProcessing = false;

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
            
            // Enable auto-update of visualisation as user draws
            await canvasModule.InvokeVoidAsync("enableAutoVisualisation", true);
        }
    }

    private async Task LoadNetwork()
    {
        try
        {
            isNetworkLoading = true;
            StateHasChanged();
            
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
        finally
        {
            isNetworkLoading = false;
            StateHasChanged();
        }
    }

    private async Task RecognizeDigit()
    {
        try
        {
            isProcessing = true;
            StateHasChanged();
            
            // Wait for network to load if it's still loading
            while (isNetworkLoading)
            {
                await Task.Delay(100);
            }
            
            if (network == null)
            {
                resultMessage = "Failed to load neural network. Please refresh the page.";
                return;
            }

            if (canvasModule != null)
            {
                // Process the image data into a 28x28 bitmap
                var bitmap = await ProcessImageToBitmap();
                
                // Update the network with the input pixels
                network.UpdateNetwork(bitmap.ToList());
                
                // Get the most likely digit
                string result = network.GetMostLikelyAnswer();
                
                // Get all output values and visualise them
                var outputValues = network.GetOutputValues();
                await canvasModule.InvokeVoidAsync("visualiseOutputNeurons", "outputCanvas", outputValues);
                
                resultMessage = $"I think it looks like {result}";
            }
        }
        catch (Exception ex)
        {
            resultMessage = $"Error recognizing digit: {ex.Message}";
        }
        finally
        {
            isProcessing = false;
            StateHasChanged();
        }
    }

    private async Task ClearCanvas()
    {
        if (canvasModule != null)
        {
            await canvasModule.InvokeVoidAsync("clearCanvas", "drawingCanvas");
            
            // Clear output visualisation
            var emptyOutputs = new double[10];
            await canvasModule.InvokeVoidAsync("visualiseOutputNeurons", "outputCanvas", emptyOutputs);
            
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

    private async Task UpdateVisualisation()
    {
        if (canvasModule != null)
        {
            // Get the processed 28x28 pixel data
            var pixelData = await ProcessImageToBitmap();
            
            // Visualise it on the visualisation canvas
            await canvasModule.InvokeVoidAsync("visualiseInputLayer", "visualizationCanvas", pixelData);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (canvasModule != null)
        {
            await canvasModule.DisposeAsync();
        }
    }
}
