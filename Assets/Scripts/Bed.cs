using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bed : MonoBehaviour
{
    [SerializeField]
    private Vector3 rotationForSleeping;
    [SerializeField]
    private Vector3 positionForAwake;
    private Vector3 sleepPosition;
    private void Awake()
    {
        sleepPosition = new Vector3(-0.1f, 1.25f, 0.3f);
    }
    public Vector3 getRotationForSleeping()
    {
        rotationForSleeping += transform.rotation.eulerAngles;
        return rotationForSleeping;
    }
    public Vector3 getPositionForSleeping()
    {
        return transform.TransformPoint(sleepPosition);
    }
    public Vector3 GetPositionForAwake()
    {
        return transform.TransformPoint(positionForAwake);
    }
}
