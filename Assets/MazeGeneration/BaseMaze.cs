using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BaseMaze : BaseGenerator {

    public MazeAlgorithm Algorithm;
    public float placementThreshold = 0.1f;
    public bool Advanced = false;
    public int AdditionalGaps = 0;
    public GapType GapType;
    public string FileName;

    // needed to store the files
    const string path = "Assets/Resources/";


    // for Prim and Kruskal
    int xTiles = 0;
    int yTiles = 0;
    List<MazeTile> notVisited;
    List<MazeTile> visited;
    MazeTile[,] mazeTiles;
    List<TileConnection> connections;
    const int maxCost = 10000;

    // needed for Kruskal
    List<List<MazeTile>> tileGroups;
    int groupCount = 0;


    public override void GenerateMaze(float waitTime = 0) {
        if (mazeGeneration!=null) {
            StopCoroutine(mazeGeneration);
        }
        switch (Algorithm) {
            case MazeAlgorithm.simple:
                GenerateSimpleMaze(waitTime);
                break;
            case MazeAlgorithm.subdivision:
                GenerateSubDivision(waitTime);
                break;
            case MazeAlgorithm.prim:
                GeneratePrim(waitTime);
                break;
            case MazeAlgorithm.kruskal:
                GenerateKruskal(waitTime);
                break;
            default:
                break;
        }
    }

    public void SaveFile() {
        string json = JsonUtility.ToJson(Maze);
        File.WriteAllText(Path.Combine(path,FileName), json);
    }

    public void LoadFile() {
        string json= File.ReadAllText( Path.Combine(path, FileName));
        Maze = JsonUtility.FromJson<MazeData>(json);
    }

    // Simple algorithm using pillars (in each 4th cell) 
    // and then add a random wall in one direction from there
    public void GenerateSimpleMaze(float waitTime=0) {
        mazeGeneration = StartCoroutine(generateSimpleMaze(waitTime));
    }

    
    IEnumerator generateSimpleMaze(float waitTime = 0) {

        //Making sure it is odd (prevents endless loops in the advanced part)
        int tileSize = 2;

        if (newX % tileSize !=1) {
            newX = newX + tileSize - newX % tileSize +1;
        }
        if (newY % tileSize != 1) {
            newY = newY + tileSize - newY % tileSize +1;
        }

        NewMaze();

        int xMax = Maze.X-1;
        int yMax = Maze.Y-1;

        float rnd;

        float half = (1.0f - placementThreshold) / 2;
        float quarter = half / 2;
        float threeQuarter = half + quarter;

        float threshold = 1 - placementThreshold;

        for (int i = 0; i <= xMax; i++) {
            for (int j = 0; j <= yMax; j++) {
                //Walls around
                if (i == 0 || j == 0 || i == xMax || j == yMax) {
                    Maze[i, j].Number = 1;
                }
            }
        }
        bool shouldBreak = false;
        for (int i = 1; i <= xMax - 1; i++) {
            if (shouldBreak) {
                break;
            }
            for (int j = 1; j <= yMax - 1; j++) {
                if (shouldBreak) {
                    break;
                }
                //Pillars
                if (i % 2 == 0 && j % 2 == 0) {
                    rnd = Random.value;
                    // Small chance of not having a pillar
                    if (rnd < threshold) {

                        Maze[i, j].Number = 1;
                        bool found = false;

                        int loopcount = 0;
                        while (!found) {
                            found = false;

                            loopcount++;
                            if (loopcount>100) {
                                Debug.Log("too long loop " + i + " " + j + " try an odd number for the maze!");
                                shouldBreak = true;
                                break;
                            }

                            // Waiting (for animation)                            
                            if (waitTime>0) {
                                yield return new WaitForSeconds(waitTime);
                            }                            

                            if (rnd < half) {
                                if (rnd < quarter) {
                                    //North
                                    if (Maze[i, j + 1].Number==0) {
                                        found=true;
                                    }
                                    Maze[i, j + 1].Number = 1;
                                } else {
                                    //East
                                    if (Maze[i + 1, j].Number == 0) {
                                        found = true;
                                    }
                                    Maze[i + 1, j].Number = 1;
                                }
                            } else {
                                if (rnd < threeQuarter) {
                                    // South
                                    if (Maze[i, j - 1].Number == 0) {
                                        found = true;
                                    }
                                    Maze[i, j - 1].Number = 1;
                                } else {
                                    // West
                                    if (Maze[i - 1, j].Number == 0) {
                                        found = true;
                                    }
                                    Maze[i - 1, j].Number = 1;
                                }

                            }
                            if (!Advanced) {
                                found = true;
                            } else {
                                rnd = Random.value;
                            }
                        }

                        // waiting to show animation
                        if (waitTime > 0) {
                            yield return new WaitForSeconds(waitTime);
                        }
                    }
                }
            }
        }
        yield return new WaitForSeconds(0);
    }

    // Algorithm which devides the room into smaller rooms by placing a horizontal or vertical wall somewhere in it.
    // There is always 1 door (open space) connecting the 2 newly formed rooms.
    public void GenerateSubDivision(float waitTime = 0) {
        mazeGeneration = StartCoroutine(generateSubDivision(waitTime));
    }

    IEnumerator generateSubDivision(float waitTime = 0) {

        NewMaze();

        List<MazeRegion> regions = new List<MazeRegion>();

        //the whole place is 1 region in the beginning
        MazeRegion currentRegion = new MazeRegion {
            XStart = 0,
            YStart = 0,
            XEnd = Maze.X,
            YEnd = Maze.Y,
            forbiddenX1 = -1,
            forbiddenY1 = -1,
            forbiddenX2 = -1,
            forbiddenY2 = -1,
            takenCount = 0
      
        };

        regions.Add(currentRegion);

        MazeRegion newRegion;
        int rnd, gap;
        int count=0;
        int maxCount=5000;

        //repeat while there are still regions, which can be split.
        while (regions.Count>0 && count<maxCount) {
            rnd = Random.Range(0, regions.Count);
            //taking a new random region to split
            currentRegion = regions[rnd];

            //make sure ther is no endlessloop
            regions.Remove(currentRegion);

            //making a copy of the region for the case we need to add it again
            newRegion = currentRegion;
            newRegion.takenCount += 1;
            count++;

            rnd = Random.Range(0, 2);

            if (rnd==0) {
                // Split in x direction
                rnd = Random.Range(currentRegion.XStart + 1, currentRegion.XEnd-1);
                //advanced algorithm only places walls in even spaces
                if (Advanced) {
                    rnd= Random.Range(0, (currentRegion.XEnd - currentRegion.XStart)/2);
                    rnd = rnd*2 + currentRegion.XStart + 1;
                }
                //do not place a wall where there is a gap in the neighbour room
                if (rnd==currentRegion.forbiddenX1 || rnd==currentRegion.forbiddenX2) {
                    if (newRegion.takenCount<=3) {
                        regions.Add(newRegion);
                    }
                    continue;
                }
                gap = Random.Range(currentRegion.YStart, currentRegion.YEnd);
                //advanced algorithm only places gaps in odd spaces
                if (Advanced) {
                    gap = Random.Range(0, (currentRegion.YEnd - currentRegion.YStart) / 2);
                    gap = gap * 2+ currentRegion.YStart;
                }
                for (int i = currentRegion.YStart; i < currentRegion.YEnd; i++) {
                    if (i!= gap) {
                        Maze[rnd, i].Number = count;
                    }
                }
                // if region on the left of the split is big enough add it to the regions
                if (rnd-currentRegion.XStart>2) {
                    newRegion = new MazeRegion {
                        XStart=currentRegion.XStart,
                        YStart=currentRegion.YStart,
                        XEnd=rnd,
                        YEnd=currentRegion.YEnd,
                        forbiddenX1 = currentRegion.forbiddenX1,
                        forbiddenY1 = currentRegion.forbiddenY1,
                        forbiddenX2 = currentRegion.forbiddenX2,
                        forbiddenY2 = gap
                    };
                    regions.Add(newRegion);
                }
                // if region on the right of the split is big enough add it to the regions
                if (currentRegion.XEnd-rnd>3) {
                    newRegion = new MazeRegion {
                        XStart = rnd+1,
                        YStart = currentRegion.YStart,
                        XEnd = currentRegion.XEnd,
                        YEnd = currentRegion.YEnd,
                        forbiddenX1 = currentRegion.forbiddenX1,
                        forbiddenY1 = gap,
                        forbiddenX2 = currentRegion.forbiddenX2,
                        forbiddenY2 = currentRegion.forbiddenY2
                    };
                    regions.Add(newRegion);
                }

            } else {
                // split in y direction
                rnd = Random.Range(currentRegion.YStart + 1, currentRegion.YEnd - 1);
                //advanced algorithm only places walls in even spaces
                if (Advanced) {
                    rnd = Random.Range(0, (currentRegion.YEnd - currentRegion.YStart) / 2);
                    rnd = rnd*2 + currentRegion.YStart + 1;
                }
                //do not place a wall where there is a gap in the neighbour room
                if (rnd == currentRegion.forbiddenY1 || rnd == currentRegion.forbiddenY2) {
                    if (newRegion.takenCount <= 3) {
                        regions.Add(newRegion);
                    }
                    continue;
                }
                gap = Random.Range(currentRegion.XStart, currentRegion.XEnd);
                //advanced algorithm only places gaps in odd spaces
                if (Advanced) {
                    gap = Random.Range(0, (currentRegion.XEnd - currentRegion.XStart) / 2); ;
                    gap = gap * 2 + currentRegion.XStart;
                }
                for (int i = currentRegion.XStart; i < currentRegion.XEnd; i++) {
                    if (i != gap) {
                        Maze[i, rnd].Number = count;
                    }
                }
                // if region on the top of the split is big enough add it to the regions
                if (rnd - currentRegion.YStart > 2) {
                    newRegion = new MazeRegion {
                        XStart = currentRegion.XStart,
                        YStart = currentRegion.YStart,
                        XEnd = currentRegion.XEnd,
                        YEnd = rnd,
                        forbiddenX1 = currentRegion.forbiddenX1,
                        forbiddenY1 = currentRegion.forbiddenY1,
                        forbiddenX2 = gap,
                        forbiddenY2 = currentRegion.forbiddenY2
                    };
                    regions.Add(newRegion);
                }

                // if region below the split is big enough add it to the regions
                if (currentRegion.YEnd - rnd > 3) {
                    newRegion = new MazeRegion {
                        XStart = currentRegion.XStart,
                        YStart = rnd + 1,
                        XEnd = currentRegion.XEnd,
                        YEnd = currentRegion.YEnd,
                        forbiddenX1 = gap,
                        forbiddenY1 = currentRegion.forbiddenY1,
                        forbiddenX2 = currentRegion.forbiddenX2,
                        forbiddenY2 = currentRegion.forbiddenY2
                    };
                    regions.Add(newRegion);
                }
            }

            if (waitTime > 0) {
                yield return new WaitForSeconds(waitTime);
            }
        }
        yield return new WaitForSeconds(0);
    }

    // Prim algorithm (spanning tree) to generate a maze
    // Start with a random Tile and then adding random neighbouring tiles which are not yet inside the maze to it.
    // There is always only 1 connection between the maze and the new tile.
    public void GeneratePrim(float waitTime=0) {

        // We need tiles (consisting of several cells) for this algorithm
        PrepareTiles();
        // make sure the cells show the starting point.
        TilesToMaze();
        mazeGeneration = StartCoroutine(generatePrim(waitTime));
    }

    public IEnumerator generatePrim(float waitTime = 0) {

        // used for the animation
        if (waitTime > 0) {
            yield return new WaitForSeconds(waitTime);
        }
        // in the beginning there are no connections (no reachable tiles)
        connections = new List<TileConnection>();

        // choosing a random start tile to be in the maze
        MazeTile tile = notVisited[Random.Range(0, notVisited.Count)];

        // Visit the tile = adding the surrounding reachable tiles and remove it from the notVisited
        VisitTile(tile);

        // while there are still tiles which are not visited (in the maze) repeat
        while (notVisited.Count>0) {
            //choose a random reachable tile
            TileConnection connection = connections[Random.Range(0, connections.Count)];
            connections.Remove(connection);
            // since we store all connections not the not visited tiles, it can be that the tile is already visited.
            if (connection.First.Visited && connection.Second.Visited) {
                continue;
            } else {
                // make sure there is no wall where the connection is
                OpenConnection(connection);
                // visit the new added tile.
                if (connection.First.Visited) {
                    VisitTile(connection.Second);
                } else {
                    VisitTile(connection.First);
                }
                // used for the animation
                if (waitTime > 0) {
                    TilesToMaze(false);
                    yield return new WaitForSeconds(waitTime);
                }
            }
        }
        TilesToMaze(false);
    }


    // Kruskal algorithm (spanning tree) to generate a maze
    // Connecting random neighbouring tiles (anywhere) together until all tiles are connected.
    // This algorithm needs to store groups (of connected) tiles
    public void GenerateKruskal(float waitTime = 0) {
        // We need tiles (consisting of several cells) for this algorithm
        PrepareTiles();
        // make sure the cells show the starting point.
        TilesToMaze();
        mazeGeneration = StartCoroutine(generateKruskal(waitTime));

    }

    public IEnumerator generateKruskal(float waitTime = 0) {
        //for the animation
        if (waitTime > 0) {
            yield return new WaitForSeconds(waitTime);
        }

        // Preparing empty groups
        tileGroups = new List<List<MazeTile>>();
        List<MazeTile> empty = new List<MazeTile>();
        tileGroups.Add(empty);
        groupCount = 0;

        // Choosing random connection they will form group 1
        TileConnection connection = connections[Random.Range(0,connections.Count)];

        //Elements of group 1 are considered visited
        FastVisit(connection.First);
        FastVisit(connection.Second);

        connections.Remove(connection);
        UseConnection(connection);
        OpenConnection(connection);

        //prevent endless loops
        int count = 0;      

        // loop until all elements are visited (in group 1)
        while (notVisited.Count > 0 && count<100000) {
            count++;
            // Get random connection
            int rnd = Random.Range(0, connections.Count);
            connection = connections[rnd];
            connections.Remove(connection);

            //Debugging
            /*if (connections.Count == 0) {
                Debug.Log("Not visited total: " +notVisited.Count);
                for (int i = 0; i < notVisited.Count; i++) {
                    Debug.Log("Did not visit: " + notVisited[0].PosX + " " + notVisited[0].PosY);
                }
            }*/

            // Check if they are not already in the same group
            if (connection.First.Group > 0 && connection.First.Group== connection.Second.Group) {
                continue;
            } else {
                // Open the walls and join the 2 elements into a group
                OpenConnection(connection);
                UseConnection(connection);
                //for the animation
                if (waitTime>0) {
                    TilesToMaze(false);
                    yield return new WaitForSeconds(waitTime);
                }
            }
        }
        TilesToMaze(false);
    }

    // preparing tiles (which consists of several cells), which will be connected by kruskal or prim
    public void PrepareTiles() {
        // 3x3 tiles
        int tileSize = 3;

        if (Advanced) {
            // tiles become 2x2
            tileSize = 2;
        }

        // make sure the maze can be divided by the tile size
        if (newX % tileSize != 0) {
            newX = newX + tileSize - newX % tileSize;
        }
        if (newY % tileSize != 0) {
            newY = newY + tileSize - newY % tileSize;
        }
        xTiles = newX / tileSize;
        yTiles = newY / tileSize;

        NewMaze();

        //data structure to store all tiles, connections between tiles and which ones are visited and which ones not.
        notVisited = new List<MazeTile>();
        visited = new List<MazeTile>();
        mazeTiles = new MazeTile[xTiles, yTiles];
        connections = new List<TileConnection>();

        for (int i = 0; i < xTiles; i++) {
            for (int j = 0; j < yTiles; j++) {
                //creating new tile
                MazeTile tile = new MazeTile {
                    PosX = i,
                    PosY = j
                };                
                mazeTiles[i, j] = tile;
                // newly created tiles are not visited
                notVisited.Add(tile);
                if (i > 0) {
                    // connecting a tile with the left neighbour
                    tile.Left = mazeTiles[i - 1, j];
                    //also connecting the left tile with this
                    mazeTiles[i - 1, j].Right = tile;

                    //adding connection for Kruskal (there you start with all connections)
                    TileConnection connection = new TileConnection {
                        First = mazeTiles[i - 1, j],
                        Second = tile,
                        Horizontal = true,

                    };
                    connections.Add(connection);
                }
                if (j > 0) {
                    // connecting a tile with the upper neighbour
                    tile.Up = mazeTiles[i, j - 1];
                    //also connecting the upper tile with this
                    mazeTiles[i, j - 1].Down = tile;

                    //adding connection for Kruskal (there you start with all connections)
                    TileConnection connection = new TileConnection {
                        First = mazeTiles[i, j - 1],
                        Second = tile,
                        Horizontal = false,
                        //Cost = Random.Range(1, maxCost)
                    };
                    connections.Add(connection);
                }
            }
        }
    }

    public void VisitTile(MazeTile tile) {

        tile.Visited = true;
        //put tile from not visited to visited
        notVisited.Remove(tile);
        visited.Add(tile);

        // if tile has a left not visited neighbour add this connection  
        if (tile.PosX > 0 && !mazeTiles[tile.PosX - 1, tile.PosY].Visited) {
            TileConnection connection = new TileConnection {
                First = mazeTiles[tile.PosX - 1, tile.PosY],
                Second = tile,
                Horizontal = true,
                //Cost = Random.Range(1, maxCost)
            };
            connections.Add(connection);
        }

        // if tile has an upper not visited neighbour add this connection  
        if (tile.PosY > 0 && !mazeTiles[tile.PosX, tile.PosY - 1].Visited) {
            TileConnection connection = new TileConnection {
                First = mazeTiles[tile.PosX, tile.PosY - 1],
                Second = tile,
                Horizontal = false,
                //Cost = Random.Range(1, maxCost)
            };
            connections.Add(connection);
        }

        // if tile has a right not visited neighbour add this connection  
        if (tile.PosX < xTiles - 1 && !mazeTiles[tile.PosX + 1, tile.PosY].Visited) {
            TileConnection connection = new TileConnection {
                First = tile,
                Second = mazeTiles[tile.PosX + 1, tile.PosY],
                Horizontal = true,
                //Cost = Random.Range(1, maxCost)
            };
            connections.Add(connection);
        }

        // if tile has a down not visited neighbour add this connection  
        if (tile.PosY < yTiles - 1 && !mazeTiles[tile.PosX, tile.PosY + 1].Visited) {
            TileConnection connection = new TileConnection {
                First = tile,
                Second = mazeTiles[tile.PosX, tile.PosY + 1],
                Horizontal = false,
                //Cost = Random.Range(1, maxCost)
            };
            connections.Add(connection);
        }
    }


    //Set the cells open to each other. (To not draw walls there)
    public void OpenConnection(TileConnection connection) {
        if (connection.Horizontal) {
            connection.First.OpenRight = true;
            connection.Second.OpenLeft = true;
        } else {
            connection.First.OpenDown = true;
            connection.Second.OpenUp = true;
        }
    }


    public void FastVisit(MazeTile tile) {
        //We only need this in kruskal to keep track if all tiles are connected together.
        tile.Visited = true;
        notVisited.Remove(tile);
    }

    //connect 2 cells together
    public void UseConnection(TileConnection connection) {
        // if both are in no group form a new group
        if (connection.First.Group==0 && connection.Second.Group==0) {
            groupCount++;
            connection.First.Group = groupCount;
            connection.Second.Group = groupCount;
            List<MazeTile> group = new List<MazeTile>();
            group.Add(connection.First);
            group.Add(connection.Second);
            tileGroups.Add(group);
        } else {
            // if only the second is in a group, the first joins their group
            if (connection.First.Group==0) {
                int group = connection.Second.Group;
                tileGroups[group].Add(connection.First);
                connection.First.Group = group;
                if (group==1) {
                    FastVisit(connection.First);
                }
             // if only the first is in a group, the second joins their group
            } else if (connection.Second.Group==0) {
                int group = connection.First.Group;
                tileGroups[group].Add(connection.Second);
                connection.Second.Group = group;
                if (group == 1) {
                    FastVisit(connection.Second);
                }
            // if they are in different groups connect the groups the bigger number joins the smaller one
            } else if (connection.First.Group<connection.Second.Group) {
                CombineGroups(connection.First.Group, connection.Second.Group);
            } else {
                CombineGroups(connection.Second.Group, connection.First.Group);
            }
        }
    }

    public void CombineGroups(int first, int second) {
        // the second(bigger) group joins the first group and get their number
        int count = tileGroups[second].Count;
        for (int i = 0; i < count; i++) {
            MazeTile tile = tileGroups[second][0];
            tileGroups[second].Remove(tile);
            tileGroups[first].Add(tile);
            tile.Group = first;
            if (first==1) {
                FastVisit(tile);
            }
        }
    }


    //helper function to translate the tiles back into the maze data
    public void TilesToMaze(bool first=true) {
        int tileSize = 3;
        if (Advanced) {
            tileSize = 2;
        }
        int baseX = 0;
        int baseY = 0;
        // first time creating tiles to maze create the pillars
        if (first) {
            for (int i = 0; i < xTiles; i++) {
                baseX = i * tileSize + 1;
                //Advanced starts left not mid
                if (Advanced) {
                    baseX = i * tileSize;
                }
                for (int j = 0; j < yTiles; j++) {
                    baseY = j * tileSize + 1;
                    //Advanced starts top not mid
                    if (Advanced) {
                        baseY = j * tileSize;
                    }
                    // Room
                    Maze[baseX, baseY].Number = 0;

                    //"Pillars" Advanced only has 1 pillar (right down)
                    if (!Advanced) {
                        Maze[baseX - 1, baseY - 1].Number = 1;
                        Maze[baseX + 1, baseY - 1].Number = 1;
                        Maze[baseX - 1, baseY + 1].Number = 1;
                    }
                    Maze[baseX + 1, baseY + 1].Number = 1;

                    // Setting Walls Advanced has only down and right

                    //Top
                    if (!Advanced) {
                        if (mazeTiles[i, j].OpenUp) {
                            Maze[baseX, baseY - 1].Number = 0;
                        } else {
                            Maze[baseX, baseY - 1].Number = 1;
                        }
                    }
                    //Down
                    if (mazeTiles[i, j].OpenDown) {
                        Maze[baseX, baseY + 1].Number = 0;
                    } else {
                        Maze[baseX, baseY + 1].Number = 1;
                    }
                    //Right
                    if (mazeTiles[i, j].OpenRight) {
                        Maze[baseX+1, baseY].Number = 0;
                    } else {
                        Maze[baseX+1, baseY].Number = 1;
                    }
                    //left
                    if (!Advanced) {

                        if (mazeTiles[i, j].OpenLeft) {
                            Maze[baseX - 1, baseY].Number = 0;
                        } else {
                            Maze[baseX - 1, baseY].Number = 1;
                        }
                    }
                }
            }
        //if this is not the first call
        } else {
            for (int i = 0; i < xTiles; i++) {
                baseX = i * tileSize;
                //Advanced starts top not middle
                if (!Advanced) {
                    baseX++;
                }
                for (int j = 0; j < yTiles; j++) {
                    baseY = j * tileSize;

                    //Advanced starts left not middle
                    if (!Advanced) {
                        baseY++;
                    }

                    // Setting Walls Advanced has only down and right

                    if (!Advanced) {
                        //Top wall
                        if (mazeTiles[i, j].OpenUp) {
                            Maze[baseX, baseY - 1].Number = 0;
                        }
                    }
                    //Down Wall
                    if (mazeTiles[i, j].OpenDown) {
                        Maze[baseX, baseY + 1].Number = 0;
                    } 
                    //Right Wall
                    if (mazeTiles[i, j].OpenRight) {
                        Maze[baseX + 1, baseY].Number = 0;
                    }
                    //Left Wall
                    if (!Advanced) {

                        if (mazeTiles[i, j].OpenLeft) {
                            Maze[baseX - 1, baseY].Number = 0;
                        }
                    }
                }
            }
        }
    }

    // Not yet done / can be used by students
    public void CustomMaze(float waitTime = 0) {
        NewMaze();
        mazeGeneration = StartCoroutine(customMaze(waitTime));
    }

    IEnumerator customMaze(float waitTime = 0) {

        yield return new WaitForSeconds(0);
    }

    // Making additional gaps to make it more interesting
    public void GenerateGaps() {
        if (AdditionalGaps<=0) {
            return;
        }
        switch (GapType) {
            case GapType.simple:
                SimpleGaps();
                break;
            case GapType.better:
                BetterGaps();
                break;
            case GapType.advanced:
                AdvancedGaps();
                break;
            default:
                StupidGaps();
                break;
        }
    }

    // adding random gaps into maze
    public void StupidGaps() {
        int gapCount = 0;

        // prevent endless loops
        int loopCount=0;
        int maxTries = 10000;

        while (gapCount<AdditionalGaps && loopCount<maxTries) {
            loopCount++;
            //choose a random cell if it is a wall, open it
            int x = Random.Range(1, X - 1);
            int y = Random.Range(1, Y - 1);
            if (Maze[x,y].Number>0) {
                gapCount++;
                Maze[x, y].Number = 0;
            }
        }
    }

    public void SimpleGaps() {
        int gapCount = 0;

        // prevent endless loops
        int loopCount = 0;
        int maxTries = 10000;
        while (gapCount < AdditionalGaps && loopCount < maxTries) {
            loopCount++;
            // choose a random cell
            int x = Random.Range(2, X - 2);
            int y = Random.Range(2, Y - 2);
            // if the cell is part of a wall between 2 open spaces open it
            if (Maze[x, y].IsWall &&
                ((Maze[x+1, y].IsWall && Maze[x-1, y].IsWall && !Maze[x, y+1].IsWall && !Maze[x, y-1].IsWall) ||
                (!Maze[x + 1, y].IsWall && !Maze[x - 1, y].IsWall && Maze[x, y + 1].IsWall && Maze[x, y - 1].IsWall))) {
                gapCount++;
                Maze[x, y].Number = 0;
            }
        }
    }

    public void BetterGaps() {
        int gapCount = 0;

        // prevent endless loops
        int loopCount = 0;
        int maxTries = 10000;
        while (gapCount < AdditionalGaps && loopCount < maxTries) {
            loopCount++;
            // choose a random cell
            int x = Random.Range(3, X - 3);
            int y = Random.Range(3, Y - 3);
            // if the cell is part of a long enough wall between 2 open spaces open it
            if (Maze[x, y].IsWall &&
                ((Maze[x + 1, y].IsWall && Maze[x - 1, y].IsWall && Maze[x + 2, y].IsWall && Maze[x - 2, y].IsWall && !Maze[x, y + 1].IsWall && !Maze[x, y - 1].IsWall ) ||
                (!Maze[x + 1, y].IsWall && !Maze[x - 1, y].IsWall && Maze[x, y + 1].IsWall && Maze[x, y - 1].IsWall && Maze[x, y + 2].IsWall && Maze[x, y - 2].IsWall))) {
                gapCount++;
                Maze[x, y].Number = 0;
            }
        }
    }

    public void AdvancedGaps() {
        List<Vector2Int> possiblePositions = new List<Vector2Int>();

        //here we first go through the maze and search for all fitting spaces
        for (int x = 3; x < X-2; x++) {
            for (int y = 3; y < Y-2; y++) {

                bool endLeft = false;
                bool endRight = false;
                if (Maze[x, y].IsWall) {
                    // Check if cell is part of a horizontal wall
                    if (Maze[x + 1, y].IsWall && Maze[x - 1, y].IsWall && (Maze[x + 2, y].IsWall || Maze[x - 2, y].IsWall) && !Maze[x, y + 1].IsWall && !Maze[x, y - 1].IsWall) {
                        for (int i = x + 1; i < X; i++) {
                            //check if the wall has no gap to the right
                            if (!Maze[i, y].IsWall) {
                                endRight = false;
                                break;
                            } else {
                                if (Maze[i, y + 1].IsWall || Maze[i, y - 1].IsWall) {
                                    endRight = true;
                                    break;
                                }
                            }
                        }
                        //check if the wall has no gap to the left
                        for (int i = x - 1; i >= 0; i--) {
                            if (!Maze[i, y].IsWall) {
                                endLeft = false;
                                break;
                            } else {
                                if (Maze[i, y + 1].IsWall || Maze[i, y - 1].IsWall) {
                                    endLeft = true;
                                    break;
                                }
                            }
                        }
                    // Check if cell is part of a vertical wall
                    } else if (!Maze[x + 1, y].IsWall && !Maze[x - 1, y].IsWall && Maze[x, y + 1].IsWall && Maze[x, y - 1].IsWall && (Maze[x, y + 2].IsWall || Maze[x, y - 2].IsWall)) {
                        //check if the wall has no gap to the bottom
                        for (int i = y + 1; i < Y; i++) {
                            if (!Maze[x, i].IsWall) {
                                endRight = false;
                                break;
                            } else {
                                if (Maze[x + 1, i].IsWall || Maze[x - 1, i].IsWall) {
                                    endRight = true;
                                    break;
                                }
                            }
                        }
                        //check if the wall has no gap to the top
                        for (int i = y - 1; i >= 0; i--) {
                            if (!Maze[x, i].IsWall) {
                                endLeft = false;
                                break;
                            } else {
                                if (Maze[x + 1, y].IsWall || Maze[x - 1, y].IsWall) {
                                    endLeft = true;
                                    break;
                                }
                            }
                        }
                    }
                    //if the wall has no gaps add it
                    if (endRight && endLeft) {
                        possiblePositions.Add(new Vector2Int(x,y));
                    }

                }
            }
        }

        // We now choose random cells from all the fitting ones
        for (int i = 0; i < AdditionalGaps; i++) {
            //if there are less fitting cells that wanted gaps do what you can
            if (possiblePositions.Count<=0) {
                break;
            }
            int rnd = Random.Range(0, possiblePositions.Count);
            Vector2Int pos = possiblePositions[rnd];
            Maze[pos.x, pos.y].Number = 0;
        }
    }
}



public enum MazeAlgorithm {
    simple,
    subdivision,
    prim,
    kruskal,
    custom
}

public enum GapType {
    stupid,
    simple,
    better,
    advanced,

}