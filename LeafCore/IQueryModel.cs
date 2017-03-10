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
    interface IQueryModel : IDisposable
    {
        void ImageConversion(byte[] imageInBytes);
        void startComparingImages(Dictionary<String, BFMatcher> categoryBfmatcherMapping);
        List<MatcherResult> matchedResultSet { get; }
        Image<Bgr,Byte> queryImage { get;  }
    }
}
