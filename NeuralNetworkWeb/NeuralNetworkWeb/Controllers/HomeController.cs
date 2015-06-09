using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Filters.Photo;
using ImageProcessor.Imaging.Formats;
using Filter = ImageProcessor.Processors.Filter;

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
            string raw = upload.base64Image;
            byte[] data = Convert.FromBase64String(raw.Substring(raw.IndexOf(',') + 1));
            Bitmap bitmap; 
            using (var inStream = new MemoryStream(data, 0, data.Length))
            {
                using (var outstream = new MemoryStream())
                {
                    using (var imagefactory = new ImageProcessor.ImageFactory())
                    {
                        imagefactory.Load(inStream)
                            .Resize(new ResizeLayer(new Size(28, 28), ResizeMode.Stretch))
                            .Filter(MatrixFilters.Invert)
                            .Flip()
                            .Rotate(90)
                            .Save(outstream);
                    }
                    outstream.Position = 0;
                    bitmap = new Bitmap(outstream);
                }
            }

            var result = new StringBuilder();
            for (int x = 0; x < 28; x++)
            {
                for (int y = 0; y < 28; y++)
                {
                    Color pixel = bitmap.GetPixel(x, y);
                    int value = ((pixel.R + pixel.G + pixel.B)/3);
                    result.Append(value.ToString(CultureInfo.InvariantCulture));
                    result.Append(',');
                }
            }

            return result.ToString();
        }
    }

    public class ImageData
    {
        public string base64Image { get; set; }
    }
}
