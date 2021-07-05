using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class VictoryObject : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerLogic>() != null)
        {
            Debug.Log("Picked up");
            EventSystem.Instance.VictoryObjectPickedUp();
            gameObject.SetActive(false);
        }
    }
}
