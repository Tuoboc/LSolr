using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSolr.Model
{
    public class FacetSolr<T>
    {
        public List<FacetData<T>> data { get; set; }
        public int numFound { get; set; }
    }

    public class FacetData<T>
    {
        public T entity { get; set; }
        public int num { get; set; }
    }
}
