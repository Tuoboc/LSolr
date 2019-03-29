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
        private string UserPara = "";//用户传的自定义参数
        private string UpdateData = "";//要更新的数据
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

        private Updt<T> update()
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

        public Updt<T> Update(Expression<Func<T, object>> exp)
        {
            DateTime start = DateTime.Now;
            Update<T> update = new Update<T>(exp, fieldMaps);
            UpdateData = update.UpdateData;
            DateTime end = DateTime.Now;
            TimeLineMsg += "Update方法执行时间" + Math.Round((end - start).TotalSeconds * 1000, 3) + "毫秒.";
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
            UpdateModel();
            return TimeLineMsg;
        }

        public bool UpdateModel()
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
            string queryUrl = url + CoreName + "/update?boost=1.0&commitWithin=1000&overwrite=true&wt=xml" + UserPara;
            string html = Helper.sendPost(queryUrl, null, HeadPara, "post", 5000, UpdateData, "application/json");
            XDocument doc = XDocument.Parse(html);
            //数据总数 
            var eleList = doc.Element("response").Element("lst").Elements("int");
            foreach (var item in eleList)
            {
                if (item.Attribute("name").Value == "status" && item.Value == "0")
                    return true;
            }
            throw new Exception(html);
        }

        /// <summary>
        /// 增量导入所有数据
        /// </summary>
        /// <returns></returns>
        public bool UpdateAllNewData()
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
            string queryUrl = url + CoreName + "/dataimport?indent=on&wt=xml" + UserPara;
            string html = Helper.sendPost(queryUrl, null, HeadPara, "post", 5000, UpdateData, "application/json");
            XDocument doc = XDocument.Parse(html);
            //数据总数 
            var eleList = doc.Element("response").Element("lst").Elements("int");
            foreach (var item in eleList)
            {
                if (item.Attribute("name").Value == "status" && item.Value == "0")
                    return true;
            }
            throw new Exception(html);
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
