using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
using static HarmonyLib.Code;

public class BinaryPuzzlePlus : MonoBehaviour {

    private KMBombModule BombModule;
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
        BombModule = GetComponent<KMBombModule>();
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
                b.GetComponent<KMSelectable>().OnInteract += delegate () { if (!ModuleSolved) { c.Interact(); CheckForSolve(); } return false; };
                c.Text = button.transform.Find("text").GetComponent<TextMesh>();
                c.UpdateText();

                CreateEdge(c, c.EdgeRight, new Vector3(Math.Abs(spacing) / 2f, 0, 0), button);
                CreateEdge(c, c.EdgeDown, new Vector3(0, 0, spacing / 2f), button);
            }
        }

        parentSelectable.Children = buttonList.ToArray();
        parentSelectable.UpdateChildrenProperly();

        Log("Starting Grid:" + grid.Log());
    }

    private void CreateEdge(Cell c, Edge edge, Vector3 offset, GameObject button)
    {
        if (edge == null || edge.State == EdgeState.None)
            return;

        GameObject edgeObj = Instantiate(edgePrefab, transform);
        edgeObj.transform.position = button.transform.position + offset;

        string edgeText = edge.Log();
        edgeObj.GetComponent<TextMesh>().text = edgeText == "." ? "" : edgeText;
    }

    private void Log(string s)
    {
        Debug.Log($"[Binary Puzzle Plus #{ModuleId}] {s}");
    }

    void CheckForSolve()
    {

        //check that all the cells are filled
        if (!grid.CellList.All(c => c.Value != null))
        {
            Debug.Log("Not all cells filled");
            return;
        }

        // Check that we don’t get more than two of the same digit in a straight row/column
        for (int row = 0; row > size; row++)
        {
            for (int col = 0; col > size; col++)
            {
                //these functions are very similar. This can be a function
                if (row >= 2 && grid.Cells[row, col].Value == grid.Cells[row - 1, col].Value && grid.Cells[row, col].Value == grid.Cells[row - 2, col].Value)
                {
                    Debug.Log("3 cells in a row");
                    return;
                }

                if (col >= 2 && grid.Cells[row, col].Value == grid.Cells[row, col - 1].Value && grid.Cells[row, col].Value == grid.Cells[row, col - 2].Value)
                {
                    Debug.Log("3 cells in a col");
                    return;
                }
            }
        }

        //For all edges that that are X's, make sure the cells are opposite
        Edge[] crossEdges = grid.Edges.Where(e => e.State == EdgeState.X).ToArray();

        foreach (Edge edge in crossEdges)
        {
            if (edge.CellA.Value == edge.CellB.Value)
            {
                Debug.Log("cross edge has the same value");
                return;
            }
        }

        //For all edges that that are ='s, make sure the cells are the same
        Edge[] equalEdges = grid.Edges.Where(e => e.State == EdgeState.Equals).ToArray();

        foreach (Edge edge in equalEdges)
        {
            if (edge.CellA.Value != edge.CellB.Value)
            {
                Debug.Log("same edge has the diferent value value");
                return;
            }
        }

        Solve();
    }

    public void Solve()
    {
        ModuleSolved = true;
        BombModule.HandlePass();

        Log("Submitted Grid:" + grid.Log());
        Log("Module Solve");
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
