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
    public class FeatureExtractAlgorithm : IDisposable
    {

        public Image<Gray, byte> preProcessedImageInGrayScale ;
        UMat preProcessedImage;

        public void setImage(Image<Gray, byte> preProcessedImageInGrayScale)
        {
            this.preProcessedImageInGrayScale = preProcessedImageInGrayScale;
        }
        
        public void getImage()
        {

        }
        public FeatureExtractAlgorithm()
        {

        }
        public FeatureExtractAlgorithm(Image<Gray, byte> preProcessedImageInGrayScale)
        {
            this.preProcessedImageInGrayScale = preProcessedImageInGrayScale;
        }

        public FeatureExtractAlgorithm(UMat preProcessedImage)
        {
            this.preProcessedImage = preProcessedImage;
        }

        public UMat cannyEdgeDetection()
        {
            //4) canny edge detection 
            UMat cannyEdgeFeature = new UMat();
            CvInvoke.Canny(preProcessedImage, cannyEdgeFeature , 50, 150, 3);
            //image1.Source = BitmapSourceConvert.ToBitmapSource(edge);
            cannyEdgeFeature.Save("edgeDetection.jpg");
            return cannyEdgeFeature;
        }

        public Image<Gray, Byte> harrisCornerDetection()
        {
            // original source image as grayscale
            Image<Gray, Byte> m_SourceImage = null;

            // raw corner strength image (must be 32-bit float)
            Image<Gray, float> m_CornerImage = null;

            // inverted thresholded corner strengths (for display)
            Image<Gray, Byte> m_ThresholdImage = null;

            // create and show source image as grayscale
            m_SourceImage = this.preProcessedImageInGrayScale;
            // create corner strength image and do Harris
            m_CornerImage = new Image<Gray, float>(m_SourceImage.Size);
            CvInvoke.CornerHarris(m_SourceImage, m_CornerImage, 3, 3, 0.01);

            // create and show inverted threshold image
            m_ThresholdImage = new Image<Gray, Byte>(m_SourceImage.Size);
            CvInvoke.Threshold(m_CornerImage, m_ThresholdImage, 0.0001,
                255.0, ThresholdType.BinaryInv);
            m_ThresholdImage.Save("harrisCornerDetection.jpg");
            return m_ThresholdImage;

        }

        public UMat SURFDescriptor()
        {
            double hessianThresh = 800;
            // public SURF(double hessianThresh, int nOctaves = 4, int nOctaveLayers = 2, bool extended = true, bool upright = false)
            SURF surfAlgo = new SURF(hessianThresh, 4, 2, true, false);
            VectorOfKeyPoint modelKeyPoints = new VectorOfKeyPoint();
            MKeyPoint[] mKeyPoints = surfAlgo.Detect(preProcessedImageInGrayScale);
            modelKeyPoints.Push(mKeyPoints);
            VectorOfKeyPoint observedKeyPoints = new VectorOfKeyPoint();
            UMat SurfDescriptors = new UMat();
            surfAlgo.DetectAndCompute(preProcessedImageInGrayScale, null, modelKeyPoints, SurfDescriptors, true);
            //image2.Source = BitmapSourceConvert.ToBitmapSource(modelDescriptors);
            SurfDescriptors.Save("SURFDetection.jpg");
            return SurfDescriptors;
        }

        public UMat ORBDescriptor()
        {
            //ORB Feature Descriptor 
            ORBDetector orbDetector = null;
            VectorOfKeyPoint modelKeyPointsOrb = null;
            VectorOfKeyPoint modelKeyPoints = null;
            try
            {
                orbDetector = new ORBDetector(500, 1, 8, 30, 0, 3, ORBDetector.ScoreType.Harris, 31, 20);
                modelKeyPoints = new VectorOfKeyPoint();
                modelKeyPointsOrb = new VectorOfKeyPoint();
                MKeyPoint[] mKeyPointsOrb = orbDetector.Detect(preProcessedImageInGrayScale);
                modelKeyPointsOrb.Push(mKeyPointsOrb);
                UMat ORBDescriptor = new UMat();
                orbDetector.DetectAndCompute(preProcessedImageInGrayScale, null, modelKeyPoints, ORBDescriptor, true);
                return ORBDescriptor;
            }
            finally
            {
                orbDetector.Dispose();
                modelKeyPointsOrb.Dispose();
                modelKeyPoints.Dispose();
            }
        }

        public KeyPoints SIFTDescriptor()
        {
            KeyPoints result = new KeyPoints();
            //SiFT Descriptor 
            SIFT siftAlgo = null;
            VectorOfKeyPoint modelKeyPointsSift = null;
            try
            {
                siftAlgo = new SIFT();
                modelKeyPointsSift = new VectorOfKeyPoint();

                MKeyPoint[] siftPoints = siftAlgo.Detect(preProcessedImageInGrayScale);
                modelKeyPointsSift.Push(siftPoints);
                UMat siftDescriptors = new UMat();
                siftAlgo.DetectAndCompute(preProcessedImageInGrayScale, null, modelKeyPointsSift, siftDescriptors, true);
                Image<Gray, Byte> outputImage = new Image<Gray, byte>(
                                                    preProcessedImageInGrayScale.Width,
                                                    preProcessedImageInGrayScale.Height);
                Features2DToolbox.DrawKeypoints(
                                        preProcessedImageInGrayScale,
                                        modelKeyPointsSift,
                                        outputImage,
                                        new Bgr(255, 255, 255),
                                        Features2DToolbox.KeypointDrawType.Default);

                string folderName = @"C:\Projects\LeafService\SiftImage";
                string pathString = System.IO.Path.Combine(folderName, "Sift" + DateTime.UtcNow.Ticks);
                System.IO.Directory.CreateDirectory(pathString);
                if (Directory.Exists(pathString))
                {
                    string newFilePath = Path.Combine(pathString, "SiftImage" + DateTime.UtcNow.Ticks);
                    outputImage.Save(folderName + ".jpg");
                    outputImage.Save(@"C:\Projects\LeafService\SIFTgray.jpg");

                }


                //outputImage.Save("sift.jpg");
                result.Descriptor = siftDescriptors;
                result.Points = siftPoints;
                return result;
            }
            finally
            {
                siftAlgo.Dispose();
                modelKeyPointsSift.Dispose();
            }
        }

        public void DrawSIFTDescriptor(string inputFile, string outputFile)
        {
            //SiFT Descriptor 
            SIFT siftAlgo = null;
            VectorOfKeyPoint modelKeyPointsSift = null;
            try
            {
                siftAlgo = new SIFT();
                modelKeyPointsSift = new VectorOfKeyPoint();

                using (Image<Bgr, byte> inputImage = new Image<Bgr, byte>(inputFile))
                {                    
                    MKeyPoint[] siftPoints = siftAlgo.Detect(inputImage);
                    modelKeyPointsSift.Push(siftPoints);
                    UMat siftDescriptors = new UMat();
                    siftAlgo.DetectAndCompute(inputImage, null, modelKeyPointsSift, siftDescriptors, true);
                    using (Image<Gray, Byte> outputImage = new Image<Gray, byte>(
                                                        inputImage.Width,
                                                        inputImage.Height))
                    {
                        Features2DToolbox.DrawKeypoints(
                                                inputImage,
                                                modelKeyPointsSift,
                                                outputImage,
                                                new Bgr(255, 255, 255),
                                                Features2DToolbox.KeypointDrawType.Default);
                        outputImage.Save(outputFile);
                    }
                }
            }
            finally
            {
                siftAlgo.Dispose();
                modelKeyPointsSift.Dispose();
            }
        }

        public void Dispose()
        {
            if(this.preProcessedImageInGrayScale != null)
            {
                this.preProcessedImageInGrayScale.Dispose();
                this.preProcessedImageInGrayScale = null;
            }
        }
    }
}
