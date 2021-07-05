using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VictoryZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerLogic>())
        {
            EventSystem.Instance.EnteredVictoryZone();
        }
    }
}
