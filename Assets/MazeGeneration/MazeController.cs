using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeController : MonoBehaviour {

    public bool HideTiles;

    public int AdditionalGaps = 10;

    public int MinimumPlus = 5;


    public Vector2Int PlayerPosition;


    public float AnimationTime = 1f;


    public static int CurrentRopeLength = 0;

    public static int MaxRopeLength = 100;

    public static int MaxZweifel = 10;
    public static int CurrentZweifel = 0;

    public BaseMaze MazeGenerator;
    public CharacterAnimation Character;

    public SpriteList[] RopeTypes;
    public Sprite[] ZweifelSprites;

    //[HideInInspector]
    public MazeData Maze;


    float lastTime = 0;

    private void Awake() {
        lastTime = Time.time;
        GenerateMaze();
        GeneratePaths();
        CurrentZweifel = MaxZweifel;
    }


    public void GenerateMaze() {
        MazeGenerator.GenerateMaze();
        MazeGenerator.AdditionalGaps = AdditionalGaps;
        MazeGenerator.GenerateGaps();

        this.Maze = MazeGenerator.Maze;
        PlayerPosition = new Vector2Int(Maze.X / 2, Maze.Y / 2);
        if (Character != null) {
            Character.transform.position = new Vector3(PlayerPosition.x - Maze.X / 2 + 0.5f, 0.5f, PlayerPosition.y - Maze.Y / 2 + 1);
        }

        Maze[PlayerPosition.x, PlayerPosition.y].HasPlayer = true;
    }


    public bool CanWalk(WalkDirection direction) {

        if (Application.isPlaying && Time.time - lastTime < AnimationTime) {
            return false;
        }

        bool canWalk = false;
        bool goBack = direction == Maze[PlayerPosition.x, PlayerPosition.y].LastCell;



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
        Character.Go(direction, false, false);
        if (CurrentRopeLength <= 0 && !goBack) {
            canWalk= false;
        }

        if (!canWalk) {

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
            DropRope(Maze[PlayerPosition.x, PlayerPosition.y].LastCell, direction);
        } else {
            Maze[PlayerPosition.x, PlayerPosition.y].LastCell = WalkDirection.none;
        }



        //Do walk

        DoMovement(newPosition);
        Character.Go(direction, true, goBack);


        if (goBack) {
            PickupRope();
        } else {
            Maze[PlayerPosition.x, PlayerPosition.y].LastCell = lastDirection;
        }


        if (Maze[PlayerPosition.x, PlayerPosition.y].IsExit) {
            ExitFound();
        }
        
    }


    public void PlaceChips() {
        Maze[PlayerPosition.x, PlayerPosition.y].HasChips = true;
    }


    public void DropRope( WalkDirection lastDirection, WalkDirection walkDirection) {

        int sprite = 0;
        float rotation = 0;
        if (lastDirection == WalkDirection.none) {
            Debug.Log(walkDirection);
            sprite = 0;
            if (walkDirection == WalkDirection.right) {
                rotation = 0;
            } else if (walkDirection == WalkDirection.up) {
                rotation = 90;
            } else if (walkDirection == WalkDirection.down) {
                rotation = 270;
            } else if (walkDirection == WalkDirection.left) {
                rotation = 180;
            }
        } else if ((lastDirection== WalkDirection.left || lastDirection== WalkDirection.right)
            && (walkDirection == WalkDirection.left || walkDirection == WalkDirection.right))
            {
            sprite = 1;
        } else if ((lastDirection == WalkDirection.up || lastDirection == WalkDirection.down)
            && (walkDirection == WalkDirection.up || walkDirection == WalkDirection.down)) {
            sprite = 6;
        } else if ((lastDirection == WalkDirection.left && walkDirection == WalkDirection.up)
             || (lastDirection == WalkDirection.up && walkDirection == WalkDirection.left)) {
            sprite = 2;
        } else if ((lastDirection == WalkDirection.right && walkDirection == WalkDirection.up)
              || (lastDirection == WalkDirection.up && walkDirection == WalkDirection.right)) {
            sprite = 3;
        } else if ((lastDirection == WalkDirection.left && walkDirection == WalkDirection.down)
              || (lastDirection == WalkDirection.down && walkDirection == WalkDirection.left)) {
            sprite = 5;
        } else if ((lastDirection == WalkDirection.right && walkDirection == WalkDirection.down)
               || (lastDirection == WalkDirection.down && walkDirection == WalkDirection.right)) {
            sprite = 4;
        }

        Maze[PlayerPosition.x, PlayerPosition.y].HasRope = true;
        CurrentRopeLength--;
        int rnd = Random.Range(0,RopeTypes[sprite].Sprites.Length);
        Maze[PlayerPosition.x, PlayerPosition.y].Floor?.SetRope(RopeTypes[sprite].Sprites[rnd], rotation);


    }

    public void PickupRope() {
        Maze[PlayerPosition.x, PlayerPosition.y].HasRope = false;
        CurrentRopeLength++;
        Maze[PlayerPosition.x, PlayerPosition.y].Floor?.SetRope(null);

    }

    public void DoMovement(Vector2Int newPosition) {
        Maze[PlayerPosition.x, PlayerPosition.y].HasPlayer = false;
        PlayerPosition = newPosition;
        //TODO link real movement
        /*
        if (Character != null) {
            Character.transform.position = new Vector3(PlayerPosition.x - Maze.X / 2 + 0.5f, 0.5f, PlayerPosition.y - Maze.Y / 2 + 1);
        }
        */
        Maze[PlayerPosition.x, PlayerPosition.y].HasPlayer = true;

    }

    public void GeneratePaths() {
        Queue<Vector2Int> positions = new Queue<Vector2Int>();

        positions.Enqueue(PlayerPosition);
        int count = 0;
        while (positions.Count > 0) {
            Vector2Int pos = positions.Dequeue();
            AddNeighbours(positions, pos);
            count++;
            if (count > 10000) {
                break;
            }
        }
        CloseMaze();
    }

    void AddNeighbours(Queue<Vector2Int> queue, Vector2Int pos) {

        Vector2Int newPos = pos;

        int newDistance = Maze[newPos.x, newPos.y].Distance + 1;

        if (pos.x > 0) {
            newPos = pos - new Vector2Int(1, 0);
            if (!Maze[newPos.x, newPos.y].IsWall && Maze[newPos.x, newPos.y].Distance == 0) {
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

        if (pos.x < Maze.X - 1) {
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
            if (Maze[i, 1].IsWall) {
                Maze[i, 0].Number = 1;
            } else {
                if (Maze[i, 0].Distance < min && Maze[i, 0].Distance > 0) {
                    min = Maze[i, 0].Distance;
                }
            }
        }

        for (int i = 0; i < Maze.Y; i++) {
            if (Maze[1, i].IsWall) {
                Maze[0, i].Number = 1;

            } else {
                if (Maze[0, i].Distance < min && Maze[0, i].Distance > 0) {
                    min = Maze[0, i].Distance;
                }
            }
        }

        int maxDistance = min + MinimumPlus;


        for (int i = 0; i < Maze.X; i++) {
            if (Maze[i, 0].Distance > maxDistance) {
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
        CurrentRopeLength = maxDistance;
    }

    public void DropZweifel() {
        if (Application.isPlaying && Time.time - lastTime < AnimationTime) {
            return;
        }
        if (CurrentZweifel<=0) {
            return;
        }
        int rnd = Random.Range(0, ZweifelSprites.Length);

        //TODO eating animation
        Character.EatChips();

        Maze[PlayerPosition.x, PlayerPosition.y].Floor?.SetChips(ZweifelSprites[rnd]);


        Maze[PlayerPosition.x, PlayerPosition.y].HasChips = true;
        CurrentZweifel--;

        lastTime = Time.time;
        
    }

    public void ExitFound() {
        Debug.Log("You win!");
    }

    // Update is called once per frame
    void Update() {


        if (Time.timeScale<=0.5f) {
            return;
        }

        if (Input.GetKeyDown( KeyCode.W) || Input.GetKey ( KeyCode.UpArrow)) {
            Walk(WalkDirection.up);
        }

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
            Walk(WalkDirection.down);
        }

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
            Walk(WalkDirection.right);
        }

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
            Walk(WalkDirection.left);
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKey(KeyCode.E)) {
            DropZweifel();
        }
    }
}
