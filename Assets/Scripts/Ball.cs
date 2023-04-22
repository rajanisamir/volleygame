using System.Collections;
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

    [SerializeField] float spikeSpeed = 7f;
    [SerializeField] float serveTime = 1.5f;
    [SerializeField] float setTime = 1.5f;
    [SerializeField] float bumpTime = 3f;
    [SerializeField] float airResistance = 2f;
    [SerializeField] float returnToServerTime = 1.0f;
    [SerializeField] int hitLimit = 3;

    Rigidbody rb;
   
    bool isDead = false;
    string lastHitter;
    int consecutiveTeamHits;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        scoreManager = FindObjectOfType<ScoreManager>();
        playerManager = FindObjectOfType<PlayerManager>();
        GiveBallToServer();
    }

    void FixedUpdate()
    {
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
    
    // Handling Dead Ball
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("CourtTeamA"))
        {
            HandleDeadBall("B");
        }
        else if (collision.gameObject.CompareTag("CourtTeamB"))
        {
            HandleDeadBall("A");
        }
        else if (collision.gameObject.CompareTag("Out of Bounds"))
        {
            if (lastHitter == "A") HandleDeadBall("B");
            else HandleDeadBall("A");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Under Net")) {
            if (lastHitter == "A") HandleDeadBall("B");
            else HandleDeadBall("A");
        }
    }

    void HandleDeadBall(string scoringTeam)
    {
        if (!isDead)
        {
            Debug.Log("Ball is dead.");
            isDead = true;
            consecutiveTeamHits = 0;

            scoreManager.IncrementTeamScore(scoringTeam);
            playerManager.SetCurrentServer(scoringTeam);

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
        playerManager.SetReadyToServe();
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
        if (lastHitter == team && consecutiveTeamHits == hitLimit) return false;
        else if (lastHitter == team) consecutiveTeamHits++;
        else consecutiveTeamHits = 1;
        lastHitter = team;  
        ReleaseBallFromServer();

        float multipliedSpikeSpeed = spikeSpeed + Mathf.Max(0f, (transform.position.y - 2f)) * 4f;
        float spikeTime = Vector3.Distance(target, transform.position) / multipliedSpikeSpeed;

        if (hitType == "serve") HitToPoint(target, serveTime);
        if (hitType == "set") HitToPoint(target, setTime);
        if (hitType == "bump") HitToPoint(target, bumpTime);
        if (hitType == "spike") HitToPoint(target, spikeTime);
        return true;
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

    public string GetLastHitter()
    {
        return lastHitter;
    }

    public int GetConsecutiveHits()
    {
        return consecutiveTeamHits;
    }
}
