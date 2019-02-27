using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace LSolr
{
    [DataContract]

    public class Setting
    {
        [DataMember]

        public string solrhttp { get; set; }
        [DataMember]

        public string solruserid { get; set; }
        [DataMember]

        public string solrpsw { get; set; }
        [DataMember]
        public string timezone { get; set; }
    }
}
