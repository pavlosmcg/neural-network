namespace NeuralNetwork.Serialisation
{
    public class Network
    {
        public Layer InputLayer { get; set; }
        public Layer[] HiddenLayers { get; set; }
        public Layer OutputLayer { get; set; }
    }

    public class Layer
    {
        public Neuron[] Neurons { get; set; }
    }

    public class Neuron
    {
        public double[] InputWeights { get; set; }
    }
}