namespace Components.Persistence
{
    public class NetworkModel
    {
        public LayerModel[] HiddenLayers { get; set; }
        public LayerModel OutputLayer { get; set; }
    }

    public class LayerModel
    {
        public NeuronModel[] Neurons { get; set; }
    }

    public class NeuronModel
    {
        public double[] InputWeights { get; set; }
    }
}