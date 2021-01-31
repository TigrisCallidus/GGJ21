using System.Collections;
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

    public void SetRope(Sprite sprite, float rotation=0) {
        RopeOverlay.sprite = sprite;
        if (sprite==null) {
            RopeOverlay.gameObject.SetActive(false);
        } else {
            RopeOverlay.gameObject.SetActive(true);
            //if (rotation>0) {
            RopeOverlay.transform.localRotation = Quaternion.Euler(65, 0, rotation);
            //}
        }
    }

    public void SetChips(Sprite sprite) {
        CoockieOverlay.sprite = sprite;
        if (sprite == null) {
            CoockieOverlay.gameObject.SetActive(false);
        } else {
            CoockieOverlay.gameObject.SetActive(true);
        }
    }
}
