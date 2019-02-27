using System;
using System.Collections.Generic;
using System.Text;

namespace LSolr
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SolrFieldAttribute : Attribute
    {
        public string SolrField { get; set; }

        public SolrFieldAttribute(string field)
        {
            this.SolrField = SolrField;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class SolrCoreAttribute : Attribute
    {
        public string SolrCore { get; set; }
        public SolrCoreAttribute(string core)
        {
            this.SolrCore = core;
        }
    }
}
