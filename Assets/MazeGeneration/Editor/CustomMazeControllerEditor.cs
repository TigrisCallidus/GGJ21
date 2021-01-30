using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(MazeController))]
//Custom Editor for the BazeMaze class, adding some buttons and a representation of the Maze
public class CustomMazeControllerEditor : Editor {

    MazeController targetController;

    Texture2D blueBack;
    Texture2D redBack;
    Texture2D greenBack;
    Texture2D violetBack;
    Texture2D yellowBack;

    GUIStyle blue;
    GUIStyle red;
    GUIStyle green;
    GUIStyle violet;
    GUIStyle yellow;

    void OnEnable() {
        targetController = target as MazeController;

        // Make black or red backgrounds
        blueBack = MakeTex(1, 1, new Color(0f, 0f, 1.0f, 0.2f));
        redBack = MakeTex(1, 1, new Color(1.0f, 0f, 0f, 0.2f));
        greenBack = MakeTex(1, 1, new Color(0f, 1.0f, 0f, 0.3f));
        violetBack = MakeTex(1, 1, new Color(1.0f, 0f, 1.0f, 0.2f));
        yellowBack = MakeTex(1, 1, new Color(1.0f, 1.0f, 0f, 0.2f));



    }

    public override void OnInspectorGUI() {

        // Let the default inspecter draw all the values
        DrawDefaultInspector();
        // Spawn buttons

        if (GUILayout.Button("Generate Maze")) {
            targetController.GenerateMaze();
        }

        //if the tiles should be hidden stop here
        if (targetController.HideTiles) {
            return;
        }

        //Making 2 different guy styles
        blue = new GUIStyle(EditorStyles.textField);
        blue.normal.background = blueBack;

        red = new GUIStyle(EditorStyles.textField);
        red.normal.background = redBack;

        green = new GUIStyle(EditorStyles.textField);
        green.normal.background = greenBack;

        violet = new GUIStyle(EditorStyles.textField);
        violet.normal.background = violetBack;

        yellow = new GUIStyle(EditorStyles.textField);
        yellow.normal.background = yellowBack;

        //Spawn fields to represent the Maze. Walls get blue background open places red.
        EditorGUILayout.BeginHorizontal();
        for (int x = 0; x < targetController.Maze.X; x++) {
            EditorGUILayout.BeginVertical();
            for (int y = targetController.Maze.Y-1; y >=0 ; y--) {
                if (targetController.Maze[x, y].HasPlayer) {
                    targetController.Maze[x, y].Number = EditorGUILayout.IntField(targetController.Maze[x, y].Number, green);
                } else if (targetController.Maze[x, y].Number>0) {
                    targetController.Maze[x, y].Number = EditorGUILayout.IntField(targetController.Maze[x, y].Number, blue);
                } else {
                    if (targetController.Maze[x, y].HasRope) {
                        targetController.Maze[x, y].Number = EditorGUILayout.IntField(targetController.Maze[x, y].Number, violet);
                    } else if (targetController.Maze[x, y].HasChips) {
                        targetController.Maze[x, y].Number = EditorGUILayout.IntField(targetController.Maze[x, y].Number, yellow);
                    } else {
                        targetController.Maze[x, y].Number = EditorGUILayout.IntField(targetController.Maze[x, y].Number, red);
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Walk Up")) {
            targetController.Walk(WalkDirection.up);
        }
        if (GUILayout.Button("Walk right")) {
            targetController.Walk(WalkDirection.right);
        }
        if (GUILayout.Button("Walk down")) {
            targetController.Walk(WalkDirection.down);
        }
        if (GUILayout.Button("Walk left")) {
            targetController.Walk(WalkDirection.left);
        }
    }


    // Small helper function to make a background texture
    private Texture2D MakeTex(int width, int height, Color col) {
        Color[] pix = new Color[width * height];

        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();

        return result;
    }


}