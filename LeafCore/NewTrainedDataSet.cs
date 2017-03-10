using Emgu.CV;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeafCore
{
    public class NewTrainedDataSet
    {
        //Dictionary<String, BFMatcher> categoryBfmatcherMapping = new Dictionary<String, BFMatcher>();
        List<PerfromanceEvaluator> resultSet = new List<PerfromanceEvaluator>();
        List<ImageCategory> trainingModel = new List<ImageCategory>();
        List<ImageCategory> queryModel = new List<ImageCategory>();

        public void r()
        {
            Console.Write("chekc");
        }
        public Dictionary<String, BFMatcher> categoryBfmatcherMapping { get; set; }
        public void ReadFileSystem()

        {
            readFile();
        }

        public void startTrainingModel()
        {
            readEachCategoryForTraining();
        }

        private void readEachCategoryForTraining()
        {
            foreach (ImageCategory trainImage in trainingModel)
            {
                String imageCategory = trainImage.leafCategory;
                List<String> listOfEachImage = trainImage.listOfImages;
                trainModel(imageCategory, listOfEachImage);
            }
        }

        private void readFile()
        {
            string sourceDirectory = @"C:\Users\Funapp\Desktop\Capstone Project\leaf_dataset\leafsnap-dataset\subdataset";
            var Files = Directory.EnumerateFiles(sourceDirectory, "*.jpg", SearchOption.AllDirectories);
            List<String> eachLeafImageFolder = Directory.GetDirectories(sourceDirectory).ToList();
            foreach (String leafImage in eachLeafImageFolder)
            {
                ImageCategory category = new ImageCategory();
                String plantCategoryName = new DirectoryInfo(leafImage).Name;
                List<String> eachLeafSpecies = Directory.GetFiles(leafImage, "*.*", SearchOption.AllDirectories).ToList();
                category.leafCategory = plantCategoryName;
                category.listOfImages = eachLeafSpecies;
                computeNumberOfTrainingDataSet(category);
            }
        }

        private void computeNumberOfTrainingDataSet(ImageCategory LeafType)
        {
            List<String> trainingDataSet = new List<String>();
            List<String> queryDataSet = new List<String>();
            List<String> eachLeafSpecises = LeafType.listOfImages;
            int totalImages = eachLeafSpecises.Count;
            double trainingRatio = 0.8;
            int training = (int)Math.Ceiling(trainingRatio * totalImages);
            //Training model
            for (int i = 0; i < training; i++)
            {
                trainingDataSet.Add(eachLeafSpecises[i]);
            }
            //Ratio of query model.
            for (int j = training; j < eachLeafSpecises.Count; j++)
            {
                queryDataSet.Add(eachLeafSpecises[j]);
            }

            ImageCategory newImageTrainingSet = new ImageCategory();
            newImageTrainingSet.listOfImages = trainingDataSet;
            newImageTrainingSet.leafCategory = LeafType.leafCategory;
            trainingModel.Add(newImageTrainingSet);

            ImageCategory newImageQueryDataSet = new ImageCategory();
            newImageQueryDataSet.listOfImages = queryDataSet;
            newImageQueryDataSet.leafCategory = LeafType.leafCategory;
            queryModel.Add(newImageQueryDataSet);
        }

        private void trainModel(String plantCategoryName, List<String> eachLeafSpecies)
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
            foreach (String leaf in eachLeafSpecies)
            {
                using (Image<Bgr, Byte> image = loadImage(leaf))
                {
                    using (PreProcess preProcessAlgorithm = new PreProcess(image))
                    {
                        using (Image<Hsv, Byte> HSVImage = preProcessAlgorithm._ImageToHSV())
                        {
                            using (Image<Gray, Byte> grayScaleImage = preProcessAlgorithm._ImageToGrayScaleUsingConvert())
                            {
                                using (FeatureExtractAlgorithm featureSet = new FeatureExtractAlgorithm(grayScaleImage))
                                {
                                    KeyPoints leafDescriptor = featureSet.SIFTDescriptor();
                                    match.Add(leafDescriptor.Descriptor);
                                }
                            }
                        }
                    }
                }
            }
        }

        private Image<Bgr, Byte> loadImage(String imagePath)
        {
            //1)Load the image from file and resize it to display
            Image<Bgr, Byte> img =
               new Image<Bgr, byte>(imagePath)
               .Resize(400, 400, Emgu.CV.CvEnum.Inter.Linear, true);
            return img;
        }

    }
}

