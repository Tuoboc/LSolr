using System;
using System.Collections.Generic;
using System.Text;

namespace LSolr.Model
{
    public class FieldMap
    {
        public string SolrField { get; set; }
        public string EntityField { get; set; }
        public string EntityType { get; set; }

        public string Value { get; set; }
    }
}
