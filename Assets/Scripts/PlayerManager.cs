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
            player.team = "A";
            player.firstServer = true;
            Instantiate(ball);
        }
        else
        {
            Debug.Log("Assigning player to team B.");
            playerCamera.transform.position = new Vector3(0f, 7f, 12f);
            playerCamera.transform.rotation = Quaternion.Euler(30f, -180f, 0f);
            playerTeamB = player;
            player.team = "B";
            player.firstServer = false;
        }
    }
    
    public void SetCurrentServer(string server)
    {
        playerTeamA.MoveAthletesToSpawn();
        if (playerTeamB != null) playerTeamB.MoveAthletesToSpawn();
        if (server == "A" || playerTeamB == null)
        {
            if (playerTeamB != null) playerTeamB.SetIsServer(false);
            playerTeamA.SetIsServer(true);
            currentServer = playerTeamA;
        }
        else if (server == "B")
        {
            playerTeamA.SetIsServer(false);
            playerTeamB.SetIsServer(true);
            currentServer = playerTeamB;
        }
    }

    public Transform GetServerHoldPos()
    {
        return currentServer.GetServerHoldPos();
    }

}
