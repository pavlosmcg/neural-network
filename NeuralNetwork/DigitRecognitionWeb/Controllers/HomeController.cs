using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using DigitRecognitionWeb.Models;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Filters.Photo;
using NeuralNet;

namespace DigitRecognitionWeb.Controllers
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
            string path = Server.MapPath(@"~/App_Data/network-2.json");
            var activation = new SigmoidActivation();
            var outputList = Enumerable.Range(0, 10).Select(i => i.ToString(CultureInfo.InvariantCulture)).ToList();
            var network = new Network(activation, 784, outputList, path);

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
                            .Flip(true)
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
                    inputs.Add(((pixel.R + pixel.G + pixel.B)/3.0d)/255.0d);
                }
            }

            network.UpdateNetwork(inputs);
            string result = network.GetMostLikelyAnswer();

            return string.Format("I think it looks like a {0}", result);
        }
    }
}
