using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Features2D;
using Emgu.CV.XFeatures2D;
namespace LeafCore
{
    public class DescriptorMatcher
    {
        String category = null;        
     
        public void setCategoryOfLeaf(String leafCategory)
        {
            this.category = leafCategory;
        }
        public String getCategoryOfLeaf()
        {
            return category;
        }
        public BFMatcher BfMatcher(UMat descriptors, BFMatcher matchShift)
        {
            matchShift.Add(descriptors);
            return matchShift;
        }

        public MatcherResult knnMatch(KeyPoints queryDescriptor, BFMatcher matcher, string leafCategory, int distanceCutoff = int.MaxValue, PreProcessedImage trainingData = null)
        {
            MatcherResult result = new MatcherResult();
            result.Category = leafCategory;
            
            using (VectorOfVectorOfDMatch vectorMatchesForSift = new VectorOfVectorOfDMatch())
            {
                matcher.KnnMatch(queryDescriptor.Descriptor, vectorMatchesForSift, 2, null);
                
                int numberOfMatches = 0;
                
                Dictionary<int, int> counts = new Dictionary<int, int>();
                List<MDMatch> goodMatches = new List<MDMatch>(vectorMatchesForSift.Size);
                for (int i = 0; i < vectorMatchesForSift.Size; i++)
                {
                    // Do Ratio test: Reject matches where ratio of closest match with second closest if greater than 0.8
                    if(
                        (vectorMatchesForSift[i].Size == 1 ||
                        vectorMatchesForSift[i][0].Distance < 0.75*vectorMatchesForSift[i][1].Distance) &&
                        vectorMatchesForSift[i][0].Distance < distanceCutoff)
                    {
                        goodMatches.Add(vectorMatchesForSift[i][0]);
                        numberOfMatches++;
                    }
                }

                //goodMatches = clusterBasedOnPoseEstimation(goodMatches, queryDescriptor, trainingData);

                int maxResults = int.MaxValue;
                result.MatchingPoints = goodMatches.Count;
                if (!goodMatches.Any())
                {
                    result.MatchDistance = float.MaxValue;
                    result.AverageDistance = 0;
                    result.MatchDistanceWeight = 0;
                    result.MatchingPointsWeight = 0;
                }
                else
                {
                    result.MatchDistance = goodMatches.OrderBy(item => item.Distance).Take(maxResults).Sum(item => item.Distance);
                    result.AverageDistance = result.MatchDistance / result.MatchingPoints;
                    result.MatchDistanceWeight = 1 / Math.Pow(result.AverageDistance, 2);
                    result.MatchingPointsWeight = result.MatchingPoints * result.MatchingPoints;
                }
            }
            return result;
        }

        private List<MDMatch> clusterBasedOnPoseEstimation(List<MDMatch> matches, KeyPoints queryKeyPoints, PreProcessedImage trainingData)
        {
            // If no training data is provided then we cannot compute pose (LeafAnalysisV1), hence just return back the original set
            if(trainingData == null ||
                matches == null ||
                !matches.Any())
            {
                return matches;
            }            

            Dictionary<MDMatch, List<MDMatch>> clusters = new Dictionary<MDMatch, List<MDMatch>>(matches.Count);
            List<PoseEstimate> poseEstimates = new List<PoseEstimate>(matches.Count);

            foreach(MDMatch match in matches)
            {
                MKeyPoint queryKeyPoint = queryKeyPoints.Points[match.QueryIdx];
                MKeyPoint trainingKeyPoint = trainingData.KeyPoints.Points[match.TrainIdx];

                PoseEstimate estimate = new PoseEstimate();
                estimate.Match = match;
                estimate.Dx = trainingKeyPoint.Point.X - queryKeyPoint.Point.X;
                estimate.Dy = trainingKeyPoint.Point.Y - queryKeyPoint.Point.Y;
                estimate.Ds = trainingKeyPoint.Octave / queryKeyPoint.Octave;
                estimate.Do = trainingKeyPoint.Angle - queryKeyPoint.Angle;

                poseEstimates.Add(estimate);
                // Initialize clusters for each individual match
                // Next we will add other matches which belong to this cluster
                clusters.Add(match, new List<MDMatch>(new MDMatch[] { match}));
            }

            const double errorThreshold = 5;

            // Compute cluster membership
            foreach(PoseEstimate estimate in poseEstimates)
            {
                foreach(PoseEstimate otherEstimate in poseEstimates)
                {
                    // Ignore self
                    if(estimate == otherEstimate)
                    {
                        continue;
                    }

                    double error = estimate.RMSE(otherEstimate);
                    //Console.WriteLine("Error: " + trainingData.Category + ": " + error);
                    if(error < errorThreshold)
                    {
                        clusters[estimate.Match].Add(otherEstimate.Match);
                    }
                }
            }

            // Finally pick the largest cluster
            List<MDMatch> result = null;
            int sizeOfCluster = -1; ;
            foreach(KeyValuePair<MDMatch, List<MDMatch>> cluster in clusters)
            {
                if(cluster.Value.Count == sizeOfCluster)
                {
                    // Tie breaker: choose the cluster with smaller overall distances
                    if(result.Sum(item => item.Distance) > cluster.Value.Sum(item => item.Distance))
                    {
                        result = cluster.Value;
                    }
                }
                else if (cluster.Value.Count > sizeOfCluster)
                {
                    sizeOfCluster = cluster.Value.Count;
                    result = cluster.Value;
                }
            }

            return result;            
        }
    }

}

