using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BaseTileType : MonoBehaviour{
    public string Name;
    public int Type;
    public BaseTile[] TilePrefabs;

    public virtual BaseTile GetTile(TileClassification classification) {
        if (TilePrefabs == null || TilePrefabs.Length == 0) {
            return null;
        }
        int rnd = Random.Range(0, TilePrefabs.Length);
        //Debug.Log(this.name);
        BaseTile tile = Instantiate(TilePrefabs[rnd]) as BaseTile;
        tile.transform.rotation = Quaternion.Euler(classification.Rotation);
        return tile;
    }
}

