using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeafCore
{
    interface IPerfromanceEvaluation
    {
        void findAccuracy();
        void findPrecisionAndRecall();
        float accuracyVal { get; }
        void compute(); 
    }
}
