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
    private bool onNoRope = false;

    public RectTransform hungerBarRecTrans;
    public enum HungerStage {full, m, s, xs }
    HungerStage currentHungerStage = HungerStage.full;

    public Animator magen;
    public AudioClip hunger1;
    public AudioClip hunger2;
    public AudioClip hunger3;



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

        if (MazeController.CurrentRopeLength == 0)
        {
            if (!onNoRope)
            {
                onNoRope = true;
                ropeLength.GetComponent<Animator>().SetBool("noRope", true);
            }
        }
        else
        {
            if (onNoRope)
            {
                onNoRope = false;
                ropeLength.GetComponent<Animator>().SetBool("noRope", false);
            }
        }

        float rectX = 100 - ((float)MazeController.CurrentZweifel / (float)MazeController.MaxZweifel * 100f);
        chipsBarRecTrans.sizeDelta = new Vector2(rectX, chipsBarRecTrans.sizeDelta.y);


        float rectYHunger = 100f - (MazeController.FullMeter * 100f);
        hungerBarRecTrans.sizeDelta = new Vector2(hungerBarRecTrans.sizeDelta.x, rectYHunger);

        if (MazeController.FullMeter > 0.5f)
        {
            currentHungerStage = HungerStage.full;
        }


        if (MazeController.FullMeter > 0.3f && MazeController.FullMeter < 0.5f)
        {
            if (currentHungerStage != HungerStage.m)
            {
                magen.SetTrigger("Jitter");
                currentHungerStage = HungerStage.m;
                GetComponent<AudioSource>().PlayOneShot(hunger1);
            }

        }
        else if (MazeController.FullMeter > 0f && MazeController.FullMeter < 0.3f)
        {
            if (currentHungerStage != HungerStage.s)
            {
                magen.SetTrigger("Jitter");
                currentHungerStage = HungerStage.s;
                GetComponent<AudioSource>().PlayOneShot(hunger2);
            }

        }
        else if (MazeController.FullMeter == 0f)
        {
            if (currentHungerStage != HungerStage.xs)
            {
                currentHungerStage = HungerStage.xs;
                GetComponent<AudioSource>().PlayOneShot(hunger3);
            }

        }



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
                ResumeGame();
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
        GetComponent<AudioSource>().PlayOneShot(hunger3);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        currentState = UIState.game;
        inGameMenu.SetActive(false);
    }

    public void RestartGame()
    {
        MazeController.FullMeter = 1f;
        Time.timeScale = 1f;
        SceneManager.LoadScene(1);
    }
}
