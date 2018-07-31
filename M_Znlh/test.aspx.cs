using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace M_Znlh
{
    public partial class test : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {


            string paramData = "{" +
                                            // "\"recipients\":[\"" + "18036602309" + "\",\"" + "18961388707" + "\"]," +
                                            "\"recipients\":[\"" + "18036600293" + "\"]," +
                                             "\"prefix\":\"" + "连云港港" + "\"," +
                                            "\"template\":\"" + "SMS_63090068" + "\"," +
                                            "\"parameters\":{\"TITLE\":\"短信测试\",\"STEPNAME\":\"HAH\"}}";

            paramData = "{\"template\":\"SMS_63320388\",\"recipients\":[\"18036600293\"],\"prefix\":\"连云港港\",\"parameters\":{\"AUTHCODE\":\"连云港港\",\"APPNAME\":\"18036600293\"}}";

            PostRequest(paramData);

        }

        public void PostRequest(string paramData)
        {

            string strURL = "http://168.100.1.123/cloudsms/sender";
            System.Net.HttpWebRequest request;

            System.Net.ServicePointManager.DefaultConnectionLimit = 200;

            request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(strURL);
            request.ContentType = "application/json; charset=UTF-8";
            request.Method = "POST";
            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.GetEncoding("utf-8").GetBytes("portapp:w321lXfEtVv0FTN4")));
            
            byte[] LoadData;
            //将URL编码后的字符串转化为字节
            LoadData = System.Text.Encoding.UTF8.GetBytes(paramData);
            //设置请求的 ContentLength 
            request.ContentLength = LoadData.Length;
            //获得请求流


            Stream newStream = request.GetRequestStream();
            newStream.Write(LoadData, 0, LoadData.Length);
            newStream.Close();


            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                if (response.StatusDescription != "OK")
                {
                    newStream = request.GetRequestStream();
                    newStream.Write(LoadData, 0, LoadData.Length);
                    newStream.Close();
                }
                response.Close();
                // System.GC.Collect();
            }
            catch (WebException ex)
            {
                response = (HttpWebResponse)ex.Response;
                response.Close();
            }


        }
    }
}