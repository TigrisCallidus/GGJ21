using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeController : MonoBehaviour {

    public bool HideTiles;

    public BaseMaze MazeGenerator;

    public Vector2Int PlayerPosition;

    public int CurrentRopeLength = 0;

    public int MaxRopeLength = 100;



    //[HideInInspector]
    public MazeData Maze;


    public void GenerateMaze() {
        MazeGenerator.GenerateMaze();

        this.Maze = MazeGenerator.Maze;
        PlayerPosition = new Vector2Int(Maze.X / 2, Maze.Y / 2);

        Maze[PlayerPosition.x, PlayerPosition.y].HasPlayer = true;
    }


    public bool CanWalk(WalkDirection direction) {

        bool canWalk = false;
        bool goBack = direction == Maze[PlayerPosition.x, PlayerPosition.y].LastCell;

        switch (direction) {
            case WalkDirection.none:
                break;
            case WalkDirection.up:
                if (Maze.Y > PlayerPosition.y + 1 && !Maze[PlayerPosition.x, PlayerPosition.y + 1].IsWall && (goBack || !Maze[PlayerPosition.x, PlayerPosition.y + 1].HasRope)) {
                    canWalk = true;
                } else {
                    Debug.Log(Maze[PlayerPosition.x, PlayerPosition.y + 1].IsWall + " " + 
                        Maze[PlayerPosition.x, PlayerPosition.y + 1].HasRope + " " + 
                        Maze[PlayerPosition.x, PlayerPosition.y].LastCell + " " + direction);
                }
                break;
            case WalkDirection.right:
                if (Maze.X > PlayerPosition.x + 1 && !Maze[PlayerPosition.x + 1, PlayerPosition.y].IsWall && (goBack || !Maze[PlayerPosition.x + 1, PlayerPosition.y].HasRope)) {
                    canWalk = true;
                }
                break;
            case WalkDirection.down:
                if (PlayerPosition.y > 0 && !Maze[PlayerPosition.x, PlayerPosition.y - 1].IsWall && (goBack || !Maze[PlayerPosition.x, PlayerPosition.y - 1].HasRope)) {
                    canWalk = true;
                }
                break;
            case WalkDirection.left:
                if (PlayerPosition.x > 0 && !Maze[PlayerPosition.x - 1, PlayerPosition.y].IsWall && (goBack || !Maze[PlayerPosition.x - 1, PlayerPosition.y].HasRope)) {
                    canWalk = true;
                }
                break;
            default:
                break;
        }

        

        return canWalk;
    }

    public void Walk(WalkDirection direction) {
        if (!CanWalk(direction)) {
            return;
        }

        Vector2Int newPosition = PlayerPosition;
        WalkDirection lastDirection = WalkDirection.none;

        switch (direction) {
            case WalkDirection.none:
                break;
            case WalkDirection.up:
                newPosition += new Vector2Int(0, 1);
                lastDirection = WalkDirection.down;
                break;
            case WalkDirection.right:
                newPosition += new Vector2Int(1, 0);
                lastDirection = WalkDirection.left;
                break;
            case WalkDirection.down:
                newPosition += new Vector2Int(0, -1);
                lastDirection = WalkDirection.up;
                break;
            case WalkDirection.left:
                newPosition += new Vector2Int(-1, 0);
                lastDirection = WalkDirection.right;
                break;
            default:
                break;
        }

        bool goBack = direction == Maze[PlayerPosition.x, PlayerPosition.y].LastCell;

        Debug.Log(goBack);

        if (!goBack) {
            DropRope();
        }



        //Do walk
        DoMovement(newPosition);


        if (goBack) {
            PickupRope();
        } else {
            Maze[PlayerPosition.x, PlayerPosition.y].LastCell = lastDirection;
        }
    }


    public void PlaceChips() {
        Maze[PlayerPosition.x, PlayerPosition.y].HasChips = true;
    }


    public void DropRope() {
        Maze[PlayerPosition.x, PlayerPosition.y].HasRope = true;
    }

    public void PickupRope() {
        Maze[PlayerPosition.x, PlayerPosition.y].HasRope = false;
        Maze[PlayerPosition.x, PlayerPosition.y].LastCell = WalkDirection.none;

    }

    public void DoMovement(Vector2Int newPosition) {
        Maze[PlayerPosition.x, PlayerPosition.y].HasPlayer = false;
        PlayerPosition = newPosition;
        Maze[PlayerPosition.x, PlayerPosition.y].HasPlayer = true;

    }

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}
