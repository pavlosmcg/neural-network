using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NeuralNet;

namespace DigitRecognition
{
    public class Program
    {
        public static void Main()
        {
            IList<Tuple<string, IEnumerable<double>>> trainingInputs = InputFileReader.ReadTrainingInput();
            IList<Tuple<string, IEnumerable<double>>> validationInputs = InputFileReader.ReadTestingInput();

            var outputList = Enumerable.Range(0, 10).Select(i => i.ToString(CultureInfo.InvariantCulture)).ToList();
            var hiddenLayerSizes = new[] { 256, 128 };
            var network = new Network(new SigmoidActivation(), 784, outputList, hiddenLayerSizes);

            // training:
            int trainingCounter = 0;
            do
            {
                trainingCounter++;
                int specimenCounter = 0;
                foreach (var specimen in trainingInputs)
                {
                    if (Console.KeyAvailable)
                        break;
                    Console.Write("\rTraining iteration {0}... specimen {1}", trainingCounter, ++specimenCounter);
                    network.TrainNetwork(specimen.Item2.ToList(), specimen.Item1);
                }

                if (Console.KeyAvailable)
                    break;

                // calculate error using the testing set:
                int numberIncorrect = 0;
                foreach (var specimen in validationInputs)
                {
                    network.UpdateNetwork(specimen.Item2.ToList());
                    string answer = network.GetMostLikelyAnswer();
                    if (!string.Equals(answer, specimen.Item1))
                        numberIncorrect++;
                }

                double errorRate = (numberIncorrect/(double) validationInputs.Count) * 100;
                Console.WriteLine("\nError % for iteration {0}: {1}", trainingCounter, errorRate);

                // serialise the network to disk
                Console.WriteLine("Writing iteration {0} to network.json", trainingCounter);
                network.SerialiseNetworkToDisk("network.json");

            } while (!Console.KeyAvailable);
        }
    }
}
