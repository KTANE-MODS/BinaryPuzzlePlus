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
        KMSelectable parentSelectable = GetComponent<KMSelectable>();
        Vector3 staringPos = new Vector3(-0.073f, 0.0154f, 0.0697f);
        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                Cell c = grid.Cells[row, col];
                GameObject button = Instantiate(buttonPrefab, transform);
                button.transform.position = staringPos + new Vector3(row * Math.Abs(spacing), 0, col * spacing);
                KMSelectable b = button.GetComponent<KMSelectable>();
                b.Parent = parentSelectable;
                b.GetComponent<KMSelectable>().OnInteract += delegate () { if (!ModuleSolved) { c.Interact(); } return false; };
                c.Text = button.GetComponent<TextMesh>();
                c.UpdateText();

            }
        }

        parentSelectable.Children = buttons.Cast<KMSelectable>().ToArray();
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
