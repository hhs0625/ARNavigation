using UnityEngine;
using System.Collections;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

public class User
{
    public string username;

    public User()
    {
    }

    public User(string username)
    {
        this.username = username;
    }
}


public class Facility
{
    public string ID;
    public string name;
    public double latitude;
    public double longitude;
    public double dist;
    public string pic;
    public List<Theater> theaterList = new List<Theater>();

    public Facility()
    {

    }

    public Facility(string _ID, string _name, double _latitude, double _longitude)
    {
        this.ID = _ID;
        this.name = _name;
        this.latitude = _latitude;
        this.longitude = _longitude;
    }
}


public class Theater
{
    public string ID;
    public string name;
    public double latitude;
    public double longitude;
    public double dist;
    public string pic;

    public Theater()
    {

    }

    public Theater(string _ID, string _name, double _latitude, double _longitude)
    {
        this.ID = _ID;
        this.name = _ID;
        this.latitude = _latitude;
        this.longitude = _longitude;
    }
}


public class firebase : MonoBehaviour
{

    private static firebase _instance = null;

    public static firebase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType(typeof(firebase)) as firebase;

                if (_instance == null)
                {
                    Debug.LogError("There's no active ManagerClass object");
                }
            }

            return _instance;
        }
    }


    DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
    DatabaseReference mDatabaseRef;
    public int DBversion;
    public List<Facility> ftyList = new List<Facility>();
    public int nearFtyIdx = -1;
    // Use this for initialization
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();

                try
                {
                    // 실행하고자 하는 문장들
                    loadFacility();
                }
                catch (Exception ex)
                {
                    // 에러 처리/로깅 등
                    Debug.Log(ex);
                    throw;
                }
                    
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });

    }
    // Initialize the Firebase database:
    protected virtual void InitializeFirebase()
    {
        FirebaseApp app = FirebaseApp.DefaultInstance;
        // NOTE: You'll need to replace this url with your Firebase App's database
        // path in order for the database connection to work correctly in editor.
        app.SetEditorDatabaseUrl("https://arnavigation-dd351.firebaseio.com/");
        if (app.Options.DatabaseUrl != null)
            app.SetEditorDatabaseUrl(app.Options.DatabaseUrl);

        mDatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;
    }
    private void writeNewUser(string userId, string name)
    {
        User user = new User(name);
        string json = JsonUtility.ToJson(user);
        Debug.Log("Try Write : " + name + " " + userId);
        mDatabaseRef.Child("users").Child(userId).SetRawJsonValueAsync(json);
    }
    /*
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            writeNewUser("USERID1234", "unitytest");
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            loadFacility();
        }
    }
    */
    void loadFacility()
    {
        ftyList.Clear();
        FirebaseDatabase.DefaultInstance.GetReference("facility").GetValueAsync().ContinueWith(task => {
            if (task.IsFaulted)
            {
                Debug.Log("failed");
            }
            else if (task.IsCompleted)
            {
                Firebase.Database.DataSnapshot snapshot = task.Result;

                foreach (var childSnapshot in snapshot.Children)
                {

                    string fID = childSnapshot.Key;
                    string fName = childSnapshot.Child("name").Value.ToString();
                    string fLat = childSnapshot.Child("latitude").Value.ToString();
                    string fLong = childSnapshot.Child("longitude").Value.ToString();

                    Debug.Log(fID + " " + fName + " " + fLat + " " + fLong + " theaters : " + childSnapshot.Child("theater").ChildrenCount);
                    Facility fty = new Facility(fID, fName, double.Parse(fLat), double.Parse(fLong));
            
                    foreach (var _childSnapshot in childSnapshot.Child("theater").Children)
                    {
                        string tID = _childSnapshot.Key;
                        string tName = _childSnapshot.Child("name").Value.ToString();
                        string tLat = _childSnapshot.Child("latitude").Value.ToString();
                        string tLong = _childSnapshot.Child("longitude").Value.ToString();
                        //Debug.Log(tID + " " + tName + " " + tLat);
                        Debug.Log(tID + " " + tName + " " + tLat + " " + tLong);
                        Theater tht = new Theater(tID, tName, double.Parse(tLat), double.Parse(tLong));
                        fty.theaterList.Add(tht);
                    }
                    ftyList.Add(fty);
                }
            }
        });
    }
}