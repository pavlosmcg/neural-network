using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NeuralNetwork
{
    public class InputFileReader
    {
        public IList<Tuple<string, IEnumerable<double>>> ReadTrainingInputFile(string filePath, double normalisation = 1.0d)
        {
            IList<Tuple<string, IEnumerable<double>>> csvInputs = new List<Tuple<string, IEnumerable<double>>>();
            using (var reader = new StreamReader(File.OpenRead(filePath)))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line == null || line.StartsWith("label"))
                    {
                        continue;
                    }

                    var raw = line.Split(',');
                    var tuple = new Tuple<string, IEnumerable<double>>(raw[0],
                        raw.Skip(1).Select(s => double.Parse(s)/ normalisation));
                    csvInputs.Add(tuple);
                }
            }
            return csvInputs;
        }

        public IList<IEnumerable<double>> ReadTestingInputFile(string filePath, double normalisation = 1.0d)
        {
            IList<IEnumerable<double>> csvInputs = new List<IEnumerable<double>>();
            using (var reader = new StreamReader(File.OpenRead(filePath)))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line == null || line.StartsWith("pixel"))
                    {
                        continue;
                    }

                    var raw = line.Split(',').Select(s => double.Parse(s) / normalisation);
                    csvInputs.Add(raw);
                }
            }
            return csvInputs;
        } 
    }
}