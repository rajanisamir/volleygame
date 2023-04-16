using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    int teamAScore = 0;
    int teamBScore = 0;

    [SerializeField] TextMeshProUGUI scoreText;


    // Start is called before the first frame update
    void Start()
    {
        UpdateScoreText();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateScoreText()
    {
        scoreText.text = teamAScore.ToString() + " - " + teamBScore.ToString();
    }

    public void IncrementTeamAScore()
    {
        teamAScore++;
        UpdateScoreText();
    }

    public void IncrementTeamBScore()
    {
        teamBScore++;
        UpdateScoreText();
    }
}
