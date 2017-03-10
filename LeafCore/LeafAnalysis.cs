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
    public sealed class LeafAnalysis : ILeafAnalysis
    {
        private Dictionary<String,BFMatcher> categoryBfmatcherMapping = new Dictionary<String,BFMatcher>();
        private List<MatcherResult> resultSet = new List<MatcherResult>();
        List<ImageCategory> queryImages = new List<ImageCategory>();
        private byte[] imageInBytes;
        private string filePath = null;
        private List<String> eachLeafImageFolder = new List<String>();

        List<MatcherResult> ILeafAnalysis.resultImage
        {
            get
            {
                return resultSet;
            }
        }

        byte[] ILeafAnalysis.queryImageInBytes
        { 
            set
            {
                this.imageInBytes = value;
            }
        }

        void ILeafAnalysis.trainModel()
        {
            ITrainModel trainModel = new TrainedDataSet();
            trainModel.setFilePath(this.filePath);
            trainModel.setFileInList(this.eachLeafImageFolder);
            trainModel.ReadFileSystem();
            trainModel.startTrainingModel();
            categoryBfmatcherMapping = trainModel.ImagesBfmatcherMapping;
            this.queryImages = trainModel.getQueryModel();
        }

        void ILeafAnalysis.setFilePath(string filePath)
        {
            this.filePath = filePath;
        }

        void ILeafAnalysis.setFileInList(List<string> eachLeafImageFolder)
        {
            this.eachLeafImageFolder = eachLeafImageFolder;
        }

        public void trainModel()
        {
            /*TrainedDataSet trainingModel = new TrainedDataSet();
            trainingModel.ReadFileSystem();
            trainingModel.startTrainingModel();
            categoryBfmatcherMapping = trainingModel.ImagesBfmatcherMapping;
            this.queryImages = trainingModel.getQueryModel();
            */
        }

        void ILeafAnalysis.compareQueryImageWithModel()
        {
            //1. Convert the image to bitmap 
            using (QueryModel queryImage = new QueryModel())
            {
                queryImage.ImageConversion(imageInBytes);
                queryImage.startComparingImages(categoryBfmatcherMapping);
                resultSet = queryImage.matchedResultSet;
            }
        }

        List<PerfromanceEvaluator> ILeafAnalysis.compareQueryImagesForPerfromance()
        { 
            List<PerfromanceEvaluator> allQueryImageResultSet = new List<PerfromanceEvaluator>();
            List<MatcherResult> resultSetForPerf = new List<MatcherResult>();
            foreach(ImageCategory img in queryImages)
            {
                List<String> images = img.listOfImages;
                String leafCategory = img.leafCategory;
                foreach(String imagePath in images)
                {
                    QueryModel queryImage = null;
                    try
                    {
                        PerfromanceEvaluator performanceMatrix = new PerfromanceEvaluator();
                        performanceMatrix.expectedLeafName = leafCategory;
                        ConvertImage convertImage = new ConvertImage();
                        queryImage = new QueryModel();
                        using (Image imgBytes = Image.FromFile(imagePath))
                        {
                            byte[] imginByte = convertImage.imageToByteArrayConverter(imgBytes);
                            queryImage.ImageConversion(imginByte);
                            queryImage.startComparingImages(categoryBfmatcherMapping);
                            resultSetForPerf = queryImage.matchedResultSet;
                            performanceMatrix.result = resultSetForPerf;
                            allQueryImageResultSet.Add(performanceMatrix);
                            Console.WriteLine(imagePath + ":" + string.Join(":", resultSetForPerf.Select(item => item.ToString())));
                        }
                    }
                    catch(Exception exception)
                    {
                        // TODO
                        //Console.WriteLine(exception.ToString());
                    }
                    finally
                    {
                        if (queryImage != null)
                        {
                            queryImage.Dispose();
                            queryImage = null;
                        }
                    }
                }
            }
            return allQueryImageResultSet;
        }

        /*public LeafAnalysis()
        {
            TrainedDataSet trainingModel = new TrainedDataSet();
            trainingModel.ReadFileSystem();
            trainingModel.startTrainingModel();
            categoryBfmatcherMapping = trainingModel.ImagesBfmatcherMapping;
        }
        public LeafAnalysis(byte[] image)
        {
            //1. Convert the image to bitmap 
            QueryModel queryImage = new QueryModel();
            queryImage.startComparingImages();
            resultSet = queryImage.matchedResultSet;
        }
        */
        /*public void trainModel(String plantCategoryName, List<String> eachLeafSpecies) 
        {
            BFMatcher match;
            if (!categoryBfmatcherMapping.ContainsKey(plantCategoryName))
            {
                match = new BFMatcher(DistanceType.L2);
                categoryBfmatcherMapping.Add(plantCategoryName, match);
            }
            else
            {
                //Find values associated with key
                match = categoryBfmatcherMapping[plantCategoryName];
            }
            foreach(String leaf in eachLeafSpecies)
            {
                Image<Bgr, Byte> image = loadImage(leaf);
                PreProcess preProcessAlgorithm = new PreProcess(image);
                Image<Hsv, Byte> HSVImage = preProcessAlgorithm._ImageToHSV();
                Image<Gray, Byte> grayScaleImage = preProcessAlgorithm._ImageToGrayScaleUsingConvert();
                
                FeatureExtractAlgorithm featureSet = new FeatureExtractAlgorithm(grayScaleImage);
                UMat leafDescriptor = featureSet.SIFTDescriptor();
                match.Add(leafDescriptor);
            }
            
        }

        public void queryModel(String imageCategory,String imagePath)
        {
            PerfromanceEvaluator evaluateResult = new PerfromanceEvaluator();
            evaluateResult.expectedLeafName = imageCategory;
            List<MatcherResult> results = new List<MatcherResult>();
            Image<Bgr, Byte> image = loadImage(imagePath);
            PreProcess preProcessAlgorithm = new PreProcess(image);
            Image<Gray, Byte> grayScaleImage = preProcessAlgorithm._ImageToGrayScaleUsingConvert();
           
            FeatureExtractAlgorithm featureSet = new FeatureExtractAlgorithm(grayScaleImage);
            UMat leafDescriptor = featureSet.SIFTDescriptor();
            foreach (var pair in categoryBfmatcherMapping)
            {
                DescriptorMatcher learningAlgo = new DescriptorMatcher();
                BFMatcher bfmatch = pair.Value;
                string leafCategory = pair.Key;
                MatcherResult result = learningAlgo.knnMatch(leafDescriptor, bfmatch, leafCategory);
                results.Add(result);
            }
            results = results.OrderByDescending(item => item.MatchingPoints).Take(3).ToList();
            evaluateResult.result = results;
            resultSet.Add(evaluateResult);
        }
        public Image<Bgr, Byte> loadImage(String imagePath)
        {
            //1)Load the image from file and resize it to display
            Image<Bgr, Byte> img =
               new Image<Bgr, byte>(imagePath)
               .Resize(400, 400, Emgu.CV.CvEnum.Inter.Linear, true);
            return img;
           
        }

        public Image<Bgr, Byte> loadImage(byte[] image)
        {
            using (MemoryStream stream = new MemoryStream(image))
            {
                Bitmap bitmap = new Bitmap(stream);
                Image<Bgr, Byte> img =
                   new Image<Bgr, byte>(bitmap)
                   .Resize(400, 400, Emgu.CV.CvEnum.Inter.Linear, true);
                return img;
            }
        }

        public float findAccuracyOfSystem()
        {
            PerfromanceEvaluator evaluate = new PerfromanceEvaluator();
            float accuracyValue = evaluate.computeAccuracy(resultSet);
            return accuracyValue;
        }

        public void findPrecision()
        {
            PerfromanceEvaluator evaluate = new PerfromanceEvaluator();
            evaluate.computePrecisionAndRecall(resultSet);
            
        }
        //UMat umatGratScaleImage = preProcessAlgorithm._ImageToGrayScaleUsingCvtColor();
        //Image<Gray, Byte> _processedImageUsingGrabCut = preProcessAlgorithm.grabCutAlgo(img);
        //FeatureExtractAlgorithm featureSet = new FeatureExtractAlgorithm(grayScaleImage);
        //Image<Gray, byte> _cornerOfTheImage = featureSet.harrisCornerDetection();

        //FeatureExtractAlgorithm featureSetUsingUmatType = new FeatureExtractAlgorithm(umatGratScaleImage);
        //UMat _EdgeOfTheImage = featureSetUsingUmatType.cannyEdgeDetection();
        //UMat largestContour = new UMat();

        //VectorOfPoint result = FindLargestContour(_EdgeOfTheImage, largestContour);

        /*UMat contourImage = new UMat();
        MCvScalar color = new MCvScalar(0,0,255);

        CvInvoke.DrawContours(contourImage,result, 0, color);

        UMat trainingSiftdescriptors = featureSet.SIFTDescriptor(grayScaleImage);
        UMat SurfDescriptors = featureSet.SURFDescriptor(_processedImageUsingGrabCut);
        UMat OrbDescriptors = featureSet.ORBDescriptor(grayScaleImage);
        DescriptorMatcher matcher = new DescriptorMatcher();
        matcher.BfMatcher(trainingSiftdescriptors);
       */

        public void DetectAndCompute(Image<Bgr, byte> img)
        {
            using (PreProcess p = new PreProcess(img))
            {
                using (Image<Gray, Byte> grayScaleImage = p._ImageToGrayScaleUsingConvert())
                {
                    using (Image<Bgr, Byte> smoothImg = img.SmoothGaussian(5, 5, 1.5, 1.5))
                    {
                        using (Mat canny = new Mat())
                        using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                        {
                            CvInvoke.Canny(grayScaleImage, canny, 100, 50);
                            int[,] hierachy = CvInvoke.FindContourTree(canny, contours, ChainApproxMethod.ChainApproxSimple);
                            if (hierachy.GetLength(0) > 0)
                            {

                            }
                        }
                    }
                }
            }
        }

        public static VectorOfPoint FindLargestContour(IInputOutputArray cannyEdges, IInputOutputArray result)
        {
            int largest_contour_index = 0;
            double largest_area = 0;
            VectorOfPoint largestContour;

            using (Mat hierachy = new Mat())
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                IOutputArray hirarchy;

                CvInvoke.FindContours(cannyEdges, contours, hierachy, RetrType.Tree, ChainApproxMethod.ChainApproxNone);

                for (int i = 0; i < contours.Size; i++)
                {
                    MCvScalar color = new MCvScalar(0, 0, 255);

                    double a = CvInvoke.ContourArea(contours[i], false);  //  Find the area of contour
                    if (a > largest_area)
                    {
                        largest_area = a;
                        largest_contour_index = i;                //Store the index of largest contour
                    }

                    CvInvoke.DrawContours(result, contours, largest_contour_index, new MCvScalar(255, 0, 0));
                }

                CvInvoke.DrawContours(result, contours, largest_contour_index, new MCvScalar(0, 0, 255), 3, LineType.EightConnected, hierachy);
                largestContour = new VectorOfPoint(contours[largest_contour_index].ToArray());
                UMat t = new UMat();
                //t = result.GetOutputArray();
                
               
            }

            return largestContour;
        }

        public ResultImages query(QueryImageInfo queryImage)
        {
            using (QueryModel queryModel = new QueryModel())
            {
                queryModel.ImageConversion(queryImage.image);
                queryModel.startComparingImages(categoryBfmatcherMapping);
                resultSet = queryModel.matchedResultSet;

                ResultImages result = new ResultImages();
                result.topResultsCategory = resultSet.Select(item => item.Category).ToList();
                return result;
            }
        }

        
        ResultImages ILeafAnalysis.query(QueryImageInfo queryImage)
        {
            using (QueryModel queryModel = new QueryModel())
            {
                queryModel.ImageConversion(queryImage.image);
                queryModel.startComparingImages(categoryBfmatcherMapping);
                resultSet = queryModel.matchedResultSet;

                ResultImages result = new ResultImages();
                result.topResultsCategory = resultSet.Select(item => item.Category).ToList();
                return result;
            }
        }

        public void Dispose()
        {
            if(this.categoryBfmatcherMapping != null)
            {
                foreach(string matcherKey in this.categoryBfmatcherMapping.Keys)
                {
                    BFMatcher matcher = this.categoryBfmatcherMapping[matcherKey];
                    if(matcher != null)
                    {
                        matcher.Dispose();                        
                    }
                }

                this.categoryBfmatcherMapping.Clear();
            }
        }
    }
    
}
