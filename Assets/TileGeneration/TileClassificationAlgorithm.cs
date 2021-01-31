using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileClassificationAlgorithm : MonoBehaviour {

    public virtual TileClassification GetClassification(int x, int y) {

        return new TileClassification();
    }

    protected MazeData maze;

    public virtual void SetMazeData(MazeData newMaze) {
        maze = newMaze;
    }

    protected bool isEmpty(int x, int y) {
        if (x < 0 || y < 0 || x >= maze.X || y >= maze.Y) {
            return true;
        }
        return !maze[x, y].IsWall;

    }
}

[System.Serializable]
public class TileClassification {
    public int Type = 0;
    public Vector3 Rotation = Vector3.zero;
    public bool ShouldSpawn = true;
}
