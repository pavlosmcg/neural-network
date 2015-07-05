namespace NeuralNet
{
    public interface IActivationFunction
    {
        double Activate(double input);
        double Derivative(double input);
    }
}
