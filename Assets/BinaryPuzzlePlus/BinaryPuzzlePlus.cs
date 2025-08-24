using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class BinaryPuzzlePlus : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;

    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved;

    private Grid grid;

    void Awake () {
       ModuleId = ModuleIdCounter++;
       GeneratedPuzzles.GeneratePuzzles();
       grid = GeneratedPuzzles.Grids.PickRandom();
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
