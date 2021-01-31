using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UI_Logic : MonoBehaviour
{

    public static UI_Logic instance;

    public enum UIState {game, pause, endOfGame}
    UIState currentState = UIState.game;


    public RectTransform chipsBarRecTrans;
    public TextMeshProUGUI ropeLength;

    public GameObject inGameMenu;
    public GameObject winMenu;
    public GameObject loseMenu;

    private void Awake()
    {
        instance = this;
    }


    private void Update()
    {
        ropeLength.text = MazeController.CurrentRopeLength + " m";

        float rectX = 100 - ((float)MazeController.CurrentZweifel / (float)MazeController.MaxZweifel * 100f);
        chipsBarRecTrans.sizeDelta = new Vector2(rectX, chipsBarRecTrans.sizeDelta.y);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == UIState.game)
            {
                Time.timeScale = 0.1f;
                currentState = UIState.pause;
                inGameMenu.SetActive(true);
            }
            else if (currentState == UIState.pause)
            {
                Time.timeScale = 1f;
                currentState = UIState.game;
                inGameMenu.SetActive(false);
            }
        }
    }

    public void OpenWinningScreen()
    {
        Time.timeScale = 0.1f;
        currentState = UIState.endOfGame;
        winMenu.SetActive(true);
    }

    public void OpenLoosingScreen()
    {
        Time.timeScale = 0.1f;
        currentState = UIState.endOfGame;
        loseMenu.SetActive(true);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

}
