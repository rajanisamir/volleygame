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

    [SerializeField] Transform arms;
    [SerializeField] LineRenderer spikeLine;
   
    public Transform posHold;

    Target target;
    CharacterController controller;
    Renderer halo;

    Vector3 spikeAimPosition;
    Vector3 playerVelocity;
    Vector2 movementInput = Vector2.zero;
    bool jumped = false;
    bool isSpiking = false;
    bool groundedPlayer;
    public string team;

    Ball ball;

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

        if (move != Vector3.zero)
        {
            gameObject.transform.forward = move;
        }

        if (jumped && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
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

    public bool AttemptBump(Vector3 destination)
    {
        float distanceToBall = Vector3.Distance(ball.transform.position, arms.position);
        if (distanceToBall < bumpRange)
        {
            return ball.Hit(team, "bump", destination);
        }
        return false;
    }

    public bool AttemptServe()
    {
        return ball.Hit(team, "serve", target.transform.position);
    }

    public void SetTarget(Target target)
    {
        this.target = target;
    }

    public void SetActive(bool active)
    {
        halo.enabled = active;
    }
}
