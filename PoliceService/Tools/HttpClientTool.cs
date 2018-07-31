using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using PoliceService.Tools;

namespace PoliceService.Tools
{
    class HttpClientTool
    {

        private string keyModel = "123456";
        private static HttpClientTool instance;

        private HttpClientTool()
        {

        }

        public static HttpClientTool GetInstance()
        {
            if (instance == null)
            {
                instance = new HttpClientTool();
            }
            return instance;
        }
        /// <summary>
        /// GET请求
        /// </summary>
        /// <param name="url"></param>
        public string doGet(string url)
        {
            System.IO.StreamReader myreader = null;
            try
            {
                // string strURL = "http://localhost/WinformSubmit.php?tel=11111&name=张三";
                System.Net.HttpWebRequest request;
                // 创建一个HTTP请求
                request = (System.Net.HttpWebRequest)WebRequest.Create(url);
                //request.Method="get";
                System.Net.HttpWebResponse response;
                response = (System.Net.HttpWebResponse)request.GetResponse();
                myreader = new System.IO.StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string responseText = myreader.ReadToEnd();
                myreader.Close();
                return responseText;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Log.WriteError("Erro", e.Message);
                return null;
            }

        }

        /// <summary>
        /// Post请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="parameters"></param>
        public string doPost(string url, IDictionary<string, string> parameters)
        {
            try
            {
                Log.WriteLog("", url);
                System.Net.HttpWebRequest request;
                request = (System.Net.HttpWebRequest)WebRequest.Create(url);
                //Post请求方式
                request.Method = "POST";
                // 内容类型
                request.ContentType = "application/x-www-form-urlencoded";

                /**
                // 参数经过URL编码
                string paraUrlCoded = System.Web.HttpUtility.UrlEncode("keyword");
                paraUrlCoded += "=" + System.Web.HttpUtility.UrlEncode("多月");
                byte[] payload;
                //将URL编码后的字符串转化为字节
                payload = System.Text.Encoding.UTF8.GetBytes(paraUrlCoded);
                //设置请求的 ContentLength 
                request.ContentLength = payload.Length;
                //获得请 求流
                System.IO.Stream writer = request.GetRequestStream();
                //将请求参数写入流
                writer.Write(payload, 0, payload.Length);
                **/
                Base64 base64 = new Base64();
                //发送POST数据  
                if (!(parameters == null || parameters.Count == 0))
                {
                    StringBuilder buffer = new StringBuilder();
                    int i = 0;
                    foreach (string key in parameters.Keys)
                    {
                        //string param = base64.StringToBase64(parameters[key]);
                        string param = parameters[key];
                        if (i > 0)
                        {

                            buffer.AppendFormat("&{0}={1}", key, param);
                        }
                        else
                        {
                            buffer.AppendFormat("{0}={1}", key, param);
                            i++;
                        }
                    }
                    byte[] data = Encoding.ASCII.GetBytes(buffer.ToString());
                    using (Stream stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                        // 关闭请求流
                        stream.Close();
                    }
                }

                System.Net.HttpWebResponse response;
                // 获得响应流
                response = (System.Net.HttpWebResponse)request.GetResponse();
                System.IO.StreamReader myreader = new System.IO.StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string responseText = myreader.ReadToEnd();
                myreader.Close();
                Log.WriteLog("", responseText);
                return responseText;
            }
            catch(Exception e)
            {
                Log.WriteError("Erro", e.Message);
                return null;
            }
           
            // MessageBox.Show(responseText);
        }

   

        /// <summary>
        /// 通过http请求下载文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="url"></param>
        /// <returns></returns>

        public string httpDownloadFile(string path, string url, bool flag)
        {
            //ArrayList res = new ArrayList();

            // 设置参数
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            //发送请求并获取相应回应数据
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            if (flag == true)
            {
                if (response.ContentType.Trim() == "application/zip")
                {
                    path = path + ".zip";
                }
                if (response.ContentType.Trim() == "application/pdf")
                {
                    path = path + ".pdf";
                }
                if (response.ContentType.Trim() == "application/msword")
                {
                    path = path + ".doc";
                }
            }

            //直到request.GetResponse()程序才开始向目标网页发送Post请求
            Stream responseStream = response.GetResponseStream();
            //创建本地文件写入流
            Stream stream = new FileStream(path, FileMode.Create);
            byte[] bArr = new byte[1024];
            int size = responseStream.Read(bArr, 0, (int)bArr.Length);
            while (size > 0)
            {
                stream.Write(bArr, 0, size);
                size = responseStream.Read(bArr, 0, (int)bArr.Length);
            }
            stream.Close();
            responseStream.Close();
            return path;
        }


        //获取MD5加密的信息
        public string getMd5UrlKey(Array propertys)
        {
            Array.Sort(propertys);  //根据KEY值排序
            //组装成url
            string url = "";
            foreach (string name in propertys)
            {
                url += name + "&";
            }
            //对URL进行url编码
            string url_en = UrlEncode(url);
            Console.WriteLine(url + "的编码是:" + url_en + keyModel);
            string key = MD5Encrypt(url_en + keyModel);
            Console.WriteLine("C#端" + key);
            //Md5进行加密
            return key;
        }

        //获取MD5加密的信息
        public string getMd5Url(Array propertys)
        {
            Array.Sort(propertys);  //根据KEY值排序
            //组装成url
            string url = "";
            foreach (string name in propertys)
            {
                url += name + "&";
            }
            //对URL进行url编码
            string url_en = UrlEncode(url);
            Console.WriteLine(url + "的编码是:" + url_en + keyModel);
            url = url + "key=" + MD5Encrypt(url_en + keyModel);
            Console.WriteLine(url);
            //Md5进行加密
            return url;
        }

        /// <summary>
        /// Url编码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string UrlEncode(string str)
        {
            str = System.Web.HttpUtility.UrlEncode(str, Encoding.UTF8);
            return str.ToUpper();
        }

        ///   <summary>
        ///   给一个字符串进行MD5加密
        ///   </summary>
        ///   <param   name="strText">待加密字符串</param>
        ///   <returns>加密后的字符串</returns>
        public static string MD5Encrypt(string str)
        {
            string pwd = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(str, "MD5");
            return pwd;
        }



    }

}
