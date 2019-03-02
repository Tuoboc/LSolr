﻿using LSolr.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
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

        /// <summary>
        /// Http (GET/POST)
        /// </summary>
        /// <param name="url">请求URL</param>
        /// <param name="parameters">请求参数</param>
        /// <param name="RequestHead">请求头参数</param>
        /// <param name="method">请求方法</param>
        /// <returns>响应内容</returns>
        public static string sendPost(string url, IDictionary<string, string> parameters, IDictionary<string, string> RequestHead, string method, int timeout = 5000, string WhereString = "")
        {
            if (method.ToLower() == "post")
            {
                HttpWebRequest req = null;
                HttpWebResponse rsp = null;
                System.IO.Stream reqStream = null;
                try
                {
                    req = (HttpWebRequest)WebRequest.Create(url);
                    req.Method = method;
                    req.KeepAlive = false;
                    req.ProtocolVersion = HttpVersion.Version10;
                    req.Timeout = timeout;
                    req.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
                    string para = WhereString != "" ? WhereString + BuildQuery(parameters, "utf8") : BuildQuery(parameters, "utf8");
                    byte[] postData = Encoding.UTF8.GetBytes(para);
                    foreach (var item in RequestHead)
                    {
                        req.Headers.Add(item.Key, item.Value);
                    }
                    reqStream = req.GetRequestStream();
                    reqStream.Write(postData, 0, postData.Length);
                    rsp = (HttpWebResponse)req.GetResponse();
                    Encoding encoding = Encoding.GetEncoding(rsp.CharacterSet);
                    return GetResponseAsString(rsp, encoding);
                }
                catch (Exception ex)
                {
                    throw ex;
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


        /// <summary>
        /// 把响应流转换为文本。
        /// </summary>
        /// <param name="rsp">响应流对象</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>响应文本</returns>
        private static string GetResponseAsString(HttpWebResponse rsp, Encoding encoding)
        {
            System.IO.Stream stream = null;
            StreamReader reader = null;
            try
            {
                // 以字符流的方式读取HTTP响应
                stream = rsp.GetResponseStream();
                reader = new StreamReader(stream, encoding);
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
                        }
                    }
                    return model;
                }
                if (filename[filename.Length - 1].ToLower() == "appsettings.json")
                {
                    using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(File.ReadAllText(item))))
                    {
                        DataContractJsonSerializer deseralizer = new DataContractJsonSerializer(typeof(Setting));
                        model = (Setting)deseralizer.ReadObject(ms);// //反序列化ReadObject
                    }
                    return model;
                }
            }
            return model;
        }

    }
}
