using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class compass : MonoBehaviour
{
    Quaternion currentHeading;

    void Start()
    {
        Input.location.Start();
        Input.compass.enabled = true;
    }

    void Update()
    {
        currentHeading = Quaternion.Euler(0, 0, Input.compass.trueHeading);

        transform.rotation = Quaternion.Slerp(transform.rotation,
            currentHeading, Time.deltaTime * 2f);
    }
}
