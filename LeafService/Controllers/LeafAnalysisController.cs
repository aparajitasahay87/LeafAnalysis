using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using LeafService.Models;
using LeafCore;
namespace LeafService.Controllers
{
    public class LeafAnalysisController : ApiController
    {
        static LeafAnalysis leafAnalysis = new LeafAnalysis();

        // GET api/values
        public IEnumerable<ResultImages> Get()
        {
            return new ResultImages[] { new ResultImages() };
        }

        // POST api/LeafAnalysis
        public ResultImages Post([FromBody]QueryImageInfo queryImage)
        {
            ResultImages result = WebApiApplication.leafAnalysis.query(queryImage);
            return result;
        }
    }
}
