using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
using static HarmonyLib.Code;
using UnityEngine.Experimental.UIElements;

public class BinaryPuzzlePlus : MonoBehaviour {

    private KMBombModule BombModule;
    [SerializeField]
    private GameObject buttonPrefab;
    [SerializeField]
    private GameObject edgePrefab;
    [SerializeField]
    private KMSelectable resetButton;

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

        KMSelectable parentSelectable = GetComponent<KMSelectable>();
        Vector3 staringPos = new Vector3(-0.073f, 0.0154f, 0.0697f);

        resetButton.OnInteract += delegate () { if (!ModuleSolved) { ResetBoard(); } return false; };
        List<KMSelectable> buttonList = new List<KMSelectable>();

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
                b.GetComponent<KMSelectable>().OnInteract += delegate () { if (!ModuleSolved) { c.Interact(); if(IsSolved()) Solve(); } return false; };
                c.Text = button.transform.Find("text").GetComponent<TextMesh>();
                c.UpdateText();

                CreateEdge(c, c.EdgeRight, new Vector3(Math.Abs(spacing) / 2f, 0, 0), button);
                CreateEdge(c, c.EdgeDown, new Vector3(0, 0, spacing / 2f), button);
            }
        }

        buttonList.Add(resetButton);

        parentSelectable.Children = buttonList.ToArray();
        parentSelectable.UpdateChildrenProperly();

        Log("Starting Grid:" + grid.Log());
    }

    private void ResetBoard()
    {
        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                Cell c = grid.Cells[row, col];
                if (!c.Permanent)
                {
                    c.Value = null;
                }
                c.UpdateText();

            }
        }
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

    bool IsSolved()
    {

        //check that all the cells are filled
        if (!grid.CellList.All(c => c.Value != null))
        {
            return false;
        }

        // Check that we don’t get more than two of the same digit in a straight row/column
        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                if (row >= 2 && grid.Cells[row, col].Value == grid.Cells[row - 1, col].Value && grid.Cells[row, col].Value == grid.Cells[row - 2, col].Value)
                {
                    return false;
                }

                if (col >= 2 && grid.Cells[row, col].Value == grid.Cells[row, col - 1].Value && grid.Cells[row, col].Value == grid.Cells[row, col - 2].Value)
                {
                    return false;
                }
            }
        }

        //For all edges that that are X's, make sure the cells are opposite
        if (!ValidateEdges(grid.Edges.Where(e => e.State == EdgeState.X),
                           (a, b) => a != b))
        {
            return false;
        }

        //For all edges that that are ='s, make sure the cells are the same
        if (!ValidateEdges(grid.Edges.Where(e => e.State == EdgeState.Equals),
                           (a, b) => a == b))
        {
            return false;
        }

        return true;
    }

    private bool ValidateEdges(IEnumerable<Edge> edges, Func<int, int, bool> condition)
    {
        foreach (var edge in edges)
        {
            if (!condition((int)edge.CellA.Value, (int)edge.CellB.Value))
            {
                return false;
            }
        }
        return true;
    }


    public void Solve()
    {
        ModuleSolved = true;
        BombModule.HandlePass();

        Log("Submitted Grid:" + grid.Log());
        Log("Module Solved");
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
