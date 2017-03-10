using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeafCore
{
    public class ConfusionMatrix
    {
        public float negativeCasesPredictedPositive = 0;
        public float positiveCasesPredictedpositive = 0;
        public float negativeCasesPredictedNegative = 0;
        public float postiveCasesPredictedNegative = 0;
        public String leafCategory; 
        public float precision = 0;
        public float recall = 0;
        public float accuracy = 0;
        public int totalImages = 0;
        public override string ToString()
        {
            return "LeafCategory" + leafCategory + " " + "Precision:" + precision + " Recall:" + recall + " Accuracy" + accuracy;
        }
    }
}
