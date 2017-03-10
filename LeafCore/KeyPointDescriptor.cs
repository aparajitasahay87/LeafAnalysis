using Emgu.CV;
using Emgu.CV.Structure;
using System;

namespace LeafCore
{
    public class KeyPoints : IDisposable
    {
        public MKeyPoint[] Points { get; set; }
        public UMat Descriptor { get; set; }

        public void Dispose()
        {
            if(this.Descriptor != null)
            {
                this.Descriptor.Dispose();
            }
        }
    }
}