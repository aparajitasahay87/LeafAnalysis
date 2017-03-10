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
using System.IO;
namespace LeafCore
{
    class PreProcess : IDisposable
    {
        private Image<Bgr, Byte> img;
        
        public PreProcess(Image<Bgr, Byte> image)
        {
            this.img = image;
        }

        public PreProcessedImage Execute(string imagePath, string category)
        {
            PreProcessedImage result = new PreProcessedImage();
            result.FilePath = imagePath;
            result.Category = category;
            using (PreProcess preProcessAlgorithm = new PreProcess(new Image<Bgr, Byte>(imagePath)))
            using (Image<Gray, Byte> grayScaleImage = preProcessAlgorithm._ImageToGrayScaleUsingConvert())
            {
                using (FeatureExtractAlgorithm featureSet = new FeatureExtractAlgorithm(grayScaleImage))
                using (Mat canny = new Mat())
                using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                {
                    KeyPoints descriptor = featureSet.SIFTDescriptor();
                    result.KeyPoints = descriptor;
                    result.ContourArea = 0;
                    CvInvoke.Canny(grayScaleImage, canny, 100, 50);
                    int[,] hierarchy = CvInvoke.FindContourTree(canny, contours, ChainApproxMethod.ChainApproxSimple);
                    result.NumberOfContours = contours.Size;
                    double maxArea = double.MinValue;
                    int maxIndex = -1;
                    for (int index = 0; index < contours.Size; index++)
                    {
                        double area = CvInvoke.ContourArea(contours[index]);
                        result.ContourArea += area;
                        if (area > maxArea)
                        {
                            maxArea = area;
                            maxIndex = index;
                        }
                    }

                    result.Contour = new VectorOfPoint();

                    CvInvoke.ApproxPolyDP(contours[maxIndex], result.Contour, CvInvoke.ArcLength(contours[maxIndex], true) * 0.02, true);
                    return result;
                }
            }            
        }

        /*
        * Used CvtColor method of Emgu CV- convert image from one color space to another color space.
        */
        public UMat _ImageToGrayScaleUsingCvtColor()
        {
            //3) Convert the image to grayscale.
            UMat uimage = new UMat();
            CvInvoke.CvtColor(img, uimage, ColorConversion.Bgr2Gray);
            //image1.Source = BitmapSourceConvert.ToBitmapSource(uimage);
            uimage.Save("toGrayImage.jpg");
            return uimage;
        }

        public void _ImageToHSVUsingCVTColor()
        {
            //using (UMat uimage = new UMat())
            //{
            //    using (Image<Hsv, byte> hsvScaleImage = this.img.Convert<Hsv, byte>())
            //    {
            //        using (Image<Gray, byte>[] channels = hsvScaleImage.Split())
            //        {
            //            //satImg = channels[1];
            //            //CvInvoke.InRange(channels[0], new Gray(40).MCvScalar, new Gray(60).MCvScalar, channels);
            //            //DeNioising and Adaptive threshold 

            //            //image1.Source = BitmapSourceConvert.ToBitmapSource(uimage);
            //            uimage.Save("toGrayImage.jpg");
            //        }
            //    }
            //}
        }
        /*
         * Used Convert method using system.Drawing - convert image from one color space to another color space.
        */
        public Image<Gray, byte> _ImageToGrayScaleUsingConvert()
        {
            Image<Gray, byte> grayScaleImage = this.img.Convert<Gray, byte>();
            grayScaleImage.Erode(500);
            grayScaleImage.Dilate(500);
            grayScaleImage.Save(@"C:\Projects\LeafService\Dialate2.jpg");
            //grayScaleImage.SmoothGaussian(10);


            //string folderName = @"C:\Projects\LeafService\BinaryImage";
            //string pathString = System.IO.Path.Combine(folderName, "GrayScale" + DateTime.UtcNow.Ticks);
            //System.IO.Directory.CreateDirectory(pathString);
            //if (Directory.Exists(pathString))
            //{
            //    string newFilePath = Path.Combine(pathString, "ImageGrayImage" + DateTime.UtcNow.Ticks);
            //    grayScaleImage.Save(pathString + ".jpg");
            //}

            return grayScaleImage;
        }

        public Image<Gray, byte> _ImageToGrayScaleUsingThresholding()
        {
           
            Image<Gray, byte> Im = this.img.Convert<Gray, byte>();       
            double OtsuThreshold = CvInvoke.Threshold(Im, Im, 0, 255, ThresholdType.Otsu);
            Image<Gray, byte> grayScaleImage = this.img.Convert<Gray, byte>().ThresholdBinary(new Gray(OtsuThreshold), new Gray(255));

            //string folderName = @"C:\Projects\LeafService\BinaryImage";
            //string pathString = System.IO.Path.Combine(folderName, "Binary" + DateTime.UtcNow.Ticks);
            //System.IO.Directory.CreateDirectory(pathString);
            //if(Directory.Exists(pathString))
            //{
            //    string newFilePath = Path.Combine(pathString, "BinaryImage" + DateTime.UtcNow.Ticks);
            //    grayScaleImage.Save(newFilePath + ".jpg");
            //}

            return grayScaleImage;
        }


        public Image<Hsv,byte> _ImageToHSV()
        {
            Image<Hsv, byte> hsvScaleImage = this.img.Convert<Hsv, byte>();
            hsvScaleImage.Save("hsvImage.jpg");
            return hsvScaleImage;
        }
        /*
         * Grab cut method - extract background image from foreground image
        */
        public Image<Gray, byte> grabCutAlgo(Image<Bgr, Byte> image)
        {
            //image.Convert<Bgr, Byte>();
            Image<Gray, byte> grabCutImage = image.Convert<Gray, Byte>();
            using (UMat grabCutResult = new UMat())
            using (UMat bgModel = new UMat())
            using (UMat fgModel = new UMat())
            {
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(100, 150, image.Width, image.Height);
                Image<Gray, byte> mask = null;
                try 
                {
                    mask = image.GrabCut(rect, 15);
                    mask = mask.ThresholdBinary(new Gray(2), new Gray(255));
                    //grabCutImage = image.Copy(mask);
                    //grabCutImage.Save("newGrabCutImage.jpg");
                    image.Copy(mask).Save("GrabCutLeaf.jpg");
                    return grabCutImage;
                }
                finally
                {
                    if(mask != null)
                    {
                        mask.Dispose();
                        mask = null;
                    }
                }
            }
        }

        public void BackgroundSubtraction(Image<Bgr, Byte> image)
        {
            image.Convert<Lab, Byte>();
            int height = image.Height;
            int width = image.Width;
        }

        public void foreGroundExtraction(Image<Bgr,Byte> image)
        {
            //4) canny edge detection 
            using (UMat cannyEdgeFeature = new UMat())
            {
                CvInvoke.Canny(image, cannyEdgeFeature, 50, 150, 3);
                cannyEdgeFeature.Save("edgeDetection.jpg");
                // find the contours
                using (VectorOfVectorOfPoint contoursDetected = new VectorOfVectorOfPoint())
                {
                    CvInvoke.FindContours(cannyEdgeFeature, contoursDetected, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
                    List<VectorOfPoint> contoursArray = new List<VectorOfPoint>();
                    int count = contoursDetected.Size;
                    for (int i = 0; i < count; i++)
                    {
                        using (VectorOfPoint currContour = contoursDetected[i])
                        {
                            contoursArray.Add(currContour);
                        }
                    }
                }
            }
            // you could also reuse img1 here
        }

        

        public void convertBinaryImage()
        {

        }

        public void DiallateIMage()
        {

        }

        public void ErodeImage()
        {
            img.Dilate(4);
            //img.MorphologyEx(4);
        }
        public void getProcessedImage()
        {
            
        }

        public void preProcessImage()
        {

        }

        public void Dispose()
        {
            if(this.img != null)
            {
                this.img.Dispose();
                this.img = null;
            }
        }
    }
}
