using System;
using System.IO;
using System.Text;
using NeuralNetworkWeb.Models.Serialisation;
using Newtonsoft.Json;

namespace NeuralNetworkWeb.Providers
{
    public class NetworkDeserialiser
    {
        public Network LoadNetwork(string path)
        {
            var sb = new StringBuilder();
            using (var sr = new StreamReader(path))
            {
                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    sb.AppendLine(line);
                }
            }
            return JsonConvert.DeserializeObject<Network>(sb.ToString());
        }
    }
}