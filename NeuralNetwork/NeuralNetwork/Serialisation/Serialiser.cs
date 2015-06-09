using System.Collections.Generic;
using System.Linq;
using Neuron;
using Newtonsoft.Json;

namespace NeuralNetwork.Serialisation
{
    public class Serialiser
    {
        public void SerialiseWeightsToDisk(
            List<INeuron> inputLayer, List<INeuron> hiddenLayer, List<INeuron> outputLayer)
        {
            var network = new Network
            {
                InputLayer = SerialiseLayer(inputLayer),
                HiddenLayers = new[] { SerialiseLayer(hiddenLayer) },
                OutputLayer = SerialiseLayer(outputLayer)
            };
            string json = JsonConvert.SerializeObject(network, Formatting.Indented);
            using (var file = new System.IO.StreamWriter("network.json"))
            {
                file.Write(json);
                file.Close();
            }
        }

        private Layer SerialiseLayer(List<INeuron> layer)
        {
            return new Layer
            {
                Neurons = layer.Select(neuron => neuron.Inputs.Select(kvp => kvp.Value.Weight))
                    .Select(inputWeights => new Neuron { InputWeights = inputWeights.ToArray() })
                    .ToArray()
            };
        } 
    }
}