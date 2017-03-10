using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeafCore
{
    public class MatcherResult
    {
        public string Category { get; set; }
        public double MatchingPoints { get; set; }        
        public double MatchDistance { get; set; }
        public double AverageDistance { get; set; }

        public double MatchingPointsWeight { get; set; }
        // Square of average distance
        public double MatchDistanceWeight { get; set; }
        public double ContourRatio { get; set; }
        public double AreaRatio { get; set; }
        public int ContoursDelta { get; set; }

        public override string ToString()
        {
            return string.Format(
                        "Category: {0} , Points: {1}, Distance: {2}, AreaRatio: {3}, ContoursDelta: {4}", 
                        this.Category, 
                        this.MatchingPoints, 
                        this.MatchDistance, 
                        this.AreaRatio, 
                        this.ContoursDelta);
        }
    }
}
