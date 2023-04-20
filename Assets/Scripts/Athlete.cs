using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Athlete : MonoBehaviour
{
    [SerializeField] float spikeSlowDown = 0.4f;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float jumpHeight = 1f;
    [SerializeField] float bumpRange = 1f;
    [SerializeField] float spikeRange = 1f;
    [SerializeField] float gravityValue = -9.81f;
    [SerializeField] float rollDuration = 0.25f;
    [SerializeField] float rollSpeed = 20f;
    [SerializeField] float rollCooldown = 2f;

    [SerializeField] Transform arms;
    [SerializeField] LineRenderer spikeLine;
   
    public Transform posHold;
    Player player;
    Athlete otherAthlete;

    Target target;
    CharacterController controller;
    Renderer halo;

    Vector3 spikeAimPosition;
    Vector3 playerVelocity;

    Vector2 movementInput = Vector2.zero;
    Vector3 rollDirection;
    bool jumped = false;
    bool rolled = false;
    bool isSpiking = false;
    bool isRolling = false;
    bool groundedPlayer;
    float rollTime;
    float currentRollCooldown = 0f;
    string team;

    Ball ball;

    Vector3 opponentCourtCenter;

    public void Init(bool active, string team, Player player, Athlete otherAthlete, Target target)
    {
        SetActive(active);
        this.team = team;
        string opponentCourtTag = team == "A" ? "CourtTeamB" : "CourtTeamA";
        Debug.Log(opponentCourtTag);
        opponentCourtCenter = GameObject.FindGameObjectWithTag(opponentCourtTag).transform.position;
        this.player = player;
        this.otherAthlete = otherAthlete;
        this.target = target;
    }

    void Awake()
    {
        ball = FindObjectOfType<Ball>();
        controller = GetComponent<CharacterController>();
        halo = GetComponentInChildren<Halo>().GetComponent<Renderer>();
        spikeLine.enabled = false;
    }

    void Update()
    {
        UpdatePosition();
        UpdateSpike();
    }

    void UpdatePosition()
    {
        groundedPlayer = controller.isGrounded;

        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector3 move = new Vector3(movementInput.x, 0, movementInput.y);

        controller.Move(move * Time.deltaTime * moveSpeed);

        // Check for roll movement
        if (rolled && Mathf.Approximately(0f, currentRollCooldown)) 
        {
            if (move != Vector3.zero)
            {
                if (Mathf.Abs(move.x) >= Mathf.Abs(move.z)) {
                    rollDirection = new Vector3(Mathf.Sign(move.x), 0f, 0f);
                }
                else
                {
                    rollDirection = new Vector3(0f, 0f, Mathf.Sign(move.z));
                }
                rolled = false;
                isRolling = true;
                currentRollCooldown = rollCooldown;
                rollTime = rollDuration;
            }
        }
        if (currentRollCooldown > 0f)
        {
            currentRollCooldown -= Time.deltaTime;
            if (currentRollCooldown < 0f) currentRollCooldown = 0f;
        }
        if (rollTime > 0f)
        {
            rollTime -= Time.deltaTime;
            if (rollTime <= 0f) isRolling = false;
        }

        if (isRolling)
        {
            // move and adjust facing direction
            controller.Move(rollSpeed * Time.deltaTime * rollDirection);
            gameObject.transform.forward = rollDirection;

            if (AttemptBump())
            {
                isRolling = false;
                player.SwitchAthlete();
            }
        }
        else
        {
            if (jumped && groundedPlayer)
            {
                playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
            }
            if (move != Vector3.zero)
            {
                gameObject.transform.forward = move;
            }
            playerVelocity.y += gravityValue * Time.deltaTime;
            controller.Move(playerVelocity * Time.deltaTime);
        }
    }

    void UpdateSpike()
    {
        // Update position of spike
        if (ball == null) return;

        Vector3[] spikePositions = { ball.transform.position, spikeAimPosition };
        spikeLine.SetPositions(spikePositions);

        // Stop spiking if out of range
        StopSpikingIfOutOfRange();
    }


    // Public Controller Methods
    public void SetMovementInput(Vector2 input)
    {
        movementInput = input;
    }

    public void SetJumped(bool jumped)
    {
        this.jumped = jumped;
    }

    public void SetRolled (bool rolled)
    {
        this.rolled = rolled;
    }

    public void SetSpikeAim(Vector2 target)
    {
        spikeAimPosition = new Vector3(target.x, 0f, target.y);
    }

    public bool StartSpiking()
    {
        float distanceToBall = Vector3.Distance(ball.transform.position, arms.position);
        if (distanceToBall < spikeRange)
        {
            Debug.Log("Beginning spike mode: time will slow and spike line will show.");
            Time.timeScale = spikeSlowDown;
            spikeLine.enabled = true;
            isSpiking = true;
            return true;
        }
        return false;
    }

    public void StopSpikingIfOutOfRange()
    {
        if (!isSpiking) return;
        float distanceToBall = Vector3.Distance(ball.transform.position, arms.position);
        if (distanceToBall > spikeRange)
        {
            Time.timeScale = 1f;
            spikeLine.enabled = false;
            isSpiking = false;
        }
    }

    public bool AttemptSpike()
    {
        float distanceToBall = Vector3.Distance(ball.transform.position, arms.position);
        if (distanceToBall < spikeRange)
        {
            ball.Hit(team, "spike", spikeAimPosition);
            Time.timeScale = 1f;
            spikeLine.enabled = false;
            isSpiking = false;
            return true;
        }
        return false;
    }

    public bool AttemptBump()
    {
        float distanceToBall = Vector3.Distance(ball.transform.position, arms.position);
        if (distanceToBall < bumpRange)
        {
            Vector3 destination;
            if (ball.GetConsecutiveHits() == 2 && ball.GetLastHitter() == team)
            {
                destination = opponentCourtCenter;
            }
            else
            {
                destination = otherAthlete.transform.position;
            }
            return ball.Hit(team, "bump", destination);
        }
        return false;
    }

    public bool AttemptServe()
    {
        return ball.Hit(team, "serve", target.transform.position);
    }

    public void SetActive(bool active)
    {
        halo.enabled = active;
    }
}
