﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DigitRecognition
{
    public class InputFileReader
    {
        private const double Normalisation = 255.0d;

        public static IList<Tuple<string, IEnumerable<double>>> ReadTrainingInput()
        {
            IList<Tuple<string, IEnumerable<double>>> csvInputs = new List<Tuple<string, IEnumerable<double>>>();
            using (var reader = new StreamReader(File.OpenRead("training.csv")))
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
                        raw.Skip(1).Select(s => double.Parse(s)/ Normalisation));
                    csvInputs.Add(tuple);
                }
            }
            return csvInputs;
        }

        public static IList<Tuple<string, IEnumerable<double>>> ReadTestingInput()
        {
            IList<IEnumerable<double>> csvInputs = new List<IEnumerable<double>>();
            using (var reader = new StreamReader(File.OpenRead("testing.csv")))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line == null || line.StartsWith("pixel"))
                    {
                        continue;
                    }

                    var raw = line.Split(',').Select(s => double.Parse(s)/Normalisation);
                    csvInputs.Add(raw);
                }
            }

            IList<string> answers = new List<string>();
            using (var reader = new StreamReader(File.OpenRead("ANSWERS.txt")))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    answers.Add(line);
                }
            }

            return answers.Zip(csvInputs, (a, csv) => new Tuple<string, IEnumerable<double>>(a, csv)).ToList();
        }
    }
}