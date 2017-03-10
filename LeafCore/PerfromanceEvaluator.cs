using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeafCore
{
    public class PerfromanceEvaluator
    {
        public List<MatcherResult> result;
        public String expectedLeafName;
        List<ConfusionMatrix> performanceMatrix = new List<ConfusionMatrix>();
        
      
        /// <summary>
        /// 
        /// </summary>
        /// <param name="resultSet"></param>
        /// <returns></returns>
        public List<float> computePrecisionAndRecall(List<PerfromanceEvaluator> resultSet)
        {
           List<float> perfVal = new List<float>();
           List<String> categories = new List<String>();
           categories = resultSet.Select(x => x.expectedLeafName).Distinct().ToList();
            int totalImage = resultSet.Count;
           float count = 0;       
           foreach(String category in categories)
           {
                ConfusionMatrix matrix = new ConfusionMatrix();
                //Option 1 : To compute precision and recall.
                /*List<PerfromanceEvaluator> totalNumberOfLeafInCategory = resultSet.FindAll( x => x.expectedLeafName.Equals(item));
                int ExpectedCount = totalNumberOfLeafInCategory.Count;
                List<PerfromanceEvaluator> truePositiveResult = totalNumberOfLeafInCategory.FindAll(x => x.result[0].Category.Equals(item));

                int truePositive = truePositiveResult.Count;
                float recall = (float)truePositive / (float)ExpectedCount;

                List<PerfromanceEvaluator> resultReturnedWithSameCategoryName = resultSet.FindAll(x => x.result[0].Category.Equals(item));

                int resultReturnedWithSameCategoryNameCount = resultReturnedWithSameCategoryName.Count;
               
                float precision = (float)truePositive / (float)resultReturnedWithSameCategoryNameCount ;
                */
                float numberOfQueryImages = resultSet.FindAll(x => x.expectedLeafName.Equals(category)).Count;
               
                float predictedPositiveSet = resultSet.FindAll(x => x.result.Any() && x.result[0].Category.Equals(category)).Count;
                //Any(y => y.Category.Equals(category))
                float accuracy = predictedPositiveSet / numberOfQueryImages;
                float negativeCasesPredictedPositive = 0;
                float positiveCasesPredictedpositive = 0;
                float negativeCasesPredictedNegative = 0;
                float postiveCasesPredictedNegative = 0;
                int totalCountOfImagesInCategory = resultSet.FindAll(x => x.expectedLeafName.Equals(category)).Count;

                //positiveCasesPredictedpositive = resultSet.FindAll(x => x.expectedLeafName.Equals(category) &&
                //                                                        x.result.Any() &&
                //                                                      x.result[0].Category.Equals(category)).Count;

                positiveCasesPredictedpositive = resultSet.FindAll(x => x.expectedLeafName.Equals(category) &&
                                                                        x.result.Any() &&
                                                                        x.result.Any(y => y.Category.Equals(category))).Count;

                //computing accuracy of entire system
                count += positiveCasesPredictedpositive;
                //x.result.Any(y => y.Category.Equals(category))
                //x.result[0].Category.Equals(category)
                negativeCasesPredictedPositive = resultSet.FindAll(x => x.expectedLeafName != category &&
                                                                        x.result.Any() &&
                                                                        x.result[0].Category.Equals(category)).Count;
               
                float precisonForEachImageCategory  =  positiveCasesPredictedpositive / (positiveCasesPredictedpositive + negativeCasesPredictedPositive);

                negativeCasesPredictedNegative = resultSet.FindAll(x => x.expectedLeafName!=category && (!x.result.Any() || x.result[0].Category!=category)).Count;

                //postiveCasesPredictedNegative = resultSet.FindAll(x => x.expectedLeafName.Equals(category) &&
                //                                                       (!x.result.Any() ||
                //                                                       !x.result[0].Category.Equals(category)))
                //                                         .Count;
                postiveCasesPredictedNegative = resultSet.FindAll(x => x.expectedLeafName.Equals(category) &&
                                                                       (!x.result.Any() ||
                                                                       !x.result.Any(y => y.Category.Equals(category))))
                                                         .Count;

                //!x.result.Any(y => y.Category.Equals(category)))
                //!x.result[0].Category.Equals(category)

                float recallForEachImageCategory = positiveCasesPredictedpositive / 
                                                   (postiveCasesPredictedNegative + positiveCasesPredictedpositive);
                if (float.IsNaN(recallForEachImageCategory))
                {
                    recallForEachImageCategory = 0;
                }

                if (float.IsNaN(precisonForEachImageCategory))
                {
                    precisonForEachImageCategory = 0;
                }
                matrix.leafCategory = category;
                matrix.negativeCasesPredictedNegative = negativeCasesPredictedNegative;
                matrix.negativeCasesPredictedPositive = negativeCasesPredictedPositive;
                matrix.positiveCasesPredictedpositive = positiveCasesPredictedpositive;
                matrix.postiveCasesPredictedNegative = postiveCasesPredictedNegative;
                matrix.totalImages = totalCountOfImagesInCategory;
                matrix.precision = precisonForEachImageCategory * 100;
                matrix.recall = recallForEachImageCategory * 100;
                matrix.accuracy = accuracy * 100;
                performanceMatrix.Add(matrix);
           }
            float precisionVal = averageOfAllImageCategoryPrecision(performanceMatrix);
            float recallVal = averageOfAllImageCategoryRecall(performanceMatrix);
           
            perfVal.Add(precisionVal);
            perfVal.Add(recallVal);
            float totalAccuracy = count / totalImage;
            perfVal.Add(totalAccuracy);
            return perfVal;
        }

        public List<ConfusionMatrix> getPerformanceMatrix()
        {
            return performanceMatrix;
        }
        public float averageOfAllImageCategoryPrecision(List<ConfusionMatrix> performanceMatrix)
        {
            float sum = 0;
            int totalCount = performanceMatrix.Count;
            float avergePrecisionValue = 0;
            foreach(ConfusionMatrix eachImage in performanceMatrix)
            {
                sum = sum + eachImage.precision;
            }
            avergePrecisionValue = sum / (float)totalCount ;
            return avergePrecisionValue;
        }
        public float averageOfAllImageCategoryRecall(List<ConfusionMatrix> performanceMatrix)
        {
            float sum = 0;
            int totalCount = performanceMatrix.Count;
            float avergeRecallValue = 0;
            foreach (ConfusionMatrix eachImage in performanceMatrix)
            {
                sum = sum + eachImage.recall;
            }
            avergeRecallValue = sum / (float)totalCount;
            return avergeRecallValue;
        }       

    }
}

/*Things 
1. Decision fusion , weight <- literature survey.
2.Multi class classifier , single model multi label
2. 
3.
*/