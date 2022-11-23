using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerCapture : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerLogic>() != null)
        {
            //EventSystem.Instance.PlayerCaptured();
        }
    }
}
