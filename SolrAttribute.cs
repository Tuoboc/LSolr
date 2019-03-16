using System;
using System.Collections.Generic;
using System.Text;

namespace LSolr
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SolrFieldAttribute : Attribute
    {
        public string SolrField { get; set; }
        public bool IsKey { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="field"></param>
        /// <param name="IsKey">是否为主键</param>
        public SolrFieldAttribute(string field, bool IsKey = false)
        {
            this.SolrField = SolrField;
            this.IsKey = IsKey;
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
