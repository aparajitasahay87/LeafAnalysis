using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeafCore
{
    public class QueryResultProperties
    {
        public string match;
        public int rank = -1;
        public int precision;
        public int recall; //sssssd
    }
}
