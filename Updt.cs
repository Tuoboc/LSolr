using LSolr.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LSolr
{
    public class Updt<T>
    {
        #region 参数
        public string CoreName = "";//solr中core的名字
        private string WhereStr = "";//条件参数字符串
        public string SelectStr = "";//查询字段字符串
        public string OrderStr = "";//排序条件
        private string UserPara = "";//用户传的自定义参数
        private string GroupStr = "";
        public int DataStart = 0;//solr中的start参数
        public int DataRows = 10;//solr中的rows参数
        public List<FieldMap> fieldMaps;
        private string TimeLineMsg = "";

        private string solrhttp = "";
        private string solruserid = "";
        private string solrpsw = "";
        #endregion

        public Updt()
        {
            DateTime start = DateTime.Now;
            update();
            DateTime end = DateTime.Now;
            TimeLineMsg += "Updt+" + Math.Round((end - start).TotalSeconds, 3) + "秒.";
        }

        public Updt(string SolrHttp = "", string SolrUserid = "", string SolrPsw = "")
        {
            solrhttp = SolrHttp;
            solruserid = SolrUserid;
            solrpsw = SolrPsw;
            DateTime start = DateTime.Now;
            update();
            DateTime end = DateTime.Now;
            TimeLineMsg += "Query方法执行时间" + Math.Round((end - start).TotalSeconds * 1000, 4) + "毫秒.";
        }

        public Updt<T> update()
        {
            Type type = typeof(T);
            fieldMaps = Helper.GetFieldMap(type);
            try
            {
                Attribute[] attrs = System.Attribute.GetCustomAttributes(type);
                if (attrs.Length > 0)
                {
                    foreach (Attribute item in attrs)
                    {
                        if (item is SolrCoreAttribute)
                        {
                            CoreName = ((SolrCoreAttribute)item).SolrCore;
                        }
                    }
                }
                if (string.IsNullOrEmpty(CoreName))
                    CoreName = type.Name.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return this;
        }



        /// <summary>
        /// 查询条件
        /// </summary>
        /// <param name="exp">现支持== != > >= &lt; &lt;= Like NotLike In NotIn </param>
        /// <returns></returns>
        public Updt<T> Where(Expression<Func<T, object>> exp)
        {
            DateTime start = DateTime.Now;
            Where<T> where = new Where<T>(exp, fieldMaps);
            WhereStr += where._where;
            DateTime end = DateTime.Now;
            TimeLineMsg += "Where方法执行时间" + Math.Round((end - start).TotalSeconds * 1000, 3) + "毫秒.";
            return this;
        }

        /// <summary>
        /// 添加自定义参数
        /// </summary>
        /// <param name="paras">参数字符串</param>
        /// <returns></returns>
        public Updt<T> AddPara(string paras)
        {
            UserPara += paras;
            return this;
        }

        public string OutpuntTimeLine()
        {
            return TimeLineMsg;
        }

        #region 私有函数


        /// <summary>
        /// 进行网络请求
        /// </summary>
        /// <returns></returns>
        private string CreateSolrHttp(string paras)
        {
            string url = string.IsNullOrEmpty(solrhttp) ? Helper.setting.solrhttp : solrhttp;
            if (string.IsNullOrEmpty(url))
                throw new Exception("配置文件中没有找到solrhttp配置");
            string userid = string.IsNullOrEmpty(solruserid) ? Helper.setting.solruserid : solruserid;
            string psw = string.IsNullOrEmpty(solrpsw) ? Helper.setting.solrpsw : solrpsw;
            Dictionary<string, string> HeadPara = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(userid) && !string.IsNullOrEmpty(psw))
            {
                byte[] bytes = Encoding.Default.GetBytes(userid + ":" + psw);
                string value = Convert.ToBase64String(bytes);
                HeadPara.Add("Authorization", "Basic " + value);
            }
            string queryUrl = url + CoreName + paras;


            DateTime start = DateTime.Now;
            string html = Helper.sendPost(queryUrl, null, HeadPara, "post", 5000, WhereStr);
            DateTime end = DateTime.Now;
            TimeLineMsg += "网络请求执行时间" + Math.Round((end - start).TotalSeconds * 1000, 3) + "毫秒.";
            return html;
        }

        #endregion
    }
}
