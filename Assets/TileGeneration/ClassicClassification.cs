using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassicClassification : TileClassificationAlgorithm {

    public override TileClassification GetClassification(int x, int y) {

        TileClassification classification = base.GetClassification(x, y);


        if (!maze[x,y].IsWall) {
            classification.ShouldSpawn = false;
            return classification;
        }

        bool leftOpen = isEmpty(x - 1, y);
        bool rightOpen = isEmpty(x + 1, y);

        //
        bool topOpen = isEmpty(x, y + 1);
        bool botOpen = isEmpty(x, y - 1);


        if (leftOpen && rightOpen && topOpen && botOpen) {
            classification.Type = 0;
        }
        if (leftOpen && rightOpen && !topOpen && botOpen) {
            classification.Type = 1;
        }
        if (leftOpen && rightOpen && topOpen && !botOpen) {
            classification.Type = 2;
        }
        if (!leftOpen && rightOpen && topOpen && botOpen) {
            classification.Type = 3;
        }
        if (leftOpen && !rightOpen && topOpen && botOpen) {
            classification.Type = 4;
        }
        if (!leftOpen && rightOpen && !topOpen && botOpen) {
            classification.Type = 5;
        }
        if (leftOpen && !rightOpen && !topOpen && botOpen) {
            classification.Type = 6;
        }
        if (leftOpen && !rightOpen && topOpen && !botOpen) {
            classification.Type = 7;
        }
        if (!leftOpen && rightOpen && topOpen && !botOpen) {
            classification.Type = 8;
        }
        if (leftOpen && !rightOpen && !topOpen && !botOpen) {
            classification.Type = 9;
        }
        if (!leftOpen && !rightOpen && topOpen && botOpen) {
            classification.Type = 10;
        }
        if (!leftOpen && !rightOpen && topOpen && !botOpen) {
            classification.Type = 11;
        }
        if (!leftOpen && !rightOpen && !topOpen && botOpen) {
            classification.Type = 12;
        }
        if (!leftOpen && rightOpen && !topOpen && !botOpen) {
            classification.Type = 13;
        }
        if (leftOpen && rightOpen && !topOpen && !botOpen) {
            classification.Type = 14;
        }
        if (!leftOpen && !rightOpen && !topOpen && !botOpen) {
            classification.Type = 15;
        }
        return classification;
    }


}
