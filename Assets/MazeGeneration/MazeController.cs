﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeController : MonoBehaviour {

    public bool HideTiles;

    public int AdditionalGaps = 10;

    public int MinimumPlus = 5;

    public BaseMaze MazeGenerator;

    public Vector2Int PlayerPosition;




    public float MovementTime = 3f;


    public static int CurrentRopeLength = 0;

    public static int MaxRopeLength = 100;


    //[HideInInspector]
    public MazeData Maze;


    float lastTime = 0;

    private void Awake() {
        lastTime = Time.time;
    }


    public void GenerateMaze() {
        MazeGenerator.GenerateMaze();
        MazeGenerator.AdditionalGaps = AdditionalGaps;
        MazeGenerator.GenerateGaps();

        this.Maze = MazeGenerator.Maze;
        PlayerPosition = new Vector2Int(Maze.X / 2, Maze.Y / 2);

        Maze[PlayerPosition.x, PlayerPosition.y].HasPlayer = true;
    }


    public bool CanWalk(WalkDirection direction) {

        if (Application.isPlaying && Time.time-lastTime<MovementTime) {
            return false;
        }

        bool canWalk = false;
        bool goBack = direction == Maze[PlayerPosition.x, PlayerPosition.y].LastCell;

        if (CurrentRopeLength<=0 &&!goBack) {
            return false;
        }

        if (goBack) {
            return true;
        }

        switch (direction) {
            case WalkDirection.none:
                break;
            case WalkDirection.up:
                if (Maze.Y > PlayerPosition.y + 1 && !Maze[PlayerPosition.x, PlayerPosition.y + 1].IsWall && (goBack || !Maze[PlayerPosition.x, PlayerPosition.y + 1].HasRope)) {
                    canWalk = true;
                } else {
                    //Debug.Log(Maze[PlayerPosition.x, PlayerPosition.y + 1].IsWall + " " + 
                    //   Maze[PlayerPosition.x, PlayerPosition.y + 1].HasRope + " " + 
                    //    Maze[PlayerPosition.x, PlayerPosition.y].LastCell + " " + direction);
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

    public bool CheckTime() {
        return false;
    }


    public void Walk(WalkDirection direction) {
        if (!CanWalk(direction)) {
            return;
        }

        lastTime = Time.time;

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

        //Debug.Log(goBack);

        if (!goBack) {
            DropRope();
        } else {
            Maze[PlayerPosition.x, PlayerPosition.y].LastCell = WalkDirection.none;
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
        CurrentRopeLength--;
    }

    public void PickupRope() {
        Maze[PlayerPosition.x, PlayerPosition.y].HasRope = false;
        CurrentRopeLength++;
    }

    public void DoMovement(Vector2Int newPosition) {
        Maze[PlayerPosition.x, PlayerPosition.y].HasPlayer = false;
        PlayerPosition = newPosition;
        Maze[PlayerPosition.x, PlayerPosition.y].HasPlayer = true;

    }

    public void GeneratePaths() {
        Queue<Vector2Int> positions = new Queue<Vector2Int>();

        positions.Enqueue(PlayerPosition);
        int count = 0;
        while (positions.Count>0) {
            Vector2Int pos = positions.Dequeue();
            AddNeighbours(positions, pos);
            count++;
            if (count>10000) {
                break;
            }
        }
        CloseMaze();
    }

    void AddNeighbours(Queue<Vector2Int> queue, Vector2Int pos) {

        Vector2Int newPos = pos;

        int newDistance = Maze[newPos.x, newPos.y].Distance + 1;

        if (pos.x>0) {
            newPos = pos - new Vector2Int(1, 0);
            if (!Maze[newPos.x,newPos.y].IsWall && Maze[newPos.x, newPos.y].Distance==0) {
                queue.Enqueue(newPos);
                Maze[newPos.x, newPos.y].Distance = newDistance;
                Maze[newPos.x, newPos.y].Number = -newDistance;
            }
        }

        if (pos.y > 0) {
            newPos = pos - new Vector2Int(0, 1);
            if (!Maze[newPos.x, newPos.y].IsWall && Maze[newPos.x, newPos.y].Distance == 0) {
                queue.Enqueue(newPos);
                Maze[newPos.x, newPos.y].Distance = newDistance;
                Maze[newPos.x, newPos.y].Number = -newDistance;
            }
        }

        if (pos.x < Maze.X-1) {
            newPos = pos + new Vector2Int(1, 0);
            if (!Maze[newPos.x, newPos.y].IsWall && Maze[newPos.x, newPos.y].Distance == 0) {
                queue.Enqueue(newPos);
                Maze[newPos.x, newPos.y].Distance = newDistance;
                Maze[newPos.x, newPos.y].Number = -newDistance;
            }
        }

        if (pos.y < Maze.Y - 1) {
            newPos = pos + new Vector2Int(0, 1);
            if (!Maze[newPos.x, newPos.y].IsWall && Maze[newPos.x, newPos.y].Distance == 0) {
                queue.Enqueue(newPos);
                Maze[newPos.x, newPos.y].Distance = newDistance;
                Maze[newPos.x, newPos.y].Number = -newDistance;
            }
        }

    }

    public void CloseMaze() {

        int min = 100000;

        for (int i = 0; i < Maze.X; i++) {
            if (Maze[i,1].IsWall) {
                Maze[i, 0].Number = 1;
            } else {
                if (Maze[i, 0].Distance < min && Maze[i, 0].Distance>0) {
                    min = Maze[i, 0].Distance;
                }
            }
        }

        for (int i = 0; i < Maze.Y; i++) {
            if (Maze[1, i].IsWall) {
                Maze[0, i].Number = 1;

            } else {
                if (Maze[0, i].Distance < min && Maze[0, i].Distance>0) {
                    min = Maze[0, i].Distance;
                }
            }
        }

        int maxDistance = min + MinimumPlus;


        for (int i = 0; i < Maze.X; i++) {
            if (Maze[i, 0].Distance>maxDistance) {
                Maze[i, 0].Number = 1;
            } else {
                Maze[i, 0].IsExit = true;
            } 
        }

        for (int i = 0; i < Maze.Y; i++) {
            if (Maze[0, i].Distance > maxDistance) {
                Maze[0, i].Number = 1;

            } else {
                Maze[0, i].IsExit = true;
            }
        }

        MaxRopeLength = maxDistance;
    }

    // Update is called once per frame
    void Update() {

    }
}
