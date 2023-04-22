using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] Target targetTemplate;
    [SerializeField] Athlete athleteTemplate;
    [SerializeField] int athletesPerPlayer;

    Target target;

    Athlete currentAthlete;
    Athlete otherAthlete;
    Athlete leftAthlete;
    Athlete rightAthlete;

    string team;
    bool firstServer = false;

    float opponentCourtLeft;
    float opponentCourtRight;
    float opponentCourtFront;
    float opponentCourtBack;

    float teamCourtLeft;
    float teamCourtCenterX;
    float teamCourtRight;
    float teamCourtFront;
    float teamCourtCenterZ;
    float teamCourtBack;

    Vector3 leftSpawnPositionServe;
    Vector3 rightSpawnPositionServe;
    Vector3 leftSpawnPositionReceive;
    Vector3 rightSpawnPositionReceive;

    bool isServer = false;
    bool readyToServe = false;
    bool isSpiking = false;

    public void Init(string team, bool firstServer)
    {
        this.team = team;
        this.firstServer = firstServer;
    }

    void Awake()
    {
        GetCourtPositions();
        SpawnTarget();
        SpawnAthletes();
        SetIsServer(firstServer);
    }

    void GetCourtPositions()
    {
        string teamCourtTag = team == "A" ? "CourtTeamA" : "CourtTeamB";
        string opponentCourtTag = team == "A" ? "CourtTeamB" : "CourtTeamA";

        Transform teamCourt = GameObject.FindGameObjectWithTag(teamCourtTag).transform;
        Transform opponentCourt = GameObject.FindGameObjectWithTag(opponentCourtTag).transform;

        int teamMultiplier = team == "A" ? 1 : -1;

        teamCourtLeft = teamCourt.position.x - teamMultiplier * teamCourt.localScale.x / 2;
        teamCourtCenterX = teamCourt.position.x;
        teamCourtRight = teamCourt.position.x + teamMultiplier * teamCourt.localScale.x / 2;
        teamCourtFront = teamCourt.position.z + teamMultiplier * teamCourt.localScale.z / 2;
        teamCourtCenterZ = teamCourt.position.z;
        teamCourtBack = teamCourt.position.z - teamMultiplier * teamCourt.localScale.z / 2;

        opponentCourtLeft = opponentCourt.position.x - teamMultiplier * opponentCourt.localScale.x / 2;
        opponentCourtRight = opponentCourt.position.x + teamMultiplier * opponentCourt.localScale.x / 2;
        opponentCourtFront = opponentCourt.position.z - teamMultiplier * opponentCourt.localScale.z / 2;
        opponentCourtBack = opponentCourt.position.z + teamMultiplier * opponentCourt.localScale.z / 2;

        leftSpawnPositionServe = new Vector3(teamCourtLeft, 0f, teamCourtBack);
        rightSpawnPositionServe = new Vector3(teamCourtRight, 0f, teamCourtBack);
        leftSpawnPositionReceive = new Vector3((teamCourtLeft + teamCourtCenterX) / 2, 0f, teamCourtCenterZ);
        rightSpawnPositionReceive = new Vector3((teamCourtRight + teamCourtCenterX) / 2, 0f, teamCourtCenterZ);
    }

    void SpawnTarget()
    {
        target = Instantiate(targetTemplate);
        Renderer targetRenderer = target.GetComponent<Renderer>();
        targetRenderer.material.SetColor("_Color", team == "A" ? Color.red : Color.blue);
    }

    void SpawnAthletes()
    {
        if (firstServer)
        {
            leftAthlete = Instantiate(athleteTemplate, leftSpawnPositionServe, Quaternion.identity);
            rightAthlete = Instantiate(athleteTemplate, rightSpawnPositionServe, Quaternion.identity);
        }
        else
        {
            leftAthlete = Instantiate(athleteTemplate, leftSpawnPositionReceive, Quaternion.identity);
            rightAthlete = Instantiate(athleteTemplate, rightSpawnPositionReceive, Quaternion.identity);
        }

        leftAthlete.Init(true, team, this, rightAthlete, target);
        rightAthlete.Init(false, team, this, leftAthlete, target);

        currentAthlete = leftAthlete;
        otherAthlete = rightAthlete;
    }

    public void MoveAthletesToSpawn()
    {
        Debug.Log("Moving athletes to spawn.");
        CharacterController leftCharacterController = leftAthlete.GetComponent<CharacterController>();
        CharacterController rightCharacterController = rightAthlete.GetComponent<CharacterController>();

        leftCharacterController.enabled = false;
        rightCharacterController.enabled = false;

        if (isServer)
        {
            leftAthlete.transform.position = leftSpawnPositionServe;
            rightAthlete.transform.position = rightSpawnPositionServe;
        }
        else
        {
            leftAthlete.transform.position = leftSpawnPositionReceive;
            rightAthlete.transform.position = rightSpawnPositionReceive;
        }

        leftCharacterController.enabled = true;
        rightCharacterController.enabled = true;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 movementInput = context.ReadValue<Vector2>();
        

        if (readyToServe)
        {
            leftAthlete.SetMovementInput(Vector2.zero);
        }
        else if (isSpiking)
        {
            leftAthlete.SetMovementInput(Vector2.zero);

            // Compute spike aim
            float aimX = Mathf.Lerp(opponentCourtLeft, opponentCourtRight, Mathf.InverseLerp(-1f, 1f, movementInput.x));
            float aimY = Mathf.Lerp(opponentCourtFront, opponentCourtBack, Mathf.InverseLerp(-1f, 1f, movementInput.y));
            currentAthlete.SetSpikeAim(new Vector2(aimX, aimY));
        }
        else
        {
            // need to flip input for team B because of camera direction
            if (team == "B") movementInput = -movementInput;
            leftAthlete.SetMovementInput(movementInput);
        }
    }

    public void SetTargetVisible(bool visible)
    {
        target.GetComponent<Renderer>().enabled = visible;
    }

    public void SwitchAthlete()
    {
        currentAthlete.SetActive(false);
        var temp = otherAthlete;
        otherAthlete = currentAthlete;
        currentAthlete = temp;
        currentAthlete.SetActive(true);
    }

    private void SwitchServer()
    {
        var temp = leftAthlete;
        leftAthlete = rightAthlete;
        rightAthlete = temp;
        if (currentAthlete != leftAthlete) SwitchAthlete();
    }
   
    public void SetIsServer(bool server)
    {   
        SetTargetVisible(server);
        isServer = server;
        // If player wasn't the server, need to switch the athlete who is serving.
        if (server && !isServer)
        {
            SwitchServer();
        }
    }

    public void SetReadyToServe()
    {
        readyToServe = true;
    }

    public Transform GetServerHoldPos()
    {
        return leftAthlete.GetHoldPos();
    }

    // Input System Callbacks
    public void OnTargetMove(InputAction.CallbackContext context)
    {
        Vector2 movementInput = context.ReadValue<Vector2>();
        // flip input for team B because of camera direction!
        if (team == "B") movementInput = -movementInput;
        if (readyToServe)
        {
            rightAthlete.SetMovementInput(Vector2.zero);
            target.SetMovementInput(movementInput);
        }
        else
        {
            rightAthlete.SetMovementInput(movementInput);
            target.SetMovementInput(Vector2.zero);
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        currentAthlete.SetJumped(context.action.triggered);
        otherAthlete.SetJumped(false);
    }

    public void OnSpike(InputAction.CallbackContext context)
    {
        if (readyToServe) return;
        if (context.started)
        {
            isSpiking = currentAthlete.StartSpiking();
        }
        else if (context.canceled)
        {
            if (isSpiking) {
                currentAthlete.AttemptSpike();
            }
            isSpiking = false;
        }
    }

    public void OnSet(InputAction.CallbackContext context)
    {
        if (context.action.triggered)
        {
            if (readyToServe)
            {
                readyToServe = false;
                currentAthlete.AttemptServe();
                SetTargetVisible(false);
            }
            else
            {
                if (currentAthlete.AttemptSet()) SwitchAthlete();
            }
        }
    }

    public void OnBump(InputAction.CallbackContext context)
    {
        if (readyToServe) return;
        if (context.action.triggered)
        {
            if (currentAthlete.AttemptBump()) SwitchAthlete();
        }
    }

    public void OnSwitch(InputAction.CallbackContext context)
    {
        if (context.action.triggered)
        {
            SwitchAthlete();
        }
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        currentAthlete.SetRolled(context.action.triggered);
    }
}
