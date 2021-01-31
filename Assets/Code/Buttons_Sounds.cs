using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class Buttons_Sounds : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    
    public AudioClip[] buttonSounds;

    private void Awake()
    {
        if (!GetComponent<AudioSource>()) gameObject.AddComponent<AudioSource>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        int rand = Random.Range(0, buttonSounds.Length);
        GetComponent<AudioSource>().PlayOneShot(buttonSounds[rand]);
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        int rand = Random.Range(0, buttonSounds.Length);
        GetComponent<AudioSource>().PlayOneShot(buttonSounds[rand]);
    }
}


