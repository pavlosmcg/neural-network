using System.Collections.Generic;

namespace Components
{
    public interface INeuron : IInput
    {
        double Error { get; }
        Dictionary<IInput, ISynapse> Inputs { get; }
        void Train(double errorContribution);
        void RegisterInput(IInput input);
        void Update();
    }
}
