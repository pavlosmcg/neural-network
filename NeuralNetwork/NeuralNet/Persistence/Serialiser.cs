using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace NeuralNet.Persistence
{
    public static class Serialiser
    {
        public static void SerialiseWeightsToDisk(string filename, List<List<INeuron>> hiddenLayers, List<INeuron> outputLayer)
        {
            var network = new NetworkModel
            {
                HiddenLayers = hiddenLayers.Select(SerialiseLayer).ToArray(),
                OutputLayer = SerialiseLayer(outputLayer)
            };
            string json = JsonConvert.SerializeObject(network, Formatting.Indented);
            using (var file = new StreamWriter(filename))
            {
                file.Write(json);
                file.Close();
            }
        }

        private static LayerModel SerialiseLayer(IEnumerable<INeuron> layer)
        {
            return new LayerModel
            {
                Neurons = layer.Select(neuron => neuron.Inputs.Select(kvp => kvp.Value.Weight))
                    .Select(inputWeights => new NeuronModel { InputWeights = inputWeights.ToArray() })
                    .ToArray()
            };
        }

        public static NetworkModel LoadNetworkFromDisk(string path)
        {
            var sb = new StringBuilder();
            using (var sr = new StreamReader(path))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    sb.AppendLine(line);
                }
            }
            return JsonConvert.DeserializeObject<NetworkModel>(sb.ToString());
        }
    }
}