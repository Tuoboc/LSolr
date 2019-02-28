using LSolr.Model;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LSolr
{
    public class Query<T>
    {
        #region 参数
        public string CoreName = "";//solr中core的名字
        private string WhereStr = "";//条件参数字符串
        public string SelectStr = "";//查询字段字符串
        public string OrderStr = "";//排序条件
        private string UserPara = "";//用户传的自定义参数
        public int DataStart = 0;//solr中的start参数
        public int DataRows = 10;//solr中的rows参数
        public List<FieldMap> fieldMaps;
        private string TimeLineMsg = "";

        private string solrhttp = "";
        private string solruserid = "";
        private string solrpsw = "";
        #endregion

        public Query()
        {
            DateTime start = DateTime.Now;
            query();
            DateTime end = DateTime.Now;
            TimeLineMsg += "Query方法执行时间+" + Math.Round((end - start).TotalSeconds, 3) + "秒.";
        }

        public Query(string SolrHttp = "", string SolrUserid = "", string SolrPsw = "")
        {
            solrhttp = SolrHttp;
            solruserid = SolrUserid;
            solrpsw = SolrPsw;
            DateTime start = DateTime.Now;
            query();
            DateTime end = DateTime.Now;
            TimeLineMsg += "Query方法执行时间" + Math.Round((end - start).TotalSeconds * 1000, 4) + "毫秒.";
        }

        public Query<T> query()
        {
            fieldMaps = GetFieldMap();
            Type type = typeof(T);
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
        /// 查询的字段
        /// </summary>
        /// <param name="exp">如果查询所有字段可省略.Select方法</param>
        /// <returns></returns>
        public Query<T> Select(Expression<Func<T, object>> exp)
        {
            DateTime start = DateTime.Now;
            Select<T> select = new Select<T>(exp, fieldMaps);
            SelectStr = select._select;
            DateTime end = DateTime.Now;
            TimeLineMsg += "Select方法执行时间" + Math.Round((end - start).TotalSeconds * 1000, 3) + "毫秒.";
            return this;
        }

        /// <summary>
        /// 查询条件
        /// </summary>
        /// <param name="exp">现支持== != > >= &lt; &lt;= Like NotLike In NotIn </param>
        /// <returns></returns>
        public Query<T> Where(Expression<Func<T, object>> exp)
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
        public Query<T> AddPara(string paras)
        {
            UserPara += paras;
            return this;
        }

        /// <summary>
        /// 排序，适用方法如下 .OrderBy(a => new { a.property1, a.property2 })
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public Query<T> OrderBy(Expression<Func<T, object>> exp)
        {
            DateTime start = DateTime.Now;
            Order<T> order = new Order<T>(exp, fieldMaps);
            OrderStr += order._order;
            DateTime end = DateTime.Now;
            TimeLineMsg += "OrderBy方法执行时间" + Math.Round((end - start).TotalSeconds * 1000, 3) + "毫秒.";
            return this;
        }

        /// <summary>
        /// /// <summary>
        /// 倒叙排序，适用方法如下 .OrderDescBy(a => new { a.property1, a.property2 })
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public Query<T> OrderDescBy(Expression<Func<T, object>> exp)
        {
            DateTime start = DateTime.Now;
            OrderDesc<T> order = new OrderDesc<T>(exp, fieldMaps);
            OrderStr += order._order;
            DateTime end = DateTime.Now;
            TimeLineMsg += "OrderDescBy方法执行时间" + Math.Round((end - start).TotalSeconds * 1000, 3) + "毫秒.";
            return this;
        }

        public string OutpuntTimeLine()
        {
            var result = tolist();
            return TimeLineMsg;
        }

        #region 分页相关参数设置
        /// <summary>
        /// Solr中查询的start参数
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public Query<T> Start(int start)
        {
            DataStart = start;
            return this;
        }
        /// <summary>
        /// Solr中查询的rows参数
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public Query<T> Rows(int row)
        {
            DataRows = row;
            return this;
        }
        /// <summary>
        /// 现实中的分页
        /// </summary>
        /// <param name="PageIndex">页码</param>
        /// <param name="PageSize">每页数量</param>
        /// <returns></returns>
        public Query<T> Page(int PageIndex, int PageSize)
        {
            int start = (PageIndex - 1) * PageSize + 1;
            int end = PageIndex * PageSize;

            int querycount = 10;
            if (start == 0)
            {
                querycount = end - start;
            }
            else if (start == 1)
            {
                start = 0;
                querycount = end - start;
            }
            else
            {
                start = start - 1;
                querycount = end - start;
            }

            DataStart = start;
            DataRows = querycount;
            return this;
        }
        #endregion

        #region 查询数据
        /// <summary>
        /// 返回结果List
        /// </summary>
        /// <returns></returns>
        public List<T> ToList()
        {
            return tolist().response.docs;
        }

        /// <summary>
        /// 返回结果List
        /// </summary>
        /// <param name="total">数据总数</param>
        /// <returns>T类型的List</returns>
        public List<T> ToList(ref int total)
        {
            var result = tolist();
            total = result.response.numFound;
            return tolist().response.docs;
        }

        /// <summary>
        /// 返回第一个结果
        /// </summary>
        /// <returns>T类型的实体类</returns>
        public T ToModel()
        {
            DataRows = 1;
            DataStart = 0;
            var list = tolist();
            if (list.response.docs.Count > 0)
                return list.response.docs[0];
            else
                return default(T);
        }

        /// <summary>
        /// 异步返回结果List
        /// </summary>
        /// <returns></returns>
        public async Task<DocSolr<T>> ToListAsync()
        {
            DocSolr<T> list = await tolistAsync();
            return list;
        }

        /// <summary>
        /// 异步返回第一个结果
        /// </summary>
        /// <returns>T类型的实体类</returns>
        public async Task<T> ToModelAsync()
        {
            DataRows = 1;
            DataStart = 0;
            var list = await tolistAsync();
            if (list.response.docs.Count > 0)
                return list.response.docs[0];
            else
                return default(T);
        }
        #endregion

        #region 私有函数
        private DocSolr<T> tolist()
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
            string queryUrl = url + CoreName + "/select?indent=on&q=*:*&wt=xml" + "&start=" + DataStart + "&rows=" + DataRows + SelectStr + WhereStr + OrderStr + UserPara;

            DateTime start = DateTime.Now;
            string html = Helper.sendPost(queryUrl, null, HeadPara, "get");
            DateTime end = DateTime.Now;
            TimeLineMsg += "网络请求执行时间" + Math.Round((end - start).TotalSeconds * 1000, 3) + "毫秒.";
            return HtmlToSolrModel(html);
        }

        private async Task<DocSolr<T>> tolistAsync()
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
            string queryUrl = url + CoreName + "/select?indent=on&q=*:*&wt=xml" + "&start=" + DataStart + "&rows=" + DataRows + SelectStr + WhereStr + OrderStr + UserPara;

            DateTime start = DateTime.Now;
            string html = await Helper.sendPostAsync(queryUrl, null, HeadPara, "get");
            DateTime end = DateTime.Now;
            TimeLineMsg += "网络请求执行时间+" + Math.Round((end - start).TotalSeconds, 3) + "秒.";
            return HtmlToSolrModel(html);
        }

        private DocSolr<T> HtmlToSolrModel(string html)
        {
            DateTime start = DateTime.Now;
            DocSolr<T> result = new DocSolr<T>();
            result.responseHeader = new ResponseHeader();
            result.response = new DocResponse<T>();
            result.response.docs = new List<T>();

            XDocument doc = XDocument.Parse(html);
            //数据总数 
            result.response.numFound = Convert.ToInt32(doc.Element("response").Element("result").Attribute("numFound").Value);
            foreach (var dataitem in doc.Element("response").Element("result").Elements("doc"))
            {
                fieldMaps.ForEach(a => a.Value = "");
                foreach (var datanode in dataitem.Elements())
                {
                    string DataType = datanode.Name.ToString();
                    string DataField = datanode.Attribute("name").Value;
                    string DataValue = datanode.Value;
                    var field = fieldMaps.Find(a => a.SolrField == DataField || a.EntityField == DataField);
                    if (field != null)
                        field.Value = DataValue;
                }
                Type type = typeof(T);
                object o = Activator.CreateInstance(type);
                var infos = type.GetProperties();
                foreach (var info in infos)
                {
                    var field = fieldMaps.Find(a => a.SolrField == info.Name || a.EntityField == info.Name);
                    if (field.Value != "")
                    {
                        switch (field.EntityType)
                        {
                            case "String":
                                info.SetValue(o, field.Value);
                                break;
                            case "Double":
                                info.SetValue(o, Convert.ToDouble(field.Value));
                                break;
                            case "Float":
                                info.SetValue(o, float.Parse(field.Value));
                                break;
                            case "Decimal":
                                info.SetValue(o, Convert.ToDecimal(field.Value));
                                break;
                            case "Int64":
                                info.SetValue(o, Convert.ToInt64(field.Value));
                                break;
                            case "Int32":
                                info.SetValue(o, Convert.ToInt32(field.Value));
                                break;
                            case "DateTime":
                                info.SetValue(o, Convert.ToDateTime(field.Value));
                                break;
                        }
                    }
                }
                result.response.docs.Add((T)o);

            }
            DateTime end = DateTime.Now;
            TimeLineMsg += "转换成对象执行时间" + Math.Round((end - start).TotalSeconds * 1000, 3) + "毫秒.";

            return result;
        }

        private List<FieldMap> GetFieldMap()
        {
            Type type = typeof(T);
            List<FieldMap> result = new List<FieldMap>();
            PropertyInfo[] propertyInfos = type.GetProperties();
            foreach (PropertyInfo item in propertyInfos)
            {
                FieldMap map = new FieldMap();
                IList<CustomAttributeData> attributes = CustomAttributeData.GetCustomAttributes(item);

                foreach (CustomAttributeData data in attributes)
                {
                    if (data.AttributeType == typeof(SolrFieldAttribute))
                    {
                        foreach (CustomAttributeTypedArgument ite in data.ConstructorArguments)
                        {
                            map.SolrField = ite.Value.ToString();
                        }
                    }

                }
                map.EntityField = item.Name;
                if (item.PropertyType.IsGenericType && item.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    // If it is NULLABLE, then get the underlying type. eg if "Nullable<int>" then this will return just "int"
                    map.EntityType = item.PropertyType.GetGenericArguments()[0].Name;
                }
                else
                {
                    map.EntityType = item.PropertyType.Name;
                }
                result.Add(map);
            }
            return result;
        }
        #endregion
    }
}
