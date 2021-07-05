using System;
using UnityEngine;

public class EventSystem : MonoBehaviour
{
    public static EventSystem Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
       //Debug.Log("EventSystem Activated");
    }

    #region GameManagerEvents
    public event Action OnStartGame;
    public void StartGame()
    {
        OnStartGame?.Invoke();
    }

    public event Action OnStopGame;
    public void StopGame()
    {
        OnStopGame?.Invoke();
    }

    public event Action OnContinueGame;
    public void ContinueGame()
    {
        OnContinueGame?.Invoke();
    }

    #region Initializers
    public event Action OnInitializeGameManager;
    public void InitializeGameManager()
    {
        OnInitializeGameManager?.Invoke();
    }

    #endregion
    #endregion

    #region PlayerCharacterEvents

    public event Action<int> OnCameraRotationStarted;
    public void CameraRotationStarted(int rot)
    {
        OnCameraRotationStarted?.Invoke(rot);
    }

    public event Action OnCameraRotationCompleted;
    public void CameraRotationCompleted()
    {
        OnCameraRotationCompleted?.Invoke();
    }

    public event Action OnPlayerSpotted;
    public void PlayerSpotted()
    {
        OnPlayerSpotted?.Invoke();
    }

    public event Action OnPlayerCaptured;
    public void PlayerCaptured()
    {
        //Debug.Log("PlayerCaptured");
        //OnPlayerCaptured?.Invoke();
    }

    public event Action OnVictoryObjectPickedUp;
    public void VictoryObjectPickedUp()
    {
        OnVictoryObjectPickedUp?.Invoke();
    }

    public event Action OnEnteredVictoryZone;
    public void EnteredVictoryZone()
    {
        OnEnteredVictoryZone?.Invoke();
    }

    public event Action OnPlayerRunnedAway;
    public void PlayerRunnedAway()
    {
        OnPlayerRunnedAway?.Invoke();
    }
    #endregion

    #region ShortcutEvents
    public event Action<Vector3> OnShortcutEntered;
    public void ShortcutEntered(Vector3 translation)
    {
        OnShortcutEntered?.Invoke(translation);
    }

    public event Action<float> OnShortcutExitted;
    public void ShortcutExitted(float positionY)
    {
        OnShortcutExitted?.Invoke(positionY);
    }
    #endregion

    public event Action<float> OnTrapEntered;
    public void TrapEntered (float secondsDelay)
    {
        //Debug.Log("TrapEvent Fired");
        OnTrapEntered?.Invoke(secondsDelay);
    }

}
