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
    public class TrainedDataSetV2 : ITrainModel
    {
        List<PerfromanceEvaluator> resultSet = new List<PerfromanceEvaluator>();
        List<ImageCategory> trainingModel = new List<ImageCategory>();
        List<ImageCategory> queryModel = new List<ImageCategory>();
        private Dictionary<String, BFMatcher> categoryBfmatcherMapping = new Dictionary<string, BFMatcher>();
        private string filePath = null;
        private List<String> eachLeafImageFolder = new List<String>();
        
        void ITrainModel.setFilePath(string filePath)
        {
            this.filePath = filePath;
        }
        void ITrainModel.setFileInList(List<string> eachLeafImageFolder)
        {
            this.eachLeafImageFolder = eachLeafImageFolder;
        }
        void ITrainModel.ReadFileSystem()
        {
            List<String> eachLeafImageFolder = this.eachLeafImageFolder;
            string sourceDirectory = this.filePath;
            //@"C:\Projects\leafsnap-dataset\leafsnap-dataset~\dataset\images\field";
            //IEnumerable<String> Files = Directory.EnumerateFiles(sourceDirectory, "*.jpg", SearchOption.AllDirectories);
            //List<String> eachLeafImageFolder = Directory.GetDirectories(sourceDirectory).ToList().GetRange(0,12);
            int count = 0;
            int totalCount = eachLeafImageFolder.Count;
            foreach (String leafImage in eachLeafImageFolder)
            {
                count++;
                ImageCategory category = new ImageCategory();
                String plantCategoryName = new DirectoryInfo(leafImage).Name;
                List<String> eachLeafSpecies = Directory.GetFiles(leafImage, "*.*", SearchOption.AllDirectories).ToList();
                category.leafCategory = plantCategoryName;
                category.listOfImages = eachLeafSpecies;
                computeNumberOfTrainingDataSet(category);
                Console.WriteLine("{0} category of {1} containing {2} leaves", count, totalCount, eachLeafSpecies.Count);
            }
        }

        IList<PreProcessedImage> ITrainModel.startTrainingModel()
        {
            IList<PreProcessedImage> result = new List<PreProcessedImage>(this.trainingModel.Sum(item => item.listOfImages.Count));

            using (PreProcess preProcesAlgorithm = new PreProcess(null))
            {
                int count = 0;
                foreach (ImageCategory imageCategory in this.trainingModel)
                {
                    Console.WriteLine(count++ + " Training category: " + imageCategory.leafCategory + "total files: " + imageCategory.listOfImages.Count);
                    foreach (string imagePath in imageCategory.listOfImages)
                    {
                        PreProcessedImage preProcessedImage = preProcesAlgorithm.Execute(imagePath, imageCategory.leafCategory);
                        result.Add(preProcessedImage);
                    }
                }
            }

            return result;
        }
      

        Dictionary<String, BFMatcher> ITrainModel.ImagesBfmatcherMapping
        {
            get
            {
                return categoryBfmatcherMapping;
            }
        }

        List<ImageCategory> ITrainModel.getQueryModel()
        {
            return this.queryModel;
        }


        public Dictionary<String, BFMatcher> ImagesBfmatcherMapping
        {
            get
            {
                return categoryBfmatcherMapping;
            }
        }

        //public void ReadFileSystem()
        //{
        //    readFile();
        //}
        public void startTrainingModel()
        {
            readEachCategoryForTraining();
        }

        private void readEachCategoryForTraining()
        {
            int count = 0;
            int totalCount = trainingModel.Count;

            foreach (ImageCategory trainImage in trainingModel)
            {
                String imageCategory = trainImage.leafCategory;
                List<String> listOfEachImage = trainImage.listOfImages;
                Console.WriteLine(
                            "Train model {0} :{1} of total {2} containing images {3}", 
                            imageCategory, 
                            count++, 
                            totalCount, 
                            listOfEachImage.Count);
                trainModel(imageCategory, listOfEachImage);
            }
        }

        //private void readFile()
        //{
        //    string sourceDirectory = filePath;
        //    //@"C:\Projects\leafsnap-dataset\leafsnap-dataset~\dataset\images\field";
        //    IEnumerable<String> Files = Directory.EnumerateFiles(sourceDirectory, "*.jpg", SearchOption.AllDirectories);
        //    List<String> eachLeafImageFolder = Directory.GetDirectories(sourceDirectory).Take(10).ToList();
        //    int count = 0;
        //    int totalCount = eachLeafImageFolder.Count;
        //    foreach (String leafImage in eachLeafImageFolder)
        //    {
        //        count++;
        //        ImageCategory category = new ImageCategory();
        //        String plantCategoryName = new DirectoryInfo(leafImage).Name;
        //        List<String> eachLeafSpecies = Directory.GetFiles(leafImage, "*.*", SearchOption.AllDirectories).ToList();
        //        category.leafCategory = plantCategoryName;
        //        category.listOfImages = eachLeafSpecies;
        //        computeNumberOfTrainingDataSet(category);
        //        Console.WriteLine("{0} category of {1} containing {2} leaves", count, totalCount, eachLeafSpecies.Count);
        //    }
        //}

        private void computeNumberOfTrainingDataSet(ImageCategory LeafType)
        {
            List<String> trainingDataSet = new List<String>();
            List<String> queryDataSet = new List<String>();
            List<String> eachLeafSpecises = LeafType.listOfImages;
            int totalImages = eachLeafSpecises.Count;
            
            // Ensure that each category has balanced instances.
            totalImages = Math.Min(totalImages, 20);

            double trainingRatio = 0.75;
            int training = (int)Math.Ceiling(trainingRatio * totalImages);
            //Training model
            for (int i = 0; i < training; i++)
            {
                trainingDataSet.Add(eachLeafSpecises[i]);
            }
            //Ratio of query model.
            for (int j = training; j < totalImages; j++)
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
                match = new  BFMatcher(DistanceType.L1);
                categoryBfmatcherMapping.Add(plantCategoryName, match);
            }
            else
            {
                //Find values associated with key
                match = categoryBfmatcherMapping[plantCategoryName];
            }
            foreach (String leaf in eachLeafSpecies)
            {
                PreProcess preProcessAlgorithm = null;
                FeatureExtractAlgorithm featureSet = null;
                try
                {
                    //Console.WriteLine(leaf + "\n");
                    Image<Bgr, Byte> image = loadImage(leaf);
                    preProcessAlgorithm = new PreProcess(image);
                    //Image<Hsv, Byte> HSVImage = preProcessAlgorithm._ImageToHSV();
                    Image<Gray, Byte> grayScaleImage = preProcessAlgorithm._ImageToGrayScaleUsingConvert();

                    featureSet = new FeatureExtractAlgorithm(grayScaleImage);
                    KeyPoints leafDescriptor = featureSet.SIFTDescriptor();
                    match.Add(leafDescriptor.Descriptor);
                }
                finally
                {
                    if (preProcessAlgorithm != null)
                    {
                        preProcessAlgorithm.Dispose();
                    }

                    if (featureSet != null)
                    {
                        featureSet.Dispose();
                    }
                }
            }
        }

        private Image<Bgr, Byte> loadImage(String imagePath)
        {
            //1)Load the image from file and resize it to display
            Image<Bgr, Byte> img =
               new Image<Bgr, byte>(imagePath);
               //.Resize(400, 400, Emgu.CV.CvEnum.Inter.Linear, true);
            return img;
        }

        public List<ImageCategory> getQueryModel()
        {
            return queryModel;
        }

    }
}
