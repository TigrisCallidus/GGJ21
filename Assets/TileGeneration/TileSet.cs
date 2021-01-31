using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileSet : MonoBehaviour {
    public TileClassificationAlgorithm UsedAlgorithm;
    public BaseTileType[] TileTypes;

    MazeData maze;

    public void SetMazeData(MazeData newMaze) {
        maze = newMaze;
        UsedAlgorithm.SetMazeData(maze);
    }

    public BaseTile GetTile(int x, int y) {
        TileClassification classification = UsedAlgorithm.GetClassification(x, y);
        if (!classification.ShouldSpawn) {
            return null;
        }
        BaseTile tile= TileTypes[classification.Type].GetTile(classification);
        
        return tile;
    }
}

