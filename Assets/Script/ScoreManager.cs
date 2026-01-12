using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public int wins;
    public int losses;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadScores();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void LoadScores()
    {
        wins = PlayerPrefs.GetInt("Wins", 0);
        losses = PlayerPrefs.GetInt("Losses", 0);
    }

    public void AddWin()
    {
        wins++;
        PlayerPrefs.SetInt("Wins", wins);
        PlayerPrefs.Save();

        Debug.Log("WIN ADDED → Total Wins: " + wins);
    }

    public void AddLoss()
    {
        losses++;
        PlayerPrefs.SetInt("Losses", losses);
        PlayerPrefs.Save();

        Debug.Log("LOSS ADDED → Total Losses: " + losses);
    }

    public void ResetScores()
    {
        wins = 0;
        losses = 0;

        PlayerPrefs.DeleteKey("Wins");
        PlayerPrefs.DeleteKey("Losses");
        PlayerPrefs.Save();

        Debug.Log("Scores reset");
    }
}
