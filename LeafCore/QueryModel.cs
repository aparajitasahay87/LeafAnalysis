using Emgu.CV;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeafCore
{
    public class QueryModel : IQueryModel
    {
        private List<MatcherResult> finalResultSet = new List<MatcherResult>();
        private Image<Bgr, Byte> queryImage;
        public List<MatcherResult> matchedResultSet
        {
            get
            {
                return finalResultSet;
            }
        }

        Image<Bgr, byte> IQueryModel.queryImage
        {
            get
            {
                return queryImage;
            }
        }

        public void startComparingImages(Dictionary<String, BFMatcher> categoryBfmatcherMapping)
        {
            using (PreProcess preProcessAlgorithm = new PreProcess(queryImage))
            {
                using (Image<Gray, Byte> grayScaleImage = preProcessAlgorithm._ImageToGrayScaleUsingConvert())
                {
                    using (FeatureExtractAlgorithm featureSet = new FeatureExtractAlgorithm(grayScaleImage))
                    {
                        using (KeyPoints leafDescriptor = featureSet.SIFTDescriptor())
                        {
                            foreach (var pair in categoryBfmatcherMapping)
                            {
                                if(pair.Key == "abies_concolor")
                                {
                                    continue;
                                }

                                try
                                {
                                    DescriptorMatcher learningAlgo = new DescriptorMatcher();
                                    BFMatcher bfmatch = pair.Value;
                                    string leafCategory = pair.Key;
                                    MatcherResult result = learningAlgo.knnMatch(leafDescriptor, bfmatch, leafCategory);
                                    finalResultSet.Add(result);
                                }
                                catch(Exception exception)
                                {
                                    //TODO
                                    //Console.WriteLine(exception.ToString());
                                }
                            }
                            finalResultSet = finalResultSet.OrderByDescending(item => item,new MatcherResultComparer()).Take(3).ToList();
                        }
                    }
                }
            }
        }

        public void ImageConversion(byte[] queryimageData)
        {
            ConvertImage convertImage = new ConvertImage();
            this.queryImage = convertImage._byteArrayToImageConverter(queryimageData);
        }

        public void Dispose()
        {
            if (queryImage != null)
            {
                queryImage.Dispose();
                queryImage = null;
            }
        }
    }
}
