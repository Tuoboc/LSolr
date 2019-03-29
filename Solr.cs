using System;
using System.Collections.Generic;
using System.Text;

namespace LSolr
{

    /// <summary>
    /// 单例模式的实现
    /// </summary>
    public class Solr
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="SolrHttp">solr的url，例如http://aaa.bbb.com/solr/</param>
        /// <param name="SolrUserid">如果solr有basic验证，那么这个是登陆的账号</param>
        /// <param name="SolrPsw">如果solr有basic验证，那么这个是登陆的密码</param>
        /// <returns></returns>
        public static Query<T> Query<T>(string SolrHttp = "", string SolrUserid = "", string SolrPsw = "")
        {
            Query<T> query = new Query<T>(SolrHttp, SolrUserid, SolrPsw);
            return query;
        }

        public static Updt<T> Updt<T>(string SolrHttp = "", string SolrUserid = "", string SolrPsw = "")
        {
            Updt<T> updt = new Updt<T>(SolrHttp, SolrUserid, SolrPsw);
            return updt;
        }
    }
}
