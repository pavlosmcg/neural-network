using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeuralNet.Persistence;
using Newtonsoft.Json;

namespace NeuralNet
{
    public class Network
    {
        private readonly IActivationFunction _activation;
        private readonly List<SensoryInput> _inputs = new List<SensoryInput>();
        private readonly List<List<INeuron>> _hiddenLayers = new List<List<INeuron>>();
        private Dictionary<INeuron, string> _outputLayer;

        public Network(IActivationFunction activation, int inputSize, IEnumerable<string> outputList, params int[] hiddenLayerSizes)
        {
            _activation = activation;
            CreateNetwork(hiddenLayerSizes, inputSize, outputList);
        }

        public Network(IActivationFunction activation, int inputSize, IEnumerable<string> outputList, string filePath)
        {
            _activation = activation;
            RestoreNetwork(RestoreNetworkFromDisk(filePath), inputSize, outputList);
        }

        private void RestoreNetwork(NetworkModel networkModel, int inputSize, IEnumerable<string> outputList)
        {
            // restore input layer
            InitialiseInputLayer(inputSize);

            // restore hidden layers
            _hiddenLayers.Add(RestoreLayer(networkModel.HiddenLayers[0], _inputs.Cast<IInput>().ToList()));
            for (int i = 1; i < networkModel.HiddenLayers.Length; i++)
            {
                var previousLayer = _hiddenLayers[i - 1];
                _hiddenLayers.Add(RestoreLayer(networkModel.HiddenLayers[i], previousLayer.Cast<IInput>().ToList()));
            }

            // restore output layer
            List<INeuron> outputNeurons = RestoreLayer(networkModel.OutputLayer, _hiddenLayers.Last().Cast<IInput>().ToList());
            _outputLayer = outputNeurons.Zip(outputList, (n, s) => new { n, s })
                .ToDictionary(i => i.n, i => i.s);
        }

        public List<INeuron> RestoreLayer(LayerModel layerModel, List<IInput> inputs)
        {
            var layerBias = new BiasInput(1.0d);
            var layer = new List<INeuron>();
            foreach (var model in layerModel.Neurons)
            {
                var neuron = new Neuron(_activation);
                neuron.RegisterInput(layerBias, model.InputWeights[0]);
                var inputWeights = model.InputWeights.Skip(1).ToArray();
                for (int j = 0; j < inputs.Count; j++)
                {
                    neuron.RegisterInput(inputs[j], inputWeights[j]);
                }
                layer.Add(neuron);
            }
            return layer;
        }

        private void CreateNetwork(int[] hiddenLayerSizes, int inputSize, IEnumerable<string> outputList)
        {
            // intialise input layer
            InitialiseInputLayer(inputSize);

            // initialise hidden layers
            _hiddenLayers.Add(CreateLayer(hiddenLayerSizes[0], _inputs.Cast<IInput>().ToList()));
            for (int i = 1; i < hiddenLayerSizes.Length; i++)
            {
                var previousLayer = _hiddenLayers[i - 1];
                _hiddenLayers.Add(CreateLayer(hiddenLayerSizes[i], previousLayer.Cast<IInput>().ToList()));
            }
            // initialise output layer
            var outputs = outputList.ToList();
            List<INeuron> outputNeurons = CreateLayer(outputs.Count, _hiddenLayers.Last().Cast<IInput>().ToList());
            _outputLayer = outputNeurons.Zip(outputs, (n, s) => new { n, s })
                .ToDictionary(i => i.n, i => i.s);
        }

        private void InitialiseInputLayer(int inputSize)
        {
            for (int i = 0; i < inputSize; i++)
            {
                _inputs.Add(new SensoryInput());
            }
        }

        private List<INeuron> CreateLayer(int layerSize, List<IInput> inputs)
        {
            var layerBias = new BiasInput(1.0d);
            var layer = new List<INeuron>();
            for (var i = 0; i < layerSize; i++)
            {
                var neuron = new Neuron(_activation);
                neuron.RegisterInput(layerBias, Util.GetRandomWeight());
                inputs.ForEach(input => neuron.RegisterInput(input, Util.GetRandomWeight()));
                layer.Add(neuron);
            }
            return layer;
        }

        public void UpdateNetwork(IList<double> specimen)
        {
            for (int i = 0; i < specimen.Count; i++)
                _inputs[i].UpdateValue(specimen[i]);

            _hiddenLayers.ForEach(UpdateLayer);
            UpdateLayer(_outputLayer.Select(i=>i.Key));
        }

        private void UpdateLayer(IEnumerable<INeuron> layer)
        {
            // Output can be computed in parallel as neurons in a layer are independent of each other
            Parallel.ForEach(layer, neuron => neuron.Update());
        }

        public void TrainNetwork(IList<double> specimen, string correctOutput)
        {
            // Update the network with the specimen first
            UpdateNetwork(specimen);

            // train output layer and keep a running total of the errors
            foreach (var kvp in _outputLayer)
            {
                INeuron neuron = kvp.Key;
                double desired = kvp.Value == correctOutput ? 1.0d : 0.0d;
                double output = neuron.GetValue();
                double error = desired - output;
                neuron.Train(error);
            }

            // backpropagate hidden layers from right to left
            BackPropagate(_hiddenLayers.Last(), _outputLayer.Keys.ToList());
            for (int i = _hiddenLayers.Count -1; i > 0; i--)
            {
                BackPropagate(_hiddenLayers[i-1], _hiddenLayers[i]);
            }
        }

        private void BackPropagate(IEnumerable<INeuron> leftLayer, IEnumerable<INeuron> rightLayer)
        {
            // backpropagation can also be done in parallel for a particular layer
            Parallel.ForEach(leftLayer, leftNeuron =>
            {
                // the compute the error contribution of this neuron, by looking at the 
                // error of the neurons it is connected to on the right
                var errorContribution =
                    rightLayer.Sum(rightNeuron => rightNeuron.Inputs[leftNeuron].Weight * rightNeuron.Error);
                leftNeuron.Train(errorContribution);
            });
        }

        public string GetMostLikelyAnswer()
        {
            double max = 0.0d;
            INeuron winningNeuron = null;
            foreach (KeyValuePair<INeuron, string> kvp in _outputLayer)
            {
                INeuron currentNeuron = kvp.Key;
                if (currentNeuron.GetValue() > max)
                {
                    max = currentNeuron.GetValue();
                    winningNeuron = currentNeuron;
                }
            }
            return _outputLayer[winningNeuron];
        }

        public void SerialiseNetworkToDisk(string filename)
        {
            Serialiser.SerialiseWeightsToDisk(
                filename, _hiddenLayers, _outputLayer.Select(item => item.Key).ToList());
        }

        public NetworkModel RestoreNetworkFromDisk(string filePath)
        {
            var sb = new StringBuilder();
            using (var sr = new StreamReader(filePath))
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