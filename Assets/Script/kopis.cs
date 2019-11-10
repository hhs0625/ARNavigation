﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using UnityEngine.UI;
using System.Net;
using System;
using System.Text;
using UnityEngine.Networking;

public class Show
{
    public string ID;
    public string name;
    public DateTime stDate;
    public DateTime edDate;
    public string poster;
    public string genre;
    public string fName;
    public string loID;
    public string loName;
    public double loLat;
    public double loLong;

    //public string fName = firebase.Instance.ftyList[firebase.Instance.nearFtyIdx].name;

    private static String APIGetShowLocation = "http://www.kopis.or.kr/openApi/restful/pblprfr/";
    private static String APIKey = "?service=fbd78a24798d46d38b156cf982339d7b";

    public Show()
    {

    }

    public Show(string _ID, string _name, DateTime _stDate, DateTime _edDate, string _poster, string _genre, string _fName)
    {
        this.ID = _ID;
        this.name = _name;
        this.stDate = _stDate;
        this.edDate = _edDate;
        this.poster = _poster;
        this.genre = _genre;
        this.fName = _fName;
        this.loName = getShowLocation(_ID).Replace(fName,"").Replace("(","").Replace(")", "").Replace(" ", "");
    }


    public string getShowLocation(string _sID)
    {

        string url = APIGetShowLocation + _sID + APIKey;
        string responseText = string.Empty;
        // utf-8 인코딩


        Debug.Log("URL:" + url);
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
        Debug.Log("len : " + responseText.Length);

        XmlDocument xml = new XmlDocument(); // XmlDocument 생성
        xml.LoadXml(responseText);

        XmlNodeList xnList = xml.GetElementsByTagName("db"); //접근할 노드

        foreach (XmlNode xn in xnList)
        {
            try
            {

                string loName = xn["fcltynm"].InnerText;
                return loName;
            }
            catch (Exception ex)
            {
                // 에러 처리/로깅 등
                Debug.Log(ex);
                throw;
            }
        }
        return "Nothing";
    }
}


public class kopis : MonoBehaviour
{
    public Text TargetPos;
    public Boolean isOldbie;
    public int dbSt = 0;

    private static String APIGetShows = "http://www.kopis.or.kr/openApi/restful/pblprfr?service=fbd78a24798d46d38b156cf982339d7b&rows=10&cpage=1";
    private static String varName = "&shprfnmfct=";
    private static String varStDate = "&stdate=";
    private static String varEdDate = "&eddate=";
    private static String putFmtDate = "yyyyMMdd";
    private static String getFmtDate = "yyyy.MM.dd";

    private static String oDpPosterStr = "imgPoster";
    private static String oDpNameStr = "txtName";
    private static String oDpDateStr = "txtDate";
    private static String oDpLocationStr = "txtLocation";

    public List<Show> showList = new List<Show>();
    public List<RawImage> posterList = new List<RawImage>();
    public List<GameObject> dpList = new List<GameObject>();
    //public List<Texture> tList = new List<Texture>();
    //Use this for initialization
    void Start () {
        //string url = "http://www.kopis.or.kr/openApi/restful/pblprfr?service=fbd78a24798d46d38b156cf982339d7b&stdate=20191008&shprfnmfct=%EC%98%88%EC%88%A0%EC%9D%98%EC%A0%84%EB%8B%B9&eddate=20191008&rows=10&cpage=1";
        //string responseText = string.Empty;

        // 파일 존재유무 검사 현재 info파일 지워놓은 상태


        /*
        string path = Application.persistentDataPath + "/" + "0.jpg";
        isOldbie = File.Exists(path);
        byte[] filedata;
        filedata = File.ReadAllBytes(path);
        tList[0] = null;
        tList[0] = new Texture2D(2, 2);
        tList[0].LoadImage(filedata);
        //tList[0] = (Texture2D)Resources.Load(path, typeof(Texture2D)) as Texture2D;  // 이미지 로드
        Debug.Log("File successfully loaded from " + path);
        */
    }





    public void getFtyShows(string _fName, DateTime _stDate, DateTime _edDate)
    {

        string url = APIGetShows + varName + WWW.EscapeURL(_fName)+ varStDate + _stDate.ToString(putFmtDate) + varEdDate + _edDate.ToString(putFmtDate);
        string responseText = string.Empty;
        // utf-8 인코딩


        Debug.Log("URL:" + url);
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
        Debug.Log("len : " + responseText.Length);

        XmlDocument xml = new XmlDocument(); // XmlDocument 생성
        xml.LoadXml(responseText);

        XmlNodeList xnList = xml.GetElementsByTagName("db"); //접근할 노드

        foreach (XmlNode xn in xnList)
        {
            try
            {

                string ID = xn["mt20id"].InnerText;
                string name = xn["prfnm"].InnerText;
                string stDate = xn["prfpdfrom"].InnerText;
                string edDate = xn["prfpdto"].InnerText;
                string poster = xn["poster"].InnerText;
                string genre = xn["genrenm"].InnerText;

                string fcltynm = xn["fcltynm"].InnerText;
                string prfstate = xn["prfstate"].InnerText;
                string openrun = xn["openrun"].InnerText;
                Debug.Log(ID + "\n" + name + "\n" + stDate + "\n" + edDate + "\n" + poster + "\n" + genre + "\n" + fcltynm + "\n" + prfstate + "\n" + openrun);
                Show show = new Show(ID, name, DateTime.ParseExact(stDate, getFmtDate, null), DateTime.ParseExact(edDate, getFmtDate, null), poster, genre, _fName);
                showList.Add(show);

            }
            catch (Exception ex)
            {
                // 에러 처리/로깅 등
                Debug.Log(ex);
                dbSt = -1;
                throw;
            }
        }

        dbSt = 1;
        //dpShow();

    }

    public void dpShow()
    {
        int idx = 0;
        foreach (Show s in showList)
        {
            Debug.Log(idx + " " + s.name + " : " + s.stDate);
            Debug.Log("---->" + s.poster);
            StartCoroutine(setDetails(idx, s)); //balanced parens CAS

            idx++;
        }
    }


    IEnumerator setDetails(int _idx, Show _s)
    {
        GameObject oDp = null;
        RawImage oDpPoster = null;
        Text oDpName = null;
        Text oDpDate = null;
        Text oDpLocation = null;


        UnityWebRequest www = UnityWebRequestTexture.GetTexture("http://www.kopis.or.kr/upload/pfmPoster/PF_PF155591_191004_113953.jpg");
        //UnityWebRequest www = UnityWebRequestTexture.GetTexture(_s.poster);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }

        oDp = GameObject.Find("dp" + _idx.ToString());
        if(oDp != null)
        {
            oDpPoster = oDp.transform.Find(oDpPosterStr).GetComponent<RawImage>();
            if (oDpPoster != null) oDpPoster.texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            else Debug.Log(oDpPosterStr + "를 찾을 수 없음");

            oDpName = oDp.transform.Find(oDpNameStr).GetComponent<Text>();
            if (oDpName != null) oDpName.text = _s.name;
            else Debug.Log(oDpNameStr + "를 찾을 수 없음");

            oDpDate = oDp.transform.Find(oDpDateStr).GetComponent<Text>();
            if (oDpDate != null) oDpDate.text = _s.stDate.ToString() + " - " + _s.edDate.ToString();
            else Debug.Log(oDpDateStr + "를 찾을 수 없음");

            oDpLocation = oDp.transform.Find(oDpLocationStr).GetComponent<Text>();
            if (oDpLocation != null) oDpLocation.text = _s.loName;
            else Debug.Log(oDpNameStr + "를 찾을 수 없음");

        }
        else
        {
            Debug.Log("dp" + _idx.ToString() + "를 찾을 수 없음");
        }
    



        /*
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(_s.poster);

        yield return www.SendWebRequest();

        AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);

        tList[_idx] = DownloadHandlerTexture.GetContent(www);
        posterList[_idx].texture = tList[_idx];
        www.Dispose();
        www = null;
        */
    }

}
