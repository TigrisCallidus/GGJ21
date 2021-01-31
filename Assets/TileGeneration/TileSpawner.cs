using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TileSpawner : MonoBehaviour {

    //public Camera Camera;
    //public BaseGenerator Maze;

    public MazeController Controller;


    public TileSet CurrentTiles;

    public FloorTile FloorPrefab;

    public CharacterAnimation PlayerPrefab;
    //public GameObject PlayerPrefab;

    List<GameObject> SpawnedObjects = new List<GameObject>();


    MazeData Maze;

    // Start is called before the first frame update
    void Start() {
        SpawnTiles();
    }


    /*
    private void Awake() {
        SpawnTiles();
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            SpawnTiles();
        }
    }

    */

    public void SpawnTiles() {

        Maze = Controller.Maze;
        if (SpawnedObjects!=null) {
            foreach (var item in SpawnedObjects) {
                Destroy(item);
            }
        }

        CurrentTiles.SetMazeData(Maze);

        SpawnedObjects = new List<GameObject>();
        //Camera.orthographicSize = 0.5f * Mathf.Max(Maze.X, Maze.Y);

        Vector3 StartPos = new Vector3(-0.5f*Maze.X+0.5f, 0.5f, -0.5f*Maze.Y+0.5f);
        Vector3 spawnPosition;
        for (int i = 0; i < Maze.X; i++) {
            for (int j = 0; j < Maze.Y; j++) {
                BaseTile tile = CurrentTiles.GetTile(i, j);
                spawnPosition = StartPos + new Vector3(i, 0, j);
                if (tile != null) {
                    tile.transform.position = spawnPosition;
                    tile.transform.parent = this.transform;
                    //GameObject wall = Instantiate(tile.gameObject, spawnPosition, Quaternion.identity, this.transform);
                    SpawnedObjects.Add(tile.gameObject);
                    Maze[i, j].Wall = tile;
                } else {
                    FloorTile floorTile = Instantiate(FloorPrefab);
                    floorTile.transform.position = spawnPosition;
                    floorTile.transform.parent = this.transform;
                    SpawnedObjects.Add(floorTile.gameObject);
                    Maze[i, j].Floor = floorTile;
                    if (Maze[i,j].IsExit) {
                        floorTile.ActivateExit();
                    }
                }
            }
        }
    }
}


