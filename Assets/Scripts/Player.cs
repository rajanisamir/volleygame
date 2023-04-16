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

    public string team;

    bool isServer = false;
    bool readyToServe = false;
    public bool firstServer = false;

    bool isSpiking = false;

    float opponentCourtLeft;
    float opponentCourtRight;
    float opponentCourtFront;
    float opponentCourtBack;

    float teamCourtLeft;
    float teamCourtRight;
    float teamCourtFront;
    float teamCourtBack;

    Vector3 leftSpawnPosition;
    Vector3 rightSpawnPosition;

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
        int opponentMultiplier = team == "A" ? -1 : 1;

        teamCourtLeft = teamCourt.position.x - teamMultiplier * teamCourt.localScale.x / 2;
        teamCourtRight = teamCourt.position.x + teamMultiplier * teamCourt.localScale.x / 2;
        teamCourtFront = teamCourt.position.z + teamMultiplier * teamCourt.localScale.z / 2;
        teamCourtBack = teamCourt.position.z - teamMultiplier * teamCourt.localScale.z / 2;

        opponentCourtLeft = opponentCourt.position.x - opponentMultiplier * opponentCourt.localScale.x / 2;
        opponentCourtRight = opponentCourt.position.x + opponentMultiplier * opponentCourt.localScale.x / 2;
        opponentCourtFront = opponentCourt.position.z + opponentMultiplier * opponentCourt.localScale.z / 2;
        opponentCourtBack = opponentCourt.position.z - opponentMultiplier * opponentCourt.localScale.z / 2;

        leftSpawnPosition = new Vector3(teamCourtLeft, 0f, teamCourtBack);
        rightSpawnPosition = new Vector3(teamCourtRight, 0f, teamCourtBack);
    }

    void SpawnTarget()
    {
        target = Instantiate(targetTemplate);
        Renderer targetRenderer = target.GetComponent<Renderer>();
        targetRenderer.material.SetColor("_Color", team == "A" ? Color.red : Color.blue);
    }

    void SpawnAthletes()
    {
        leftAthlete = Instantiate(athleteTemplate, leftSpawnPosition, Quaternion.identity);
        rightAthlete = Instantiate(athleteTemplate, rightSpawnPosition, Quaternion.identity);

        leftAthlete.team = team;
        rightAthlete.team = team;

        leftAthlete.SetTarget(target);
        rightAthlete.SetTarget(target);

        leftAthlete.SetActive(true);
        rightAthlete.SetActive(false);

        currentAthlete = leftAthlete;
        otherAthlete = rightAthlete;
    }

    public void MoveAthletesToSpawn()
    {
        Debug.Log("Moving athletes to spawn.");
        leftAthlete.GetComponent<CharacterController>().enabled = false;
        rightAthlete.GetComponent<CharacterController>().enabled = false;
        leftAthlete.transform.position = leftSpawnPosition;
        rightAthlete.transform.position = rightSpawnPosition;
        leftAthlete.GetComponent<CharacterController>().enabled = true;
        rightAthlete.GetComponent<CharacterController>().enabled = true;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 movementInput = context.ReadValue<Vector2>();
        // need to flip input for team B because of camera direction
        if (team == "B") movementInput = -movementInput;

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
            leftAthlete.SetMovementInput(movementInput);
        }
    }

    public void OnTargetMove(InputAction.CallbackContext context)
    {
        Vector2 movementInput = context.ReadValue<Vector2>();
        // need to flip input for team B because of camera direction
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
        if (context.started)
        {
            isSpiking = currentAthlete.StartSpiking();
        }
        else if (context.canceled)
        {
            currentAthlete.AttemptSpike();
            isSpiking = false;
        }
    }

    public void OnServe(InputAction.CallbackContext context)
    {
        if (context.action.triggered && isServer)
        {
            if (currentAthlete.AttemptServe())
            {
                SetTargetVisible(false);
                readyToServe = false;
            }
        }
    }

    public void OnBump(InputAction.CallbackContext context)
    {
        if (context.action.triggered)
        {
            Vector3 destination = otherAthlete.transform.position;
            if (currentAthlete.AttemptBump(destination)) SwitchAthlete();
        }
    }

    public void OnSwitch(InputAction.CallbackContext context)
    {
        if (context.action.triggered)
        {
            SwitchAthlete();
        }
    }

    public void SetTargetVisible(bool visible)
    {
        target.GetComponent<Renderer>().enabled = visible;
    }

    private void SwitchAthlete()
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
        readyToServe = server;
        isServer = server;
        // If player wasn't the server, need to switch the athlete who is serving.
        if (server && !isServer)
        {
            SwitchServer();
        }
    }

    public Transform GetServerHoldPos()
    {
        return leftAthlete.posHold;
    }
}
