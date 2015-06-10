using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Filters.Photo;
using NeuralNetworkWeb.Models;
using NeuralNetworkWeb.Models.Serialisation;
using NeuralNetworkWeb.Providers;
using Neuron;
using Newtonsoft.Json;

namespace NeuralNetworkWeb.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";
            return View();
        }

        [HttpPost]
        public string Upload(ImageData upload)
        {
            Network networkModel = new NetworkDeserialiser().LoadNetwork(Server.MapPath(@"~/App_Data/network-7.json"));

            string raw = upload.base64Image;
            byte[] data = Convert.FromBase64String(raw.Substring(raw.IndexOf(',') + 1));
            Bitmap bitmap;
            const int imageSize = 28;
            using (var inStream = new MemoryStream(data, 0, data.Length))
            {
                using (var outstream = new MemoryStream())
                {
                    using (var imagefactory = new ImageProcessor.ImageFactory())
                    {
                        imagefactory.Load(inStream)
                            .Resize(new ResizeLayer(new Size(imageSize, imageSize), ResizeMode.Stretch))
                            .Filter(MatrixFilters.GreyScale)
                            .Filter(MatrixFilters.Invert)
                            .Flip(false, true)
                            .Flip()
                            .GaussianSharpen(2)
                            .Rotate(90)
                            .Save(outstream);
                    }
                    outstream.Position = 0;
                    bitmap = new Bitmap(outstream);
                }
            }

            var inputs = new List<double>();
            for (int x = 0; x < imageSize; x++)
            {
                for (int y = 0; y < imageSize; y++)
                {
                    Color pixel = bitmap.GetPixel(x, y);
                    inputs.Add(((pixel.R + pixel.G + pixel.B)/3.0d));
                }
            }

            //var stringinput = inputs.Select(i => i.ToString()).ToArray();
            //return string.Join(",", stringinput);
            var networkDriver = new NetworkDriver();
            List<SensoryInput> sensoryInputs = inputs.Select(i => new SensoryInput(i)).ToList();
            List<INeuron> inputLayer = networkDriver.CreateLayer(sensoryInputs.Cast<IInput>().ToList(),
                networkModel.InputLayer);
            List<INeuron> hiddenLayer = networkDriver.CreateLayer(inputLayer.Cast<IInput>().ToList(),
                networkModel.HiddenLayers[0]);
            List<INeuron> outputLayer = networkDriver.CreateLayer(hiddenLayer.Cast<IInput>().ToList(),
                networkModel.OutputLayer);

            networkDriver.UpdateNetwork(inputLayer, hiddenLayer, outputLayer);
            int result = networkDriver.GetMostLikelyAnswer(outputLayer);

            return string.Format("I think it looks like a {0}", result);
        }
    }
}
