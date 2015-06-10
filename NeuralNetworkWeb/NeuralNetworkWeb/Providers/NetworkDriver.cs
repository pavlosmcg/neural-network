using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NeuralNetworkWeb.Models.Serialisation;
using Neuron;

namespace NeuralNetworkWeb.Providers
{
    public class NetworkDriver
    {
        private static readonly IActivationFunction Activation = new SigmoidActivation();

        public void UpdateNetwork(List<INeuron> inputLayer, List<INeuron> hiddenLayer, List<INeuron> outputLayer)
        {
            UpdateLayer(inputLayer);
            UpdateLayer(hiddenLayer);
            UpdateLayer(outputLayer);
        }

        private void UpdateLayer(IEnumerable<INeuron> layer)
        {
            // Output can be computed in parallel as neurons in a layer are independent of each other
            Parallel.ForEach(layer, neuron => neuron.Update());
        }

        public List<INeuron> CreateLayer(List<IInput> inputs, Layer layerModel)
        {
            var layerBias = new BiasInput(1.0d);
            var layer = new List<INeuron>();
            foreach (var model in layerModel.Neurons)
            {
                var neuron = new Neuron.Neuron(Activation);
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

        public int GetMostLikelyAnswer(IList<INeuron> outputLayer)
        {
            double max = 0.0d;
            int answer = 0;
            for (int i = 0; i < outputLayer.Count; i++)
            {
                double value = outputLayer[i].GetValue();
                if (value > max)
                {
                    max = value;
                    answer = i;
                }
            }
            return answer;
        }
    }
}