
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

    public Show()
    {

    }

    public Show(string _ID, string _name, DateTime _stDate, DateTime _edDate, string _poster, string _genre)
    {
        this.ID = _ID;
        this.name = _name;
        this.stDate = _stDate;
        this.edDate = _edDate;
        this.poster = _poster;
        this.genre = _genre;
    }
}


public class kopis : MonoBehaviour
{
    public Text TargetPos;

    private static String APIGetShows = "http://www.kopis.or.kr/openApi/restful/pblprfr?service=fbd78a24798d46d38b156cf982339d7b&rows=10&cpage=1";
    private static String varName = "&shprfnmfct=";
    private static String varStDate = "&stdate=";
    private static String varEdDate = "&eddate=";
    private static String putFmtDate = "yyyyMMdd";
    private static String getFmtDate = "yyyy.MM.dd";
 
    public List<Show> showList = new List<Show>();
    public List<RawImage> posterList = new List<RawImage>();
    public List<Texture> tList = new List<Texture>();
    //Use this for initialization
    void Start () {
        //string url = "http://www.kopis.or.kr/openApi/restful/pblprfr?service=fbd78a24798d46d38b156cf982339d7b&stdate=20191008&shprfnmfct=%EC%98%88%EC%88%A0%EC%9D%98%EC%A0%84%EB%8B%B9&eddate=20191008&rows=10&cpage=1";
        //string responseText = string.Empty;

    }

    public void getCurFty()
    {
        try
        {

            if (findCurFty())
            {
                //TargetPos.text = "Found!";
                string fDist = firebase.Instance.ftyList[firebase.Instance.nearFtyIdx].dist.ToString();
                string fLat = firebase.Instance.ftyList[firebase.Instance.nearFtyIdx].latitude.ToString();
                string fLong = firebase.Instance.ftyList[firebase.Instance.nearFtyIdx].longitude.ToString();
                TargetPos.text = "Dist:" + fDist + "\n(Lat:" + fLat + " Long:" + fLong + ")";
            }
            else
            {
                TargetPos.text = "Not Found!";
            }
        }
        catch (Exception ex)
        {
            // 에러 처리/로깅 등
            Debug.Log(ex);
            throw;
        }

        
    }

    public void getCurShow()
    {
        try
        {

            getFtyShows(firebase.Instance.ftyList[firebase.Instance.nearFtyIdx].name, DateTime.Today, DateTime.Today);
            Debug.Log("Show Num : " + showList.Count);

        }
        catch (Exception ex)
        {
            // 에러 처리/로깅 등
            Debug.Log(ex);
            throw;
        }

    }

    public Boolean findCurFty()
    {
        double minDist = 100000000, dist;    //minimum distance 1km
        int fIdx = 0;
        int pFIdx = firebase.Instance.nearFtyIdx;
        foreach (Facility f in firebase.Instance.ftyList)
        {
            dist = gps.Instance.distance(f.latitude, f.longitude);
            f.dist = dist;
            Debug.Log(f.name + " : " + dist.ToString());
            if (dist < minDist)
            {
                minDist = dist;
                firebase.Instance.nearFtyIdx = fIdx;

            }
            fIdx++;
        }
        if (pFIdx != firebase.Instance.nearFtyIdx) return true;
        else return false;

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
            string ID = xn["mt20id"].InnerText;
            string name = xn["prfnm"].InnerText;
            string stDate = xn["prfpdfrom"].InnerText;
            string edDate = xn["prfpdto"].InnerText;
            string poster = xn["poster"].InnerText;
            string genre = xn["genrenm"].InnerText;

            string fcltynm = xn["fcltynm"].InnerText;
            string prfstate = xn["prfstate"].InnerText;
            string openrun = xn["openrun"].InnerText;
            Show show = new Show(ID, name, DateTime.ParseExact(stDate, getFmtDate, null), DateTime.ParseExact(edDate, getFmtDate, null), poster, genre);
            showList.Add(show);
       
 
            //string lat = xn["point"]["x"].InnerText;
            //string lng = xn["point"]["y"].InnerText;
        }
        dpShow();

    }

    public void dpShow()
    {
        int idx = 0;
        foreach (Show s in showList)
        {
            Debug.Log(idx + " " + s.name + " : " + s.stDate);
            Debug.Log("---->" + s.poster);
            //StartCoroutine(setDetails(idx, s)); //balanced parens CAS
            StartCoroutine(DownloadFile(idx, s)); //balanced parens CAS
            idx++;
        }
    }

    IEnumerator DownloadFile(int _idx, Show _s)
    {
        string fName = _idx + ".gif";
        
        var uwr = new UnityWebRequest(_s.poster, UnityWebRequest.kHttpVerbGET);
        string path = Path.Combine(Application.persistentDataPath, fName);
        uwr.downloadHandler = new DownloadHandlerFile(path);
        yield return uwr.SendWebRequest();
        if (uwr.isNetworkError || uwr.isHttpError)
            Debug.LogError(uwr.error);
        else
            Debug.Log("File successfully downloaded and saved to " + path);

        tList[_idx] = Resources.Load(Application.persistentDataPath +"/"+fName, typeof(Texture)) as Texture;  // 이미지 로드
        posterList[_idx].texture = tList[_idx];

        Debug.Log("File successfully loaded from " + Application.persistentDataPath + "/" + fName);
    }

    IEnumerator setDetails(int _idx, Show _s)
    {
        //Texture2D texture = posterList[_idx].canvasRenderer.GetMaterial().mainTexture as Texture2D;
        //Texture2D texture = posterList[_idx].canvasRenderer.GetMaterial().mainTexture as Texture2D;

        UnityWebRequest www = UnityWebRequestTexture.GetTexture(_s.poster);

        yield return www.SendWebRequest();

        AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);


        tList[_idx] = DownloadHandlerTexture.GetContent(www);
        posterList[_idx].texture = tList[_idx];
        www.Dispose();
        www = null;
    }

}
