using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorClassification : TileClassificationAlgorithm {

    public override TileClassification GetClassification(int x, int y) {

        TileClassification classification = base.GetClassification(x, y);


        if (!maze[x, y].IsWall) {
            classification.ShouldSpawn = false;
            return classification;
        }

        classification.Type = maze[x, y].Number;


        return classification;
    }


}

