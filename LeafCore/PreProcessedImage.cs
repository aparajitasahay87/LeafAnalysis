
using Emgu.CV.Util;
using System;

namespace LeafCore
{
    public class PreProcessedImage : IDisposable
    {
        public string FilePath { get; set; }
        public string Category { get; set; }
        public KeyPoints KeyPoints { get; set; }
        public VectorOfPoint Contour { get; set; }        
        public double ContourArea { get; set; }
        public int NumberOfContours { get; set; }

        public void Dispose()
        {
            if(this.KeyPoints != null)
            {
                this.KeyPoints.Dispose();
            }

            if(this.Contour != null)
            {
                this.Contour.Dispose();
            }
        }
    }
}