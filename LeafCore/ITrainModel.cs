using Emgu.CV.Features2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeafCore
{
    interface ITrainModel
    {
       void setFilePath(string filePath);
       void setFileInList(List<string> eachLeafImageFolder);
       void ReadFileSystem();
       IList<PreProcessedImage> startTrainingModel();
       Dictionary<String, BFMatcher> ImagesBfmatcherMapping { get; }
       List<ImageCategory> getQueryModel();
    }
}
