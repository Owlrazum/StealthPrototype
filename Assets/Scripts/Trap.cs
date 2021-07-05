using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Trap : MonoBehaviour
{
    [SerializeField]
    private float seconds;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerLogic>() != null || other.GetComponent<GirlLogic>() != null)
        {
            //Debug.Log("SomeoneEnteredTrap");
            EventSystem.Instance.TrapEntered(seconds);
        }
    }
}
