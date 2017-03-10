using LeafCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeafAnalysisConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] bannedCategories = new string[]
            {
                //"cedrus_atlantica",
                //"tsuga_canadensis"                
            };

            string[] includedCategories = new string[]
            {
                "acer_palmatum",
                "pinus_thunbergii"
            };

            DateTime startTime = DateTime.UtcNow;
            //FilePath file = new FilePath(@"C:\Projects\leafsnap-dataset\leafsnap-dataset~\dataset\images\field");
            FilePath file = new FilePath(@"C:\Projects\leafsnap-dataset\leafsnap-dataset~\dataset\images\DataSet1");
            //  FilePath file = new FilePath(@"C:\Projects\leafsnap-dataset\leafsnap-dataset~\dataset\images\field\platanus_acerifolia");
            FeatureExtractAlgorithm algorithm = new FeatureExtractAlgorithm();
            algorithm.DrawSIFTDescriptor(
                            @"C:\Projects\leafsnap-dataset\leafsnap-dataset~\dataset\images\quercus\quercus_bicolor\1.jpg",
                            @"C:\Projects\leafsnap-dataset\leafsnap-dataset~\dataset\images\IntermediateImages\quercus_1.jpg");

            algorithm.DrawSIFTDescriptor(
                            @"C:\Projects\leafsnap-dataset\leafsnap-dataset~\dataset\images\quercus\quercus_bicolor\shadow.jpg",
                            @"C:\Projects\leafsnap-dataset\leafsnap-dataset~\dataset\images\IntermediateImages\siftquercus.jpg");

            string resultFile = string.Format("{0}{1}.csv", @"C:\Projects\LeafService\PerformanceResult_", DateTime.UtcNow.Ticks);
            List<string> listOfFiles = file.getFileInList().Where(item => !bannedCategories.Contains(item.Split('\\').Last())).ToList();
            int numberOfSpecies = listOfFiles.Count;
            int start = 0;
            int pageSize = 20;
            while (start < numberOfSpecies)
            {
                List<string> fileNames = listOfFiles.Skip(start)
                    .Take(pageSize).ToList();
                leafAnalysis(fileNames, resultFile);

                start += pageSize;
            }

            Console.WriteLine("Total Time: " + TimeSpan.FromTicks((DateTime.UtcNow - startTime).Ticks).TotalMinutes + " minutes");
        }

        static void leafAnalysis(List<string> fileNames, string resultFile)
        {
            ILeafAnalysis leaf = null;
            try
            {
                leaf = new LeafAnalysisV2();
                leaf.setFileInList(fileNames);
                leaf.trainModel();

                //QueryImageInfo queryImage = new QueryImageInfo()
                //{
                //    image = File.ReadAllBytes(@"C:\Projects\leafsnap-dataset\leafsnap-dataset~\dataset\images\field\acer_ginnala\13291762519532.jpg")
                //};

                //ResultImages queryResult = leaf.query(queryImage);

                List<PerfromanceEvaluator> allQueryImageResultSet = leaf.compareQueryImagesForPerfromance();
                //compute performance........
                PerfromanceEvaluator computePerformance = new PerfromanceEvaluator();
                
                List<float> perfVal = computePerformance.computePrecisionAndRecall(allQueryImageResultSet);
                List<ConfusionMatrix> performanceMatrix = computePerformance.getPerformanceMatrix();
                foreach (ConfusionMatrix result in performanceMatrix)
                {
                    Console.WriteLine(result);
                }

                Console.WriteLine(
                            "Precision: {0} Recall: {1} Accuracy: {2}",
                            performanceMatrix.Average(item => item.precision),
                            performanceMatrix.Average(item => item.recall),
                            100* performanceMatrix.Sum(item =>
                                                    item.positiveCasesPredictedpositive) /
                            (performanceMatrix.First().negativeCasesPredictedNegative +
                            performanceMatrix.First().negativeCasesPredictedPositive +
                            performanceMatrix.First().positiveCasesPredictedpositive +
                            performanceMatrix.First().postiveCasesPredictedNegative));

                using (
                    
                    StreamWriter writer = new StreamWriter(resultFile, true))
                {
                    StringBuilder sb1 = new StringBuilder();
                    sb1.Append(perfVal[2]);
                    foreach (ConfusionMatrix perf in performanceMatrix)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(perf.leafCategory + "," + perf.precision +
                               "," + perf.recall + "," + perf.accuracy + 
                               "," + perf.positiveCasesPredictedpositive + "," + perf.postiveCasesPredictedNegative +
                               "," +  "," + perf.negativeCasesPredictedPositive + "," + perf.totalImages + "," + perf.negativeCasesPredictedNegative  );
                        writer.WriteLine(sb);
                    }

                    writer.Close();
               }
            }
            finally
            {
                if (leaf != null)
                {
                    leaf.Dispose();
                    leaf = null;

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
        }
    }
}
