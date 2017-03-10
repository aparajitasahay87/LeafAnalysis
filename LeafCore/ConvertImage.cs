using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Features2D;
using Emgu.CV.XFeatures2D;
using System.Collections;
using System.IO;

namespace LeafCore
{
    public class ConvertImage
    {
        public byte[] imageToByteArrayConverter(Image image)
        {
            ImageConverter img = new ImageConverter();
            return (byte[])img.ConvertTo(image, typeof(byte[]));

        }
        public Image byteArrayToImageConverter(byte[] imageData)
        {
            Image newImage;
            try
            {
                using (MemoryStream ms = new MemoryStream(imageData, 0, imageData.Length))
                {
                    ms.Write(imageData, 0, imageData.Length);
                    newImage = Image.FromStream(ms, true);
                    return newImage;
                }
            }
            finally { newImage = null; }
            
        }
        public Image<Bgr, Byte> _byteArrayToImageConverter(byte[] imgData)
        {
            Bitmap bitmap = null;
            try
            {
                using (MemoryStream stream = new MemoryStream(imgData))
                {
                    bitmap = new Bitmap(stream);
                    Image<Bgr, Byte> img = null;
                    try
                    {
                        img = new Image<Bgr, byte>(bitmap)
                                .Resize(400, 400, Emgu.CV.CvEnum.Inter.Linear, true);
                        return img;
                    }
                    catch
                    {
                        if(img != null)
                        {
                            img.Dispose();
                            img = null;
                        }
                        throw;
                    }
                }
            }
            finally
            {
                if (bitmap != null)
                {
                    bitmap.Dispose();
                    bitmap = null;
                }
            }
        }
    }
}
