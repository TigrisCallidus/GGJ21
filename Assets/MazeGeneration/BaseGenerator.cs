using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseGenerator : MonoBehaviour {

    [HideInInspector]
    public MazeData Maze;
    // used to hide the tiles in the custom editor
    public bool HideTiles = false;

    //The waiting time during the animation between steps
    public float WaitTime = 0.5f;

    public int newX = 10;
    public int newY = 10;

    public int X {
        get {
            return Maze.X;
        }
    }

    public int Y {
        get {

            return Maze.Y;
        }
    }

    protected Coroutine mazeGeneration;

    // clearing the maze data
    public virtual void NewMaze() {
        if (mazeGeneration != null) {
            StopCoroutine(mazeGeneration);
        }
        Maze = new MazeData(newX, newY);
    }

    //generating a new maze
    public virtual void GenerateMaze(float waitTime = 0) {

    }
}
