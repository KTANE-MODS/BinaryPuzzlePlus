using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratedPuzzles {
    public static List<Grid> Grids = new List<Grid>();

    //Gives a predeterminted puzzle. Placeholder until puzzle generation is implemented
    public static void GeneratePuzzles(int size)
    {
        Grid g1 = new Grid(6);
        
        g1.Cells[0, 1].SetPermanance(1);
        g1.Cells[0, 2].SetPermanance(1);
        g1.Cells[0, 3].SetPermanance(0);
        g1.Cells[0, 4].SetPermanance(0);
        g1.Cells[0, 5].EdgeDown.State = EdgeState.X;

        g1.Cells[1, 1].EdgeDown.State = EdgeState.Equals;
        g1.Cells[1, 5].EdgeDown.State = EdgeState.X;

        g1.Cells[2, 2].EdgeDown.State = EdgeState.Equals;

        g1.Cells[4, 4].EdgeRight.State = EdgeState.Equals;

        g1.Cells[5, 1].SetPermanance(1);
        g1.Cells[5, 2].SetPermanance(0);
        g1.Cells[5, 3].SetPermanance(1);
        g1.Cells[5, 4].SetPermanance(0);

        Grids.Add(g1);
    }
}
