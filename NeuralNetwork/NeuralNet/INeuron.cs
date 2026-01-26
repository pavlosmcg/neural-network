using System.Collections.Generic;

namespace NeuralNet
{
    public interface INeuron : IInput
    {
        double Error { get; }
        Dictionary<IInput, ISynapse> Inputs { get; }
        void Train(double errorContribution);
        void TrainOutputLayer(double error);
        void RegisterInput(IInput input, double weight);
        void Update();
    }
}
