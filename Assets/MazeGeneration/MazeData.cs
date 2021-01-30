using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
// Maze data class is just a custom 2D array, which is serializable
public class MazeData {

    // Size
    public int X;
    public int Y;

    // Data
    public MazeColumns[] Columns;

    // Initializing the data (as empty)
    public MazeData(int x, int y) {
        Columns = new MazeColumns[x];
        for (int i = 0; i < x; i++) {
            Columns[i] = new MazeColumns(y);
        }
        X = x;
        Y = y;
    }

    // Simple accessor (like 2D array)
    public MazeCell this[int index, int index2] {
        get {
            return Columns[index].Elements[index2];
        }
        set {
            Columns[index].Elements[index2] = value;
        }
    }

}

[System.Serializable]
// Columns of MazeData
public class MazeColumns {
    // Data
    public MazeCell[] Elements;

    // Initializing the data (as empty)
    public MazeColumns(int y) {
        Elements = new MazeCell[y];
        for (int i = 0; i < y; i++) {
            Elements[i] = new MazeCell(0);
        }
    }
}

[System.Serializable]
//needs to be class (and not struct) else can't be returned by accessor (from MazeData)
public class MazeCell {

    public MazeCell(int value) {
        Number = value;
    }
    public int Number;

    public bool IsWall {
        get {
            return Number > 0;
        }
    }

    public WalkDirection LastCell = WalkDirection.none;

    public bool HasPlayer = false;

    public bool HasChips = false;

    public bool HasRope;

    public int Distance;

}

public enum WalkDirection {
    none,
    up,
    right,
    down,
    left
}

[System.Serializable]
// Used by subdivision algorithm
public struct MazeRegion {
    //start and ends of the regions
    public int XStart;
    public int XEnd;
    public int YStart;
    public int YEnd;
    // which values are forbidden (because there are openings at that place)
    public int forbiddenY1;
    public int forbiddenY2;
    public int forbiddenX1;
    public int forbiddenX2;
    //how often was this taken?
    public int takenCount;
}

[System.Serializable]
// Tiles the maze consists of. Used by Prim and Kruskal algorithm.
// Class instead of struct to make comparisons easier
public class MazeTile {
    public int PosX;
    public int PosY;

    public MazeTile Up;
    public MazeTile Right;
    public MazeTile Down;
    public MazeTile Left;

    public int Group = 0;

    public bool Visited = false;

    public bool OpenUp = false;
    public bool OpenRight = false;
    public bool OpenDown = false;
    public bool OpenLeft = false;

}

[System.Serializable]
// Connection of 2 tiles. Used by Prim and Kruskal algorithm.
public class TileConnection {
    //Cost of connection (not needed for labyrinths)
    public int Cost;
    public MazeTile First;
    public MazeTile Second;
    public bool Horizontal = true;
}



/*
// used for sorting.
public class ConnectionComparer : IComparer {
    public int Compare(object x, object y) {
        TileConnection xTile = (TileConnection)x;
        TileConnection yTile = (TileConnection)y;
        if (xTile==null || yTile==null) {
            return ((new CaseInsensitiveComparer()).Compare(x, y));
        }
        return ((new CaseInsensitiveComparer()).Compare(xTile.Cost, yTile.Cost));
    }
}
*/
