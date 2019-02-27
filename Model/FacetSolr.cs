using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSolr.Model
{
   public class FacetSolr<T> where T:Bucket
    {
       public ResponseHeader responseHeader { get; set; }

       public Response response { get; set; }

       public Fecets<T> facets { get; set; }
    }

   public class BasicFacetSolr<T> where T : BasicFacet
   {
       public ResponseHeader responseHeader { get; set; }

       public Response response { get; set; }

       public T facets { get; set; }
   }

   public class Fecets<T> where T:Bucket {
     
       public int count { get; set; }
       public Facet<T> fz { get; set; } 
   }


   public class Facet<T> where T:Bucket
    {
        public List<T> buckets { get; set; } 
    }

}
