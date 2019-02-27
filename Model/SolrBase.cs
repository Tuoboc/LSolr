using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSolr.Model
{
    public class ResponseHeader
    {
        public string status { get; set; }

        public int QTime { get; set; }


    }

    public class Response
    {

        public int numFound { get; set; }

        public int start { get; set; }

    }

    public class Params
    {
        public string q { get; set; }

        public string indent { get; set; }

        public string wt { get; set; }
    }

    public class Bucket
    {
        public string val { get; set; }

        public int count { get; set; }
    }

    public class BasicFacet
    {
        public int count { get; set; }
    }
}
