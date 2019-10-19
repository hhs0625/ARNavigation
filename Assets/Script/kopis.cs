
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using UnityEngine.UI;
using System.Net;
using System;

public class kopis : MonoBehaviour
{
    public TextAsset xmlRawFile;
    public Text uiText;
    private static String APIGetFShows = "http://www.kopis.or.kr/openApi/restful/pblprfr?service=fbd78a24798d46d38b156cf982339d7b&rows=10&cpage=1";
    private static String APIGetGShows = "http://www.kopis.or.kr/openApi/restful/pblprfr?service=fbd78a24798d46d38b156cf982339d7b&rows=10&cpage=1";

    //Use this for initialization



    void Start () {   

    }

    public void testKopis()
    {
        string url = "http://www.kopis.or.kr/openApi/restful/pblprfr?service=fbd78a24798d46d38b156cf982339d7b&stdate=20191008&shprfnmfct=%EC%98%88%EC%88%A0%EC%9D%98%EC%A0%84%EB%8B%B9&eddate=20191008&rows=10&cpage=1";
        string responseText = string.Empty;

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "GET";
        request.Timeout = 30 * 1000; // 30초
        request.Headers.Add("Authorization", "BASIC SGVsbG8="); // 헤더 추가 방법

        using (HttpWebResponse resp = (HttpWebResponse)request.GetResponse())
        {
            HttpStatusCode status = resp.StatusCode;
            Console.WriteLine(status);  // 정상이면 "OK"

            Stream respStream = resp.GetResponseStream();
            using (StreamReader sr = new StreamReader(respStream))
            {
                responseText = sr.ReadToEnd();
            }
        }
        Debug.Log("len : "+responseText.Length);

        XmlDocument xml = new XmlDocument(); // XmlDocument 생성
        xml.LoadXml(responseText);

        XmlNodeList xnList = xml.GetElementsByTagName("db"); //접근할 노드

        foreach (XmlNode xn in xnList)
        {
            string mt20id = xn["mt20id"].InnerText;
            string prfnm = xn["prfnm"].InnerText;
            string prfpdfrom = xn["prfpdfrom"].InnerText;
            string prfpdto = xn["prfpdto"].InnerText;
            string fcltynm = xn["fcltynm"].InnerText;
            string poster = xn["poster"].InnerText;
            string genrenm = xn["genrenm"].InnerText;
            string prfstate = xn["prfstate"].InnerText;
            string openrun = xn["openrun"].InnerText;
            Debug.Log(prfnm + " : " + prfpdfrom);
            //string lat = xn["point"]["x"].InnerText;
            //string lng = xn["point"]["y"].InnerText;
        }
    }


}
