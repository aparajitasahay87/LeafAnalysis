using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeafCore
{
    class MatcherResultComparer : IComparer<MatcherResult>
    {
        public int Compare(MatcherResult x, MatcherResult y)
        {
            // More matching points should be given more preferenace
            int result = x.MatchingPoints.CompareTo(y.MatchingPoints);
            if (result != 0)
            {
                return result;
            }

            double avg1 = x.MatchingPoints == 0 ? double.MaxValue : x.MatchDistance / x.MatchingPoints;
            double avg2 = y.MatchingPoints == 0 ? double.MaxValue : y.MatchDistance / y.MatchingPoints;

            // If matching points are the same, the look at average distance. Less distance the better
            return avg2.CompareTo(avg1);
        }
    }
}
