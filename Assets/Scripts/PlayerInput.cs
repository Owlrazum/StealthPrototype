using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public static PlayerInput current { get; private set; }

    private float moveX;
    private float moveZ;
    private bool isSneakButtonPressed;
    private bool isInSneakMode;

    private bool isGamePaused = false;

    private bool isTestButtonPressed = false;


    IEnumerator UpdateCoroutine;
    private int currentFinger = -1;

    private void Awake()
    {
        current = this;
        // Debug.Log("EventSystem Activated");
        moveX = 0;
        moveZ = 0;
    }
    private void Start()
    {
        if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            UpdateCoroutine = UpdateDesctop();    
        } else if (SystemInfo.deviceType == DeviceType.Handheld)
        {
            UpdateCoroutine = UpdateMobile();
        }
        StartCoroutine(UpdateCoroutine);
    }

    IEnumerator UpdateMobile()
    {
        Touch[] touches = Input.touches;
        for (int i = 0; i < touches.Length; i++)
        {
            if (touches[i].phase == TouchPhase.Began)
            {
                currentFinger = touches[i].fingerId;
            }
        }

        moveX = Input.GetAxis("Horizontal");
        moveZ = Input.GetAxis("Vertical");
        yield return null;
    }

    IEnumerator UpdateDesctop()
    {
        while(true)
        {
            //Debug.Log("Update Desctop is running");
            moveX = Input.GetAxis("Horizontal");
            moveZ = Input.GetAxis("Vertical");

            isSneakButtonPressed = Input.GetButtonDown("Fire1"); // left ctrl

            isTestButtonPressed = Input.GetButtonDown("Fire2"); // left alt

            if (isSneakButtonPressed)
            {
                isInSneakMode = !isInSneakMode;
            }

            if (Input.GetButtonDown("Cancel"))
            {
                if (isGamePaused)
                {
                    EventSystem.Instance.ContinueGame();
                }
                else if (!isGamePaused)
                {
                    EventSystem.Instance.StopGame();
                }
                isGamePaused = !isGamePaused;
            }
            yield return null;
        }
        
    }
    public (float, float) GetMoveData()
    {
        return (moveX, moveZ);
    }

    public bool IsInSneakMode()
    {
        return isInSneakMode;
    }
}
