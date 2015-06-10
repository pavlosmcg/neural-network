namespace Neuron
{
    public class Synapse : ISynapse
    {
        public IInput Input { get; private set; }
        public double Weight { get; private set; }
        private const double TrainingConstant = 0.01d;

        public Synapse(IInput input, double weight)
        {
            Input = input;
            Weight = weight;
        }

        public void UpdateWeight(double error)
        {
            Weight += (TrainingConstant * error * Input.GetValue());
        }
    }
}
