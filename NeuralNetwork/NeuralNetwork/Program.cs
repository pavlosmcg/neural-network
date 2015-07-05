using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Components;
using Network = Components.Network;

namespace NeuralNetwork
{
    public class Program
    {
        private static readonly IActivationFunction Activation = new SigmoidActivation();
        public static void Main()
        {
            const double normalisation = 255.0d;
            const string outputFileName = "output.txt";

            var inputFileReader = new InputFileReader();
            IList<Tuple<string, IEnumerable<double>>> csvInputs = inputFileReader.ReadTrainingInputFile(@"training.csv", normalisation);

            int validationFraction = (int)(csvInputs.Count * 0.05); // use all but a few percent for training, hold the rest back for validation
            var trainingInputs = csvInputs.Skip(validationFraction).ToList();
            var validationInputs = csvInputs.Take(validationFraction).ToList();

            var outputList = Enumerable.Range(0, 10).Select(i => i.ToString(CultureInfo.InvariantCulture)).ToList();
            var hiddenLayerSizes = new[] { 250, 200, 150, 100, 50 };
            var network = new Network(Activation, 784, outputList, hiddenLayerSizes);
            
            double previousErrorRate = double.MaxValue;
            double errorRateDelta;

            // training:
            int trainingCounter = 0;
            do
            {
                trainingCounter++;
                int specimenCounter = 0;
                foreach (var specimen in trainingInputs)
                {
                    Console.Write("\rTraining iteration {0}... specimen {1}", trainingCounter, ++specimenCounter);
                    network.TrainNetwork(specimen.Item2.ToList(), specimen.Item1);
                }

                // calculate global error using the validation set that was excluded from training:
                int numberIncorrect = 0;
                foreach (var specimen in validationInputs)
                {
                    network.UpdateNetwork(specimen.Item2.ToList());
                    string answer = network.GetMostLikelyAnswer();
                    if (!string.Equals(answer, specimen.Item1))
                    {
                        numberIncorrect++;
                    }
                }

                double errorRate = (numberIncorrect/(double) trainingInputs.Count) * 100;
                errorRateDelta = Math.Abs(previousErrorRate - errorRate);
                previousErrorRate = errorRate;
                Console.WriteLine("\nError % for iteration {0}: {1}", trainingCounter, errorRate);

                // serialise the network to disk
                network.SerialiseNetworkToDisk(string.Format("network-{0}.json", trainingCounter));

            } while (errorRateDelta > 0.01d); // train until global error begins to level off

            // Run on real testing data:
            Console.WriteLine("Writing output to {0}", outputFileName);
            var testingInputs = inputFileReader.ReadTestingInputFile(@"testing.csv", normalisation);
            using (var writer = new System.IO.StreamWriter(outputFileName))
            {
                foreach (var specimen in testingInputs)
                {
                    network.UpdateNetwork(specimen.ToList());
                    string mostLikelyAnswer = network.GetMostLikelyAnswer();
                    writer.WriteLine(mostLikelyAnswer);
                }
                writer.Close();
            }
        }
    }
}
