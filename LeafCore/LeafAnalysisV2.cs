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
    public sealed class LeafAnalysisV2 : ILeafAnalysis
    {        
        BFMatcher matcher = new BFMatcher(DistanceType.L1);
        private IList<PreProcessedImage> trainingDataset = null;
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
            ITrainModel trainModel = new TrainedDataSetV2();
            trainModel.setFilePath(this.filePath);
            trainModel.setFileInList(this.eachLeafImageFolder);
            trainModel.ReadFileSystem();
            this.trainingDataset = trainModel.startTrainingModel();
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

        void ILeafAnalysis.compareQueryImageWithModel()
        {           
        }

        List<PerfromanceEvaluator> ILeafAnalysis.compareQueryImagesForPerfromance()
        { 
            List<PerfromanceEvaluator> allQueryImageResultSet = new List<PerfromanceEvaluator>();
            foreach(ImageCategory img in queryImages)
            {
                List<string> images = img.listOfImages;
                string leafCategory = img.leafCategory;
                foreach(string imagePath in images)
                {
                    try
                    {
                        PerfromanceEvaluator performanceMatrix = new PerfromanceEvaluator();
                        performanceMatrix.expectedLeafName = leafCategory;
                        performanceMatrix.result = this.QueryImage(imagePath, leafCategory);
                        allQueryImageResultSet.Add(performanceMatrix);
                        Console.WriteLine(imagePath + ":" + string.Join(":", performanceMatrix.result.Select(item => item.ToString())));
                    }
                    catch (Exception exception)
                    {
                        // TODO
                        //Console.WriteLine(exception.ToString());
                    }
                }
            }
            return allQueryImageResultSet;
        }

        private List<MatcherResult> QueryImage(string filePath, string expectedCategory)
        {
            using (PreProcess preProcessAlgorithm = new PreProcess(null))
            {
                PreProcessedImage preProcessedQueryImage = preProcessAlgorithm.Execute(filePath, expectedCategory);
                List<MatcherResult> finalResultSet = new List<MatcherResult>(this.trainingDataset.Count);
                DescriptorMatcher learningAlgo = new DescriptorMatcher();

                foreach (PreProcessedImage trainingData in this.trainingDataset)
                {                    
                    try
                    {                        
                        using (BFMatcher trainingMatcher = new BFMatcher(DistanceType.L1))
                        {
                            double areaRatio = trainingData.ContourArea / preProcessedQueryImage.ContourArea;
                            int contourDelta = trainingData.NumberOfContours - preProcessedQueryImage.NumberOfContours;
                            //if (areaRatio < 0.1 || areaRatio > 5)
                            //{
                            //    continue;
                            //}

                            //if(contourDelta > 100 || contourDelta < -100)
                            //{
                            //    continue;
                            //}

                            trainingMatcher.Add(trainingData.KeyPoints.Descriptor);
                            MatcherResult result = learningAlgo.knnMatch(preProcessedQueryImage.KeyPoints, trainingMatcher, trainingData.Category, 300, trainingData);
                            result.ContourRatio = CvInvoke.MatchShapes(trainingData.Contour, preProcessedQueryImage.Contour, Emgu.CV.CvEnum.ContoursMatchType.I3);
                            result.AreaRatio = areaRatio;
                            result.ContoursDelta = contourDelta;
                            //Console.WriteLine("QueryCategory: {0}, {1}", preProcessedQueryImage.Category, result.ToString());
                            finalResultSet.Add(result);
                        }
                    }
                    catch (Exception exception)
                    {
                        //Console.WriteLine(exception);
                    }
                }

                //IEnumerable<MatcherResult> highConfidenceResults = finalResultSet.Where(item => item.MatchingPoints >= 3);
                //finalResultSet = highConfidenceResults.ToList();
                //// If there are clusters with size great than or equal to 3 , then they have a very high probability of being correct.
                //if (highConfidenceResults.Any())
                //{
                //    //Console.WriteLine("High confidence:" + highConfidenceResults.Count() + " : " + highConfidenceResults.Max(item => item.MatchingPoints));
                //    finalResultSet = highConfidenceResults.ToList();
                //}
                //else
                //{
                //    finalResultSet = finalResultSet
                //                            .Where(item => item.MatchingPoints > 0)
                //                            .GroupBy(item => item.Category)
                //                            .Select(item =>
                //                                new MatcherResult()
                //                                {
                //                                    Category = item.Key,
                //                                    MatchingPoints = item.Sum(result => result.MatchingPoints),
                //                                    MatchDistance = item.Sum(result => result.MatchDistance)
                //                                })
                //                            .ToList();
                //}

                double totalWeightDistance = finalResultSet.Sum(item => item.MatchDistanceWeight);
                double totalWeightPoints = finalResultSet.Sum(item => item.MatchingPointsWeight);

                //finalResultSet = finalResultSet
                //                    .Where(item => item.MatchingPoints > 0)
                //                    .GroupBy(item => item.Category)
                //                    .Select(item =>
                //                        new MatcherResult()
                //                        {
                //                            Category = item.Key,
                //                            MatchingPoints = item.Sum(result => result.MatchingPoints * result.MatchingPointsWeight) / totalWeightPoints,
                //                            MatchDistance = item.Sum(result => result.MatchDistance * result.MatchDistanceWeight) / totalWeightDistance
                //                        })
                //                    .ToList();

                finalResultSet = finalResultSet
                                    .Where(item => item.MatchingPoints > 0)
                                    .GroupBy(item => item.Category)
                                    .Select(item =>
                                        new MatcherResult()
                                        {
                                            Category = item.Key,
                                            MatchingPoints = item.Sum(result => result.MatchingPointsWeight) / totalWeightPoints,
                                            MatchDistance = item.Sum(result => result.MatchDistanceWeight) / totalWeightDistance
                                        })
                                    .ToList();

                finalResultSet = finalResultSet.Where(item => item.MatchingPoints > 0).OrderByDescending(item => item, new MatcherResultWeightedComparer()).Take(3).ToList();
                return finalResultSet;
            }
        }

        public ResultImages query(QueryImageInfo queryImage)
        {
            throw new NotImplementedException();
            //using (QueryModel queryModel = new QueryModel())
            //{
            //    queryModel.ImageConversion(queryImage.image);
            //    queryModel.startComparingImages(categoryBfmatcherMapping);
            //    resultSet = queryModel.matchedResultSet;

            //    ResultImages result = new ResultImages();
            //    result.topResultsCategory = resultSet.Select(item => item.Category).ToList();
            //    return result;
            //}
        }

        //ResultImages ILeafAnalysis.query(QueryImageInfo queryImage)
        //{
        //    using (QueryModel queryModel = new QueryModel())
        //    {
        //        queryModel.ImageConversion(queryImage.image);
        //        queryModel.startComparingImages(categoryBfmatcherMapping);
        //        resultSet = queryModel.matchedResultSet;

        //        ResultImages result = new ResultImages();
        //        result.topResultsCategory = resultSet.Select(item => item.Category).ToList();
        //        return result;
        //    }
        //}

        public void Dispose()
        {
            if (this.trainingDataset != null)
            {
                foreach(PreProcessedImage image in this.trainingDataset)
                {
                    image.Dispose();
                }
            }

            if(this.matcher != null)
            {
                this.matcher.Dispose();
                this.matcher = null;
            }
        }
    }
    
}
