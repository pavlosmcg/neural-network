using System;

namespace NeuralNet
{
    public class SigmoidActivation : IActivationFunction
    {
        public double Activate(double input)
        {
            var negInput = -1.0d * input;
            return 1.0d / (1.0d + Math.Exp(negInput));
        }

        public double Derivative(double input)
        {
            // input is already activated (i.e. sigmoid(input))
            // so this is the correct formula
            return input * (1.0d - input);
        }
    }
}
