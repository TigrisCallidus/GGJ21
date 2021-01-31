using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;


public class ReturnToRealiy : MonoBehaviour, IPointerClickHandler
{

    public string areUShure = "Are you really sure?";
    public string sadSmiles = "<sprite index=15> <sprite index=15> <sprite index=15>";
    public Sprite selectedButton;

    public AudioClip[] buttonSounds;
    public TextMeshProUGUI text;

    int attempts = 0;

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        int rand = Random.Range(0, buttonSounds.Length);
        GetComponent<AudioSource>().PlayOneShot(buttonSounds[rand]);
        switch (attempts)
        {
            case 0:
                text.text = areUShure;
                attempts++;
                break;
            case 1:
                text.text = sadSmiles;
                GetComponent<Image>().sprite = selectedButton;
                Invoke("Quit", 1f);
                break;

            default:
                break;
        }

    }

    public void Quit()
    {
        Application.Quit();
    }
}
