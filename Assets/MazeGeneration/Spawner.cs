using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.AI;

public class Spawner : MonoBehaviour {

    public Camera Camera;
    public BaseMaze Maze;
    public GameObject WallPrefab;

    //public NavMeshSurface Surface;

    List<GameObject> SpawnedObjects = new List<GameObject>();

    // Start is called before the first frame update
    void Start() {

    }

    private void Awake() {
        SpawnWalls();
        //Surface.BuildNavMesh();
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            SpawnWalls();
        }
    }

    public void SpawnWalls() {
        if (SpawnedObjects!=null) {
            foreach (var item in SpawnedObjects) {
                Destroy(item);
            }
        }
        SpawnedObjects = new List<GameObject>();
        Camera.orthographicSize = 0.5f * Mathf.Max(Maze.X, Maze.Y);
        Vector3 StartPos = new Vector3(-0.5f*Maze.X+0.5f, 0.5f, 0.5f*Maze.Y-0.5f);
        Vector3 spawnPosition;
        for (int i = 0; i < Maze.X; i++) {
            for (int j = 0; j < Maze.Y; j++) {
                if (Maze.Maze[i,j].Number>0) {
                    spawnPosition = StartPos + new Vector3(i, 0, -j);
                    GameObject wall = Instantiate(WallPrefab, spawnPosition, Quaternion.identity, this.transform);
                    SpawnedObjects.Add(wall);
                }
            }
        }
    }
}
