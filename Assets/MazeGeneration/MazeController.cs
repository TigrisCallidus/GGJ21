using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeController : MonoBehaviour {

    public bool HideTiles;

    public int AdditionalGaps = 10;

    public int MinimumPlus = 5;

    public int MinRange = 5;


    public Vector2Int PlayerPosition;


    public float AnimationTime = 1f;


    public static int CurrentRopeLength = 0;

    public static int MaxRopeLength = 100;

    public static int MaxZweifel = 10;
    public static int CurrentZweifel = 0;

    public static float FullMeter = 1;

    public BaseMaze MazeGenerator;
    public CharacterAnimation Character;

    public SpriteList[] RopeTypes;
    public Sprite[] ZweifelSprites;

    //[HideInInspector]
    public MazeData Maze;

    float loseFullPerStep = 0.01f;


    float lastTime = 0;

    public static MazeController Instance;


    private void Awake() {

        Debug.Log("Awake");


        lastTime = Time.time;
        bool foundSolution = false;
        while (!foundSolution) {
            GenerateMaze();
            foundSolution=GeneratePaths();
        }

        FullMeter = 1;

        CurrentZweifel = MaxZweifel;
        Instance = this;
        loseFullPerStep = 1.5f / MaxRopeLength;
    }


    public void GenerateMaze() {
        MazeGenerator.GenerateMaze();
        MazeGenerator.AdditionalGaps = AdditionalGaps;
        MazeGenerator.GenerateGaps();

        this.Maze = MazeGenerator.Maze;
        PlayerPosition = new Vector2Int(Maze.X / 2, Maze.Y / 2);
        if (Character != null) {
            Character.transform.position = new Vector3(PlayerPosition.x - Maze.X / 2 + 0.5f, 0.5f, PlayerPosition.y - Maze.Y / 2 + 0.5f);
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


        if (CurrentRopeLength <= 0 && !goBack) {
            canWalk = false;
            Character.NoRope();
        } else {

            if (!canWalk) {
                Character.Go(direction, false, false);
            }
        }

        return canWalk;
    }

    public bool CheckTime() {
        return false;
    }

    Vector2Int lastPosition;

    public void Walk(WalkDirection direction) {
        if (!CanWalk(direction)) {
            return;
        }

        FullMeter -= loseFullPerStep;
        if (FullMeter<=0) {
            UI_Logic.instance.OpenLoosingScreen();
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
            lastPosition = PlayerPosition;
            canDeleteTempRope = true;
        }



        //Do walk

        DoMovement(newPosition);
        Character.Go(direction, true, goBack);



        if (goBack) {
            //PickupRope();
        } else {
            Maze[PlayerPosition.x, PlayerPosition.y].LastCell = lastDirection;
            //Drop half rope
            DropRope(WalkDirection.none, lastDirection, true);
        }


        if (Maze[PlayerPosition.x, PlayerPosition.y].IsExit) {
            ExitFound();
        }
        
    }


    public void PlaceChips() {
        Maze[PlayerPosition.x, PlayerPosition.y].HasChips = true;
    }


    public void DropRope( WalkDirection lastDirection, WalkDirection walkDirection, bool ignoreCount=false) {

        int sprite = 0;
        float rotation = 0;
        if (lastDirection == WalkDirection.none) {
            sprite = 0;
            if (ignoreCount) {
                sprite = 7;
            }
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
        if (!ignoreCount) {
            CurrentRopeLength--;
            //Debug.Log(CurrentRopeLength);
        }
        int rnd = Random.Range(0,RopeTypes[sprite].Sprites.Length);
        Maze[PlayerPosition.x, PlayerPosition.y].Floor?.SetRope(RopeTypes[sprite].Sprites[rnd], rotation);


    }

    public void PickupRope() {
        Maze[PlayerPosition.x, PlayerPosition.y].HasRope = false;
        CurrentRopeLength++;
        //Debug.Log(CurrentRopeLength);
        Maze[PlayerPosition.x, PlayerPosition.y].Floor?.SetRope(null);

        //temp half rope
        if (Maze[PlayerPosition.x, PlayerPosition.y].LastCell!= WalkDirection.none) {
            DropRope(WalkDirection.none, Maze[PlayerPosition.x, PlayerPosition.y].LastCell, true);
        }

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

    public bool GeneratePaths() {

        OpenMaze();

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
        return CloseMaze();
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

    public void OpenMaze() {

        for (int i = 0; i < Maze.X; i++) {
            Maze[i, Maze.Y - 1].Number = 0;
        }

        for (int i = 0; i < Maze.Y; i++) {
            Maze[Maze.X - 1, i].Number = 0;
        }
    }

    public bool CloseMaze() {

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



        for (int i = 0; i < Maze.X; i++) {
            if (Maze[i, Maze.Y - 2].IsWall) {
                Maze[i, Maze.Y - 1].Number = 1;
            } else {
                if (Maze[i, Maze.Y - 1].Distance < min && Maze[i, Maze.Y - 1].Distance > 0) {
                    min = Maze[i, Maze.Y - 1].Distance;
                }
            }
        }

        for (int i = 0; i < Maze.Y; i++) {
            if (Maze[Maze.X - 2, i].IsWall) {
                Maze[Maze.X - 1, i].Number = 1;

            } else {
                if (Maze[Maze.X - 1, i].Distance < min && Maze[Maze.X - 1, i].Distance > 0) {
                    min = Maze[Maze.X - 1, i].Distance;
                }
            }
        }


        int maxDistance = min + MinimumPlus + MinRange;

        min = min + MinimumPlus;

        bool found = false;

        for (int i = 0; i < Maze.X; i++) {
            if (Maze[i, 0].Distance > maxDistance || Maze[i, 0].Distance < min) {
                Maze[i, 0].Number = 1;
            } else {
                Maze[i, 0].IsExit = true;
                found = true;
            }
        }

        for (int i = 0; i < Maze.Y; i++) {
            if (Maze[0, i].Distance > maxDistance || Maze[0, i].Distance < min) {
                Maze[0, i].Number = 1;

            } else {
                Maze[0, i].IsExit = true;
                found = true;
            }
        }

        for (int i = 0; i < Maze.X; i++) {
            if (Maze[i, Maze.Y - 1].Distance > maxDistance || Maze[i, Maze.Y - 1].Distance < min) {
                Maze[i, Maze.Y - 1].Number = 1;
            } else {
                Maze[i, Maze.Y - 1].IsExit = true;
                found = true;
            }
        }

        for (int i = 0; i < Maze.Y; i++) {
            if (Maze[Maze.X - 1, i].Distance > maxDistance || Maze[Maze.X - 1, i].Distance < min) {
                Maze[Maze.X - 1, i].Number = 1;

            } else {
                Maze[Maze.X - 1, i].IsExit = true;
                found = true;
            }
        }

        MaxRopeLength = maxDistance;
        CurrentRopeLength = maxDistance;

        return found;
    }

    public void DropZweifel() {
        if (Application.isPlaying && Time.time - lastTime < AnimationTime) {
            return;
        }
        if (CurrentZweifel<=0) {
            return;
        }
        int rnd = Random.Range(0, ZweifelSprites.Length);

        Character.EatChips();

        FullMeter += 0.5f;
        if (FullMeter>1) {
            FullMeter = 1;
        }

        Maze[PlayerPosition.x, PlayerPosition.y].Floor?.SetChips(ZweifelSprites[rnd]);


        Maze[PlayerPosition.x, PlayerPosition.y].HasChips = true;
        CurrentZweifel--;

        lastTime = Time.time;
        
    }

    public void ExitFound() {
        UI_Logic.instance.Invoke(nameof(UI_Logic.instance.OpenWinningScreen),1.0f);
        //UI_Logic.instance.OpenWinningScreen();
        Debug.Log("You win!");
    }

    static bool canDeleteTempRope = false;

    public static void DeleteTempRope() {
        if (!canDeleteTempRope) {
            return;
        }
        canDeleteTempRope = false;
        Instance.Maze[Instance.lastPosition.x, Instance.lastPosition.y].Floor.SetRope(null);
        Instance.Maze[Instance.lastPosition.x, Instance.lastPosition.y].HasRope = false;
        Instance.PickupRope();
    }

    // Update is called once per frame
    void Update() {


        if (Time.timeScale<=0.5f) {
            return;
        }

        if (Input.GetKey( KeyCode.W) || Input.GetKey( KeyCode.UpArrow)) {
            Walk(WalkDirection.up);
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
            Walk(WalkDirection.down);
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
            Walk(WalkDirection.right);
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
            Walk(WalkDirection.left);
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E)) {
            DropZweifel();
        }
    }
}
