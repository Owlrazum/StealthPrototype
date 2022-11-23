using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnCamera : MonoBehaviour
{
    [SerializeField]
    private int rotation = 0;

    private bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerLogic>() != null)
        {
            if (!isTriggered)
            {
                //EventSystem.Instance.CameraRotationStarted(rotation);
                //isTriggered = true;
            }
        }
    }
}
