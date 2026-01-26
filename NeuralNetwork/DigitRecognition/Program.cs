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
            // shuffle the 60k mnist training inputs
            var numberGenerator = new Random(123456); // fixed so that I get the same split every time I try to train this thing.
            IList<Tuple<string, IEnumerable<double>>> allInputs = InputFileReader
                .ReadMnistCsv("mnist_train.csv")
                .OrderBy(_ => numberGenerator.Next())
                .ToList();

            // keep some back for validation
            var trainingInputs = allInputs.Take(50000).ToList();
            var validationInputs = allInputs.Skip(50000).Take(10000).ToList();

            var outputList = Enumerable.Range(0, 10).Select(i => i.ToString(CultureInfo.InvariantCulture)).ToList();
            var hiddenLayerSizes = new[] { 512 };
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
