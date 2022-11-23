using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    enum State 
    { 
        Menu,
        Level,
        Transition
    }
    public static GameManager Instance { get; private set; }

    [SerializeField]
    private PlayerLogic player;
    public PlayerLogic Player
    {
        get { return player; }
        set { player = value; }
    }
    [SerializeField]
    private Transform parentOfGirls;

    [SerializeField]
    private GameObject WinPanel;
    [SerializeField]
    private Image fadingImage;

    List<GirlLogic> girls;

    private bool isVictoryObjectPickedUp = false;

    

    private void Start()
    {
        Instance = this;
        EventSystem.Instance.OnInitializeGameManager += Initialize;
    }

    private void Initialize()
    {
        EventSystem.Instance.OnVictoryObjectPickedUp += VictoryObjectPickedUp;
        EventSystem.Instance.OnEnteredVictoryZone += EnteredVictoryZone;
        EventSystem.Instance.OnStartGame += StartGame;
        EventSystem.Instance.StartGame();

        EventSystem.Instance.OnPlayerRunnedAway += EndLevel;

        girls = new List<GirlLogic>();
        for (int i = 0; i < parentOfGirls.childCount; i++)
        {
            Transform girlTransform= parentOfGirls.GetChild(i);
            GirlLogic girl = girlTransform.GetComponent<GirlLogic>();
            girls.Add(girl);
            girls[i].Player = player;
        }
        Debug.Log("Initialize happen");

        //Screen.SetResolution(1080, 1920, true);
    }

    void StartGame()
    {
       // Instantiate<Player>(player);
    }

    private void PlayerCaptured()
    {

    }

    private void VictoryObjectPickedUp()
    {
        isVictoryObjectPickedUp = true;
    }

    private void EnteredVictoryZone()
    {
        if (isVictoryObjectPickedUp)
        {
            Debug.Log("Victory");
        }
    }

    private void EndLevel()
    {
        WinPanel.SetActive(true);
    }

    IEnumerator Fading(bool isFadeIn)
    {
        int exitCondition = isFadeIn ? 255 : 0;

        while (fadingImage.color.a < exitCondition)
        {

        }
        fadingImage.color = new Color();
        yield return null;
    }

}
