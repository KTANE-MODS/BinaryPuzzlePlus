using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class BinaryPuzzlePlus : MonoBehaviour {

    [SerializeField]
    private GameObject buttonPrefab;
    [SerializeField]
    private GameObject edgePrefab;

    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved;

    private Grid grid;
    const int size = 6;

    private KMSelectable[,] buttons; 

    void Awake () {
        float spacing = -0.0214f;
        ModuleId = ModuleIdCounter++;

        GeneratedPuzzles.GeneratePuzzles(size);
        grid = GeneratedPuzzles.Grids.PickRandom();

        buttons = new KMSelectable[size, size];
        List<KMSelectable> buttonList = new List<KMSelectable>();

        KMSelectable parentSelectable = GetComponent<KMSelectable>();
        Vector3 staringPos = new Vector3(-0.073f, 0.0154f, 0.0697f);
        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                Cell c = grid.Cells[row, col];
                GameObject button = Instantiate(buttonPrefab, transform);
                button.transform.position = staringPos + new Vector3(col * Math.Abs(spacing), 0, row * spacing);
                KMSelectable b = button.GetComponent<KMSelectable>();
                buttonList.Add(b);
                b.Parent = parentSelectable;
                b.GetComponent<KMSelectable>().OnInteract += delegate () { if (!ModuleSolved) { c.Interact(); } return false; };
                c.Text = button.transform.Find("text").GetComponent<TextMesh>();
                c.UpdateText();

                //these two if statements should be turned into a function to remove duplicate code

                if (c.EdgeRight != null && c.EdgeRight.State != EdgeState.None)
                {
                    GameObject rightEdge = Instantiate(edgePrefab, transform);
                    rightEdge.transform.position = button.transform.position + new Vector3(Math.Abs(spacing) / (float)2, 0, 0);
                    string edgeText = c.EdgeRight.Log();
                    rightEdge.GetComponent<TextMesh>().text = edgeText == "." ? "" :  edgeText;
                }

                if (c.EdgeDown != null && c.EdgeDown.State != EdgeState.None)
                {
                    GameObject downEdge = Instantiate(edgePrefab, transform);
                    downEdge.transform.position = button.transform.position + new Vector3(0, 0, spacing / (float)2);
                    string edgeText = c.EdgeDown.Log();
                    downEdge.GetComponent<TextMesh>().text = edgeText == "." ? "" : edgeText;
                }

            }
        }

        parentSelectable.Children = buttonList.ToArray();
        parentSelectable.UpdateChildrenProperly();

        Log(grid.Log());
    }

    void Start () {
 
    }
 
    void Update () {
 
    }

    private void Log(string s)
    {
        Debug.Log($"[Binary Puzzle Plus #{ModuleId}] {s}");
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} to do something.";
 #pragma warning restore 414
 
    IEnumerator ProcessTwitchCommand (string Command) {
       yield return null;
    }
 
    IEnumerator TwitchHandleForcedSolve () {
       yield return null;
    }
}
