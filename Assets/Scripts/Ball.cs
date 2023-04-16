using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hit
{
    public string hitType { get; }
    public Vector3 target { get; }

    public Hit(string hitType, Vector3 target)
    {
        this.hitType = hitType;
        this.target = target;
    }

}

enum BallPosition
{
    Air,
    OutOfBounds,
    CourtTeamA,
    CourtTeamB,
}

public class Ball : MonoBehaviour
{

    static Ball _instance;
    public static Ball Instance { get { return _instance; } }


    // Implements Singleton Pattern
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    ScoreManager scoreManager;
    PlayerManager playerManager;

    [SerializeField] float spikeTime = 0.5f;
    [SerializeField] float serveTime = 1.5f;
    [SerializeField] float bumpTime = 3f;
    [SerializeField] float airResistance = 2f;
    [SerializeField] float returnToServerTime = 1.0f;

    [SerializeField] bool allowAnyHits = false;

    Rigidbody rb;
   
    bool isDead = false;
    string lastHitter;
    // Queue<Hit> hitQueue = new Queue<Hit>();

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        scoreManager = FindObjectOfType<ScoreManager>();
        playerManager = FindObjectOfType<PlayerManager>();
        GiveBallToServer();
    }

    void FixedUpdate()
    {
        // ProcessHitQueue();
        ApplyAirResistance();
    }

    // Physics
    private void ApplyAirResistance()
    {
        float v = rb.velocity.magnitude;
        var direction = -rb.velocity.normalized;
        var forceAmount = v * airResistance;
        rb.AddForce(direction * forceAmount);
    }
        
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("CourtTeamA"))
        {
            HandleDeadBall(BallPosition.CourtTeamA);
        }
        else if (collision.gameObject.CompareTag("CourtTeamB"))
        {
            HandleDeadBall(BallPosition.CourtTeamB);
        }
        else if (collision.gameObject.CompareTag("Out of Bounds"))
        {
            HandleDeadBall(BallPosition.CourtTeamA);
        }
    }

    void HandleDeadBall(BallPosition ballPosition)
    {
        if (!isDead && ballPosition != BallPosition.Air)
        {
            isDead = true;
            Debug.Log("Ball is dead.");
            if (ballPosition == BallPosition.CourtTeamA)
            {
                Debug.Log("(Point Team B) Ball landed in team A court.");
                scoreManager.IncrementTeamBScore();
                playerManager.SetCurrentServer("B");
            }
            else if (ballPosition == BallPosition.CourtTeamB)
            {
                Debug.Log("(Point Team A) Ball landed in team B court.");
                scoreManager.IncrementTeamAScore();
                playerManager.SetCurrentServer("A");
            }
            else
            {
                if (lastHitter == "A")
                {
                    Debug.Log("(Point Team B) Ball landed out of bounds; last hitter Was A.");
                    scoreManager.IncrementTeamBScore();
                    playerManager.SetCurrentServer("B");
                }
                else if (lastHitter == "B")
                {
                    Debug.Log("(Point Team A) Ball landed out of bounds; last hitter Was B.");
                    scoreManager.IncrementTeamAScore();
                    playerManager.SetCurrentServer("A");
                }
            }
            IEnumerator coroutine = GiveBallToServer();
            StartCoroutine(coroutine);
        }
    }

    public IEnumerator GiveBallToServer()
    {
        Debug.Log("Waiting before giving ball to server.");
        yield return new WaitForSeconds(returnToServerTime);

        Debug.Log("Resetting ball rigidbody.");
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        Debug.Log("Giving ball to server.");
        Transform holdPos = playerManager.GetServerHoldPos();
        transform.parent = holdPos;
        transform.position = holdPos.position;
        transform.rotation = Quaternion.identity;
    }

    private void ReleaseBallFromServer()
    {
        isDead = false;
        rb.isKinematic = false;
        transform.parent = null;
    }

    // Ball Hitting Functions
    public bool Hit(string team, string hitType, Vector3 target)
    {
        if (!allowAnyHits)
        {
            if (hitType == "serve" && transform.parent == null)
            {
                return false;
            }
            if (hitType != "serve" && transform.parent != null)
            {
                return false;
            }
        }
        lastHitter = team;
        ReleaseBallFromServer();
        if (hitType == "serve") ExecuteServe(target);
        if (hitType == "bump") ExecuteBump(target);
        if (hitType == "spike") ExecuteSpike(target);
        // hitQueue.Enqueue(new Hit(hitType, target));
        return true;
    }

    private void ExecuteServe(Vector3 target)
    {
        HitToPoint(target, serveTime);
    }

    private void ExecuteBump(Vector3 target)
    {
        HitToPoint(target, bumpTime);
    }

    private void ExecuteSpike(Vector3 target)
    {
        HitToPoint(target, spikeTime);
    }

    private void HitToPoint(Vector3 target, float time)
    {
        float dx = target.x - transform.position.x;
        float dy = target.y - transform.position.y;
        float dz = target.z - transform.position.z;

        float c = (1 - Mathf.Exp(-airResistance * time));
        float velX = dx * airResistance / c;
        float velY = (Physics.gravity.y / airResistance) + (dy * airResistance - Physics.gravity.y * time) / c;
        float velZ = dz * airResistance / c;

        rb.velocity = new Vector3(velX, velY, velZ);
    }

    /*
    void ProcessHitQueue()
    {
        while (hitQueue.Count > 0)
        {
            Hit hit = hitQueue.Dequeue();
            string hitType = hit.hitType;
            Transform target = hit.target;
            if (hitType == "serve") ExecuteServe(target);
            if (hitType == "bump") ExecuteBump(target);
            if (hitType == "spike") ExecuteSpike(target);
        }
    }
    */
}
