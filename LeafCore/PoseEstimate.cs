using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeafCore
{
    public class PoseEstimate
    {
        public MDMatch Match { get; set; }
        public float Dx { get; set; }
        public float Dy { get; set; }
        public float Ds { get; set; }
        public float Do { get; set; }

        public double RMSE(PoseEstimate other)
        {
            if(other == null)
            {
                throw new ArgumentNullException("other");
            }

            double result = this.MSE(other);
            result = Math.Sqrt(result);
            return result;
        }

        public double MSE(PoseEstimate other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            double result = Math.Pow(this.Dx - other.Dx, 2);
            result += Math.Pow(this.Dy - other.Dy, 2);
            result += Math.Pow(this.Ds - other.Ds, 2);
            result += Math.Pow(this.Do - other.Do, 2);

            result /= 4.0;

            return result;
        }
    }
}
