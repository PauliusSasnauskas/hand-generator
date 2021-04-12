using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject whatToFollow;
    private Vector3 initialDifference;
    // Start is called before the first frame update
    void Start()
    {
        initialDifference = transform.position - whatToFollow.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = whatToFollow.transform.position + initialDifference;
    }
}
