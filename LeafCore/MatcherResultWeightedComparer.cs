using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeafCore
{
    class MatcherResultWeightedComparer : IComparer<MatcherResult>
    {
        public int Compare(MatcherResult x, MatcherResult y)
        {
            double xScore = (x.MatchingPoints + x.MatchDistance) / 2;
            double yScore = (y.MatchingPoints + y.MatchDistance) / 2;

            return xScore.CompareTo(yScore);

            //return x.MatchDistance.CompareTo(y.MatchDistance);
        }
    }
}
