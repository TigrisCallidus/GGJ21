using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplerClassification : TileClassificationAlgorithm
{
    public override TileClassification GetClassification(int x, int y) {

        TileClassification classification = base.GetClassification(x, y);


        if (!maze[x, y].IsWall) {
            classification.ShouldSpawn = false;
            return classification;
        }

        int count = 0;

        bool leftOpen = isEmpty(x - 1, y);
        bool rightOpen = isEmpty(x + 1, y);
        bool topOpen = isEmpty(x, y - 1);
        bool botOpen = isEmpty(x, y + 1);

        if (leftOpen) {
            count++;
        }

        if (rightOpen) {
            count++;
        }

        if (topOpen) {
            count++;
        }

        if (botOpen) {
            count++;
        }

        if (count==0) {
            classification.Type = 0;
        }

        if (count==1) {
            classification.Type = 1;
            if (rightOpen) {
                classification.Rotation = new Vector3(0, 90, 0);
            } else if (botOpen) {
                classification.Rotation = new Vector3(0, 180, 0);
            } else if (leftOpen) {
                classification.Rotation = new Vector3(0, 270, 0);
            }
        }

        if (count == 3) {
            classification.Type = 3;
            if (!rightOpen) {
                classification.Rotation = new Vector3(0, 90, 0);
            } else if (!botOpen) {
                classification.Rotation = new Vector3(0, 180, 0);
            } else if (!leftOpen) {
                classification.Rotation = new Vector3(0, 270, 0);
            }
        }

        if (count == 4) {
            classification.Type = 4;
        }

        if (count==2) {
            if ((topOpen&&botOpen) || (leftOpen && rightOpen)) {
                classification.Type = 5;
                if (rightOpen) {
                    classification.Rotation = new Vector3(0, 90, 0);
                }

            } else {
                classification.Type = 2;
                if (topOpen) {
                    if (leftOpen) {
                        classification.Rotation = new Vector3(0, 270, 0);
                    }

                } else {
                    if (rightOpen) {
                        classification.Rotation = new Vector3(0, 90, 0);
                    } else {
                        classification.Rotation = new Vector3(0, 180, 0);
                    }
                }

            }
        }


        return classification;
    }

}
