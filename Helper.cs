﻿using LSolr.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace LSolr
{
    public static class Helper
    {
        public static Setting setting = ReadConfig();
        private static string EnvironmentName { get; set; }

        private static HttpClient _httpClient;

        public static void SetEnvironmentName(string Name)
        {
            EnvironmentName = Name;
            setting = ReadConfig();
        }
        /// <summary>
        /// Http (GET/POST)
        /// </summary>
        /// <param name="url">请求URL</param>
        /// <param name="parameters">请求参数</param>
        /// <param name="RequestHead">请求头参数</param>
        /// <param name="method">请求方法</param>
        /// <returns>响应内容</returns>
        public static string sendPost(string url, IDictionary<string, string> parameters, IDictionary<string, string> RequestHead, string method, int timeout = 50000, string PostData = "", string contentType = "application/x-www-form-urlencoded")
        {

            string result = "";
            try
            {
                if (_httpClient == null)
                {
                    _httpClient = new HttpClient();
                    _httpClient.Timeout = new TimeSpan(0, 0, 0, 0, timeout);
                }
                if (method.ToLower() == "post")
                {

                    HttpContent httpContent = new StringContent(PostData);
                    httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                    var request = new HttpRequestMessage()
                    {
                        RequestUri = new Uri(url),
                        Method = HttpMethod.Post,
                        Content = httpContent
                    };
                    if (RequestHead != null)
                    {
                        foreach (var item in RequestHead)
                        {
                            request.Headers.Add(item.Key, item.Value);
                        }
                    }
                    var responseMessage = _httpClient.SendAsync(request).Result;

                    if (responseMessage.IsSuccessStatusCode)
                    {
                        Task<string> t = responseMessage.Content.ReadAsStringAsync();
                        string reString = t.Result;
                        if (!string.IsNullOrWhiteSpace(reString))
                        {
                            result = reString;
                        }
                    }
                }
                else
                {
                    var responseMessage = _httpClient.GetStringAsync(new Uri(url));
                    string reString = responseMessage.Result;
                    result = reString;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public static async Task<string> sendPostAsync(string url, IDictionary<string, string> parameters, IDictionary<string, string> RequestHead, string method, int timeout = 5000)
        {
            if (method.ToLower() == "post")
            {
                HttpWebRequest req = null;
                WebResponse rsp = null;
                Stream reqStream = null;
                try
                {
                    req = (HttpWebRequest)WebRequest.Create(url);
                    req.Method = method;
                    req.KeepAlive = false;
                    req.ProtocolVersion = HttpVersion.Version10;
                    req.Timeout = timeout;
                    req.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
                    byte[] postData = Encoding.UTF8.GetBytes(BuildQuery(parameters, "utf8"));
                    foreach (var item in RequestHead)
                    {
                        req.Headers.Add(item.Key, item.Value);
                    }
                    reqStream = req.GetRequestStream();
                    reqStream.Write(postData, 0, postData.Length);
                    rsp = await req.GetResponseAsync();
                    return GetResponseAsString(rsp);
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
                finally
                {
                    if (reqStream != null) reqStream.Close();
                    if (rsp != null) rsp.Close();
                }
            }
            else
            {
                //创建请求
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                foreach (var item in RequestHead)
                {
                    request.Headers.Add(item.Key, item.Value);
                }
                //GET请求
                request.Method = "GET";
                request.ReadWriteTimeout = timeout;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));

                //返回内容
                string retString = myStreamReader.ReadToEnd();
                return retString;
            }
        }


        private static string GetResponseAsString(WebResponse rsp)
        {
            System.IO.Stream stream = null;
            StreamReader reader = null;
            try
            {
                // 以字符流的方式读取HTTP响应
                stream = rsp.GetResponseStream();
                reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
            finally
            {
                // 释放资源
                if (reader != null) reader.Close();
                if (stream != null) stream.Close();
                if (rsp != null) rsp.Close();
            }
        }


        /// <summary>
        /// 组装普通文本请求参数。
        /// </summary>
        /// <param name="parameters">Key-Value形式请求参数字典</param>
        /// <returns>URL编码后的请求数据</returns>
        private static string BuildQuery(IDictionary<string, string> parameters, string encode)
        {
            StringBuilder postData = new StringBuilder();
            bool hasParam = false;
            if (parameters != null)
            {
                IEnumerator<KeyValuePair<string, string>> dem = parameters.GetEnumerator();
                while (dem.MoveNext())
                {
                    string name = dem.Current.Key;
                    string value = dem.Current.Value;
                    // 忽略参数名或参数值为空的参数
                    if (!string.IsNullOrEmpty(name))//&& !string.IsNullOrEmpty(value)
                    {
                        if (hasParam)
                        {
                            postData.Append("&");
                        }
                        postData.Append(name);
                        postData.Append("=");
                        if (encode == "gb2312")
                        {
                            postData.Append(HttpUtility.UrlEncode(value, Encoding.GetEncoding("gb2312")));
                        }
                        else if (encode == "utf8")
                        {
                            postData.Append(HttpUtility.UrlEncode(value, Encoding.UTF8));
                        }
                        else
                        {
                            postData.Append(value);
                        }
                        hasParam = true;
                    }
                }
            }
            return postData.ToString();
        }

        public static Setting ReadConfig()
        {
            Setting model = new Setting();
            foreach (var item in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory))
            {
                var filename = item.Split('/', '\\');
                if (filename[filename.Length - 1].ToLower() == "appsettings.json" && EnvironmentName != "Development")
                {
                    using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(File.ReadAllText(item))))
                    {
                        DataContractJsonSerializer deseralizer = new DataContractJsonSerializer(typeof(Setting));
                        model = (Setting)deseralizer.ReadObject(ms);// //反序列化ReadObject
                    }
                    return model;
                }
                if (filename[filename.Length - 1].ToLower() == "appsettings.development.json" && EnvironmentName == "Development")
                {
                    using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(File.ReadAllText(item))))
                    {
                        byte[] b = ms.ToArray();
                        string s = System.Text.Encoding.UTF8.GetString(b, 0, b.Length);

                        DataContractJsonSerializer deseralizer = new DataContractJsonSerializer(typeof(Setting));
                        model = (Setting)deseralizer.ReadObject(ms);// //反序列化ReadObject
                    }
                    return model;
                }
                if (filename[filename.Length - 1].ToLower() == "web.config")
                {
                    XDocument doc = XDocument.Load(item);
                    foreach (var config in doc.Element("configuration").Element("appSettings").Elements("add"))
                    {
                        switch (config.Attribute("key").Value.ToLower())
                        {
                            case "solrhttp":
                                model.solrhttp = config.Attribute("value").Value;
                                break;
                            case "solruserid":
                                model.solruserid = config.Attribute("value").Value;
                                break;
                            case "solrpsw":
                                model.solrpsw = config.Attribute("value").Value;
                                break;
                            case "solrtimezone":
                                model.solrtimezone = config.Attribute("value").Value;
                                break;
                            case "solroutlog":
                                model.solroutlog = config.Attribute("value").Value;
                                break;
                        }

                    }
                    return model;
                }
                if (filename[filename.Length - 1].ToLower() == "app.config")
                {
                    XDocument doc = XDocument.Load(item);
                    foreach (var config in doc.Element("configuration").Element("appSettings").Elements("add"))
                    {
                        switch (config.Attribute("key").Value.ToLower())
                        {
                            case "solrhttp":
                                model.solrhttp = config.Attribute("value").Value;
                                break;
                            case "solruserid":
                                model.solruserid = config.Attribute("value").Value;
                                break;
                            case "solrpsw":
                                model.solrpsw = config.Attribute("value").Value;
                                break;
                            case "solrtimezone":
                                model.solrtimezone = config.Attribute("value").Value;
                                break;
                            case "solroutlog":
                                model.solroutlog = config.Attribute("value").Value;
                                break;
                        }
                    }
                    return model;
                }
               
            }
            return model;
        }

        public static void WriteLogs(string content)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            if (!string.IsNullOrEmpty(path))
            {
                path = AppDomain.CurrentDomain.BaseDirectory + "/SolrLog";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                path = path + "\\" + DateTime.Now.ToString("yyyyMMdd");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                path = path + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                if (!File.Exists(path))
                {
                    FileStream fs = File.Create(path);
                    fs.Close();
                }
                if (File.Exists(path))
                {
                    StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.Default);
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "-->" + content);
                    //  sw.WriteLine("----------------------------------------");
                    sw.Close();
                }
            }
        }

        public static List<FieldMap> GetFieldMap(Type type)
        {
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
                        map.SolrField = data.ConstructorArguments[0].Value.ToString();
                        map.IsKey = Convert.ToBoolean(data.ConstructorArguments[1].Value);
                    }

                }
                map.EntityField = item.Name;
                if (item.PropertyType.IsGenericType && item.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    // If it is NULLABLE, then get the underlying type. eg if "Nullable<int>" then this will return just "int"
                    map.EntityType = item.PropertyType.GetGenericArguments()[0].Name;
                    map.IsList = false;
                }
                else if (item.PropertyType.IsGenericType && item.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    map.EntityType = item.PropertyType.GetGenericArguments()[0].Name;
                    map.IsList = true;
                }
                else
                {
                    map.EntityType = item.PropertyType.Name;
                    map.IsList = false;
                }
                result.Add(map);
            }
            return result;
        }

        public static object GetPropertyValue(object info, string field)
        {
            if (info == null) return null;
            Type t = info.GetType();
            IEnumerable<PropertyInfo> property = from pi in t.GetProperties() where pi.Name.ToUpper() == field.ToUpper() select pi;
            return property.First().GetValue(info, null);
        }
    }
}
