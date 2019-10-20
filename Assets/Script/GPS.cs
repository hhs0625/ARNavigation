using UnityEngine;

using System.Collections;

using UnityEngine.UI;

using System;

public class gps : MonoBehaviour
{
    private static gps _instance = null;

    public static gps Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType(typeof(gps)) as gps;

                if (_instance == null)
                {
                    Debug.LogError("There's no active ManagerClass object");
                }
            }

            return _instance;
        }
    }

    //public ViewHandler v;
    public Text debugText;

    float fiveSecondCounter = 0.0f;


    /**
    * Latitude - 경도, Longtitude - 위도
    */
    public double TargetLatitude, TargetLongtitude; // 37.507839, 127.039864



    IEnumerator Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                //   app = Firebase.FirebaseApp.DefaultInstance;

                // Set a flag here to indicate whether Firebase is ready to use by your app.
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });

        debugText.text += "Starting the GPS Script\n";

#if UNITY_ANROID
         Input.compass.enabled = true;
#endif

        return InitializeGPSServices();
    }

    IEnumerator InitializeGPSServices()
    {
        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
        {
            debugText.text += "GPS disabled by user\n";
            yield break;
        }

        // Start service before querying location
        Input.location.Start(0.1f, 0.1f);

        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1)
        {
            debugText.text += "Timed out\n";
            yield break;
        }
    }

    void Update()
    {
        fiveSecondCounter += Time.deltaTime;
        if (fiveSecondCounter > 1.0)
        {
            UpdateGPS();
            fiveSecondCounter = 0.0f;
        }
    }

    int updateCnt = 0;

    void UpdateGPS()
    {
        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            debugText.text += "Unable to determine device location\n";
            Input.location.Stop();
            Start();
        }
        else
        {
            updateCnt++;
            debugText.text = "update count : " + updateCnt;
        }
    }
    /*
    string getUpdatedGPSstring()
    {
        myGPSLocation = Input.location.lastData;

        MyLongtitude = Math.Round(myGPSLocation.longitude, 6);
        MyLatitude = Math.Round(myGPSLocation.latitude, 6);

        double DistanceToMeter;
        string storeRange;

        //두 점간의 거리
        DistanceToMeter = distance(MyLatitude, MyLongtitude, TargetLatitude, TargetLongtitude, DistUnit.meter);
        //DistanceToMeter = distance (37.507775, 127.039675, 37.507660, 127.039530, "meter"); // 20미터 이내 거리체크

        if (DistanceToMeter < 20)
        {// 건물의 높낮이 등 환경적인 요소로 인해 오차가 발생 할 수 있음.
            storeRange = "근처매장 O";

           // if (!v.flowAHandler.NotiOpenChk)
           //     v.flowAHandler.OpenNoti();
        }
        else
        {
            storeRange = "근처매장 X";
        }

        return "\n현재위치 :\n" +
                            "경도 - " + Math.Round(MyLatitude, 6) + "\n" +
                            "위도 - " + Math.Round(MyLongtitude, 6) +

            "\n\n" + "목표위치 : " + LocationName + "\n" +
                            "경도 - " + TargetLatitude + "\n" +
                            "위도 - " + TargetLongtitude +

            "\n\n목표와의거리 : 약 " + DistanceToMeter + "M" + "\n" +
                            "-------------------------------\n\n" +

                            storeRange;
    }

    */


    /**
     * 두 지점간의 거리 계산
     *
     * @param lat1 지점 1 위도
     * @param lon1 지점 1 경도
     * @param lat2 지점 2 위도
     * @param lon2 지점 2 경도
     * @param unit 거리 표출단위
     * @return
     */
    public double distance(double tLat, double tLong)
    {

        LocationInfo src = Input.location.lastData;

        double sLat = Math.Round(src.longitude, 6);
        double sLong = Math.Round(src.latitude, 6);

        double theta = sLong - tLong;
        double dist = Math.Sin(deg2rad(sLat)) * Math.Sin(deg2rad(tLat)) + Math.Cos(deg2rad(sLat)) * Math.Cos(deg2rad(tLat)) * Math.Cos(deg2rad(theta));
        Debug.Log("Lat : " + sLat.ToString() + " - " + tLat.ToString());
        Debug.Log("Long : " + sLong.ToString() + " - " + tLong.ToString());
        dist = Math.Acos(dist);
        dist = rad2deg(dist);
        dist = dist * 60 * 1.1515;
        dist = dist * 1609.344;         //meter

        return (dist);
    }

    // This function converts decimal degrees to radians
    static double deg2rad(double deg)
    {
        return (deg * Math.PI / 180.0);
    }

    // This function converts radians to decimal degrees
    static double rad2deg(double rad)
    {
        return (rad * 180 / Math.PI);
    }

}

enum DistUnit
{
    kilometer,
    meter
}
