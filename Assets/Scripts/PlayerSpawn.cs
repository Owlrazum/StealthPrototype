using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private Cinemachine.CinemachineVirtualCamera playerCamera;

    void Start()
    {
        player = Instantiate(player, transform.position, transform.rotation);
        playerCamera.Follow = player.transform;
        GameManager.Instance.Player = player.GetComponent<PlayerLogic>();
        EventSystem.Instance.InitializeGameManager();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
