using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] Ball ball;

    Player playerTeamA = null;
    Player playerTeamB = null;

    Player currentServer;

    public void OnPlayerJoin(PlayerInput playerInput)
    {
        Debug.Log("Player has joined.");
        Camera playerCamera = playerInput.transform.parent.GetComponentInChildren<Camera>();
        Player player = playerInput.GetComponent<Player>();
        if (playerTeamA == null)
        {
            Debug.Log("Assigning player to team A.");
            playerCamera.transform.position = new Vector3(0f, 7f, -12f);
            playerCamera.transform.rotation = Quaternion.Euler(30f, 0f, 0f);
            playerTeamA = player;
            player.Init("A", true);
            Instantiate(ball);
        }
        else
        {
            Debug.Log("Assigning player to team B.");
            playerCamera.transform.position = new Vector3(0f, 7f, 12f);
            playerCamera.transform.rotation = Quaternion.Euler(30f, -180f, 0f);
            playerTeamB = player;
            player.Init("B", false);
        }
    }
    
    public void SetCurrentServer(string server)
    {
        if (server == "A" || playerTeamB == null)
        {
            if (playerTeamB != null) playerTeamB.SetIsServer(false);
            playerTeamA.SetIsServer(true);
            currentServer = playerTeamA;
        }
        else
        {
            playerTeamA.SetIsServer(false);
            playerTeamB.SetIsServer(true);
            currentServer = playerTeamB;
        }
        playerTeamA.MoveAthletesToSpawn();
        if (playerTeamB != null) playerTeamB.MoveAthletesToSpawn();
    }

    public void SetReadyToServe()
    {
        currentServer.SetReadyToServe();
    }

    public Transform GetServerHoldPos()
    {
        return currentServer.GetServerHoldPos();
    }

}
