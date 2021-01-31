﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorTile : MonoBehaviour
{
    public SpriteRenderer FloorSprite;

    public SpriteRenderer RopeOverlay;

    public SpriteRenderer CoockieOverlay;

    public void SetFloor(Sprite sprite) {
        FloorSprite.sprite = sprite;
    }

    public void SetRope(Sprite sprite) {
        RopeOverlay.sprite = sprite;
        if (sprite==null) {
            RopeOverlay.gameObject.SetActive(false);
        } else {
            RopeOverlay.gameObject.SetActive(true);
        }
    }

    public void SetCoockie(Sprite sprite) {
        CoockieOverlay.sprite = sprite;
        if (sprite == null) {
            CoockieOverlay.gameObject.SetActive(false);
        } else {
            CoockieOverlay.gameObject.SetActive(true);
        }
    }
}