using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoleShortcut : MonoBehaviour
{
    [SerializeField]
    private HoleShortcut otherHole;
    private Vector3 TranslationOfJump;
    [SerializeField]
    private float landY;
    [SerializeField]
    private bool isJumping;
    public bool IsReceiving { get; set; }

    private void Start()
    {
        IsReceiving = false;
        //Debug.Log("translation " + translation);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerLogic>() != null)
        {
            if (!IsReceiving)
            {
                otherHole.IsReceiving = true;
                Vector3 begin = other.transform.position;
                Vector3 end = otherHole.transform.position;
                TranslationOfJump = end - begin;
                EventSystem.Instance.ShortcutEntered(TranslationOfJump);
            } else
            {
                EventSystem.Instance.ShortcutExitted(landY);
                IsReceiving = false;
            }
        }
    }

   /* private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerLogic>() != null)
        {
            if (IsReceiving)
            {
                EventSystem.Instance.ShortcutExitted(positionY);
                IsReceiving = false;
            }
        }
    }*/
}
