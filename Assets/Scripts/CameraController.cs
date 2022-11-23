using Cinemachine;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CameraController : MonoBehaviour
{
    private CinemachineFramingTransposer transposer;
    private Vector3 currentRotation;
    private int rotation;
    void Start()
    {
        transposer = GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineFramingTransposer>();

        EventSystem.Instance.OnPlayerSpotted += PlayerSpotted;
        EventSystem.Instance.OnCameraRotationStarted += CameraRotated;
        EventSystem.Instance.OnPlayerRunnedAway += FixateCamera;


        transposer.m_CameraDistance = 15;
    }
    private void PlayerSpotted()
    {
        transposer.m_CameraDistance = 30;
    }

    private void CameraRotated(int rot) 
    {
        rotation = rot;
        IEnumerator rotationCoroutine = RotateCamera();
        StartCoroutine(rotationCoroutine);
    }

    IEnumerator RotateCamera()
    {
        currentRotation = transform.rotation.eulerAngles;
        int rotationLeft = rotation;
        int deltaRotation = 2;
        while (rotationLeft > 0)
        {
            currentRotation.y += deltaRotation;
            rotationLeft -= deltaRotation; ;
            transform.rotation = Quaternion.Euler(currentRotation);
            yield return new WaitForFixedUpdate();
        }
        EventSystem.Instance.CameraRotationCompleted();
    }

    private void FixateCamera()
    {
        transposer.enabled = false;
    }
}
