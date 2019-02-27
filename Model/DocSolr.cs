using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSolr.Model
{
    public class DocSolr<T>
    {
        public ResponseHeader responseHeader { get; set; }

        public DocResponse<T> response { get; set; }
    }


    public class DocResponse<T> : Response
    {
        public List<T> docs { get; set; }
    }
}
