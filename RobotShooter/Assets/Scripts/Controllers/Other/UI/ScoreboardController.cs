using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreboardController : MonoBehaviour
{
    public Text bestScore;
    public Text[] lastScoreTexts;

    void Start()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        bestScore.text = ConvertToFourDigit(PlayerPrefs.GetInt("BestScore"));

        for (int i = 1; i <= lastScoreTexts.Length; i++)
        {
            lastScoreTexts[i-1].text = i + ". " + ConvertToFourDigit(PlayerPrefs.GetInt("Score" + i));
        }
    }

    public void ResetScoreboard()
    {
        PlayerPrefs.SetInt("BestScore", 0);
        for (int i = 1; i <= 10; i++)
        {
            PlayerPrefs.SetInt("Score" + i, 0);
        }
        OnEnable();
    }

    string ConvertToFourDigit(int value)
    {
        string result = "";
        for (int i = 1; i <= 4; i++)
        {
            result = value % 10 + result;
            value /= 10;
        }
        return result;
    }
}
