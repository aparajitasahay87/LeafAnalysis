using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeafCore
{
    public interface ILeafAnalysis : IDisposable
    {
        List<MatcherResult> resultImage { get; }
        byte[] queryImageInBytes { set; }
        void setFilePath(string filePath);
        void setFileInList(List<string> eachLeafImageFolder);
        void trainModel();
        void compareQueryImageWithModel();
        ResultImages query(QueryImageInfo queryImage);
        List<PerfromanceEvaluator> compareQueryImagesForPerfromance();
    }
}
