using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointOfPatrol : MonoBehaviour
{
    [SerializeField]
    private int seconds;
    public int Seconds { get { return seconds; } private set { } }
    [HideInInspector]
    public bool shouldWaitHere;

    [SerializeField]
    private PointOfPatrol nextPoint;
    public PointOfPatrol NextPoint { get { return nextPoint; } private set { } }

    private Vector3 position;
    public Vector3 Position { get { return position; } set { } }

    private void Awake()
    {
        position = transform.position;
        shouldWaitHere = seconds <= 0 ? false : true;
    }
    private void OnDrawGizmos()
    {
        //Gizmos.DrawCube(position, Vector3.one);
    }
}
