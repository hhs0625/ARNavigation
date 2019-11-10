using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class appDB : MonoBehaviour
{
    private static appDB _instance = null;
    firebase fbDB = new firebase();
    kopis kopisDB = new kopis();

    int stepSyncDB = 0;
    public double minDist = 100000000;

    public static appDB Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType(typeof(appDB)) as appDB;

                if (_instance == null)
                {
                    Debug.LogError("There's no active ManagerClass object");
                }
            }

            return _instance;
        }
    }

    // Start is called before the first frame update
    void Start()
    {


    }

    // Update is called once per frame
    void Update()
    {
        switch (stepSyncDB)
        {
            case 0:
                Debug.Log("access firebase");
                fbDB.getFirebaseDB();
                stepSyncDB++;
                break;
            case 1:
                if (fbDB.dbSt > 0)
                {
                    Debug.Log("firebase success");
                    stepSyncDB++;
                }
                else if (fbDB.dbSt < 0)
                {
                    Debug.Log("firebase fail");
                    stepSyncDB--;
                }
                break;
            case 2:
                Debug.Log("Find Current Facility");
                if(findCurFty())
                {
                    Debug.Log("find success");
                    stepSyncDB++;
                }
                else
                {
                    Debug.Log("find fail");
                    stepSyncDB = 10;
                }
                break;
            case 3:
                Debug.Log("access kopis : " + fbDB.ftyList[fbDB.nearFtyIdx].name + " - " + DateTime.Today);
                //kopisDB.getFtyShows(fbDB.ftyList[fbDB.nearFtyIdx].name, DateTime.Today, DateTime.Today);
                kopisDB.getFtyShows(fbDB.ftyList[fbDB.nearFtyIdx].name, DateTime.Today, DateTime.Now.AddDays(30));
                stepSyncDB++;
                break;
            case 4:
                if (kopisDB.dbSt > 0)
                {
                    Debug.Log("kopisDB success");
                    stepSyncDB++;
                }
                else if (fbDB.dbSt < 0)
                {
                    Debug.Log("kopisDB fail");
                    stepSyncDB--;
                }
                break;
            case 5:
                if(findShowLocation()) Debug.Log("findShowLocation success");
                else Debug.Log("findShowLocation fail");
                stepSyncDB++;
                break;
            case 6:
                downloadPoster();
                stepSyncDB++;
                break;
            default:
                checkShow();
                Debug.Log("done");
                break;
        }
    }


    private void checkShow()
    {
        Debug.Log("Check Show : " + kopisDB.showList.Count);
        foreach (Show s in kopisDB.showList)
        {
            Debug.Log(s.ID + "\n" + s.name + "\n" + s.loID + "\n" + s.loName + "\n" + s.loLat + "\n" + s.loLong);
        }
    }

    private bool findShowLocation()
    {
        Debug.Log("findShowLocation : " + kopisDB.showList.Count);
        try
        {
            foreach (Show s in kopisDB.showList)
            {
                foreach (Facility f in fbDB.ftyList)
                {
                    if (s.fName == f.name)
                    {
                        foreach (Theater t in f.theaterList)
                        {
                            if (s.loName.Contains(t.keyName))
                            {
                                s.loID = t.ID;
                                s.loLat = t.latitude;
                                s.loLong = t.longitude;
                            }
                        }
                    }
                }
                if (s.loID == null)
                {
                    s.loID = "UNKNOW";
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
            return false;
        }

        return true;
    }

    private void downloadPoster()
    {
        foreach (Show s in kopisDB.showList)
        {
            string ext = s.poster;
            StartCoroutine(DownloadFile(s.ID + ext.Substring(s.poster.Length - 4, 4), s.poster));
        }

    }

    private bool findCurFty()
    {
        
        double dist;    
        int fIdx = 0;
        int pFIdx = fbDB.nearFtyIdx;

        Debug.Log("findCurFty : " + fbDB.ftyList.Count);
        foreach (Facility f in fbDB.ftyList)
        {
            Debug.Log(f.name + " : \n" + f.latitude + "  " + f.longitude);
            dist = gps.Instance.distance(f.latitude, f.longitude);
            f.dist = dist;

            if (dist < minDist)
            {
                minDist = dist;
                fbDB.nearFtyIdx = fIdx;

            }
            fIdx++;
        }
        if (pFIdx != fbDB.nearFtyIdx) return true;
        else return false;
    }

    IEnumerator DownloadFile(string _fileName, string _poster)
    {

        var uwr = new UnityWebRequest(_poster, UnityWebRequest.kHttpVerbGET);
        string path = Path.Combine(Application.persistentDataPath, _fileName);
        uwr.downloadHandler = new DownloadHandlerFile(path);
        yield return uwr.SendWebRequest();
        if (uwr.isNetworkError || uwr.isHttpError)
            Debug.LogError(uwr.error);
        else
            Debug.Log("File successfully downloaded and saved to " + path);

    }
}
