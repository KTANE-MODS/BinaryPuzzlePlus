using System;
using System.Collections.Generic;
using UnityEngine;

public class BinaryPuzzlePlus : MonoBehaviour
{

    [SerializeField]
    private GameObject buttonPrefab;
    [SerializeField]
    private GameObject edgePrefab;
    [SerializeField]
    private KMSelectable resetButton;

    private GameObject[] leftEdges;
    private GameObject[] upEdges;

    private List<Button> buttons;

    private KMBombInfo Bomb;
    private KMBombModule Module;
    private KMAudio Audio;
    private static int ModuleIdCounter = 1;
    private int ModuleId;

    [Header("Puzzle Settings")]
    [Range(6, 10)]
    [Tooltip("The dimension of the puzzle. Must be between 6 and 10.")]
    [SerializeField]
    private int size;
    [Tooltip("If rows/columns should be distinct from one another")]
    [SerializeField]
    private bool distinctRows;

    [Tooltip("if true, cells and edges will be generated at the same time. If false, cells will be generated first and all possible edges will be placed after. Edges will be removed after generation based on the constraintRemovalPercentage variable.")]
    [SerializeField]
    private bool longGeneration;

    [Tooltip("The chance of a constraint being removed when generating the puzzle. Only used if Long Generation is false.")]
    [Range(0, 100)]
    [SerializeField]
    private int constraintRemovalPercentage;


    [Header("Button Materials")]
    [SerializeField]
    private Material defaultMaterial;
    [SerializeField]
    private Material yellowMaterial;
    [SerializeField]
    private Material blueMaterial;

    private PuzzleState solution; //the generated solution to the puzzle



    void Awake()
    {
        ModuleId = ModuleIdCounter++;
    }

    void Start()
    {
        PuzzleGenerator.LongGeneration = longGeneration;
        PuzzleGenerator.Size = size;
        PuzzleGenerator.DistinctRows = distinctRows;
        PuzzleGenerator.ConstraintRemovalPercentage = constraintRemovalPercentage;
        solution = PuzzleGenerator.GeneratePuzzle();
        ConfigurePuzzleGrid();
        UpdateVisuals(solution);

        Log("Puzzle Settings");
        Log($"Long Generation: {longGeneration}");
        Log($"Size: {size}");
        Log($"Distinct Rows: {distinctRows}");
        Log($"Constraint Removal Percentage: {constraintRemovalPercentage}");

        Log("Solution\n"+solution.ToString());
    }

    /// <summary>
    /// Instantiates buttons in a grid based on the size variables
    /// </summary>
    private void ConfigurePuzzleGrid()
    {
        const float scaleFactor = 1.18f;
        buttons = new List<Button>();
        leftEdges = new GameObject[size * size];
        upEdges = new GameObject[size * size];

        //instantiate the buttons base on the size

        //local starting/ending position of the puzzle grid
        Vector2 startingPosition = new Vector2(-0.0561f, 0.0466f);
        const float puzzleDimension = 0.1122f;
        Vector2 endingPosition = startingPosition + new Vector2(puzzleDimension, -puzzleDimension);

        //figure out the width and height of each button
        float desiredWidth = Mathf.Abs(endingPosition.x - startingPosition.x) / size;
        float desiredHeight = Mathf.Abs(endingPosition.y - startingPosition.y) / size;

        //find what to scale the buttons to in order to have the wanted width/height
        float currentWidh = buttonPrefab.GetComponent<Renderer>().bounds.size.x;
        float currentHeight = buttonPrefab.GetComponent<Renderer>().bounds.size.z;

        float xScale = (desiredWidth * buttonPrefab.transform.localScale.x) / currentWidh * scaleFactor;
        float zScale = (desiredWidth * buttonPrefab.transform.localScale.z) / currentHeight * scaleFactor;

        //Populate the grid with buttons starting from the starting position and moving across and down to the ending position
        for (int row = 0; row < size; row++)
        {
            for (int column = 0; column < size; column++)
            {
                float x = Vector2.Lerp(startingPosition, endingPosition, (float)column / (size - 1)).x;
                float z = Vector2.Lerp(startingPosition, endingPosition, (float)row / (size - 1)).y;

                Vector3 buttonPosition = new Vector3(x, buttonPrefab.transform.position.y, z);
                GameObject button = Instantiate(buttonPrefab, transform);
                button.transform.localPosition = buttonPosition;
                button.transform.localScale = new Vector3(xScale, buttonPrefab.transform.localScale.y, zScale);
                buttons.Add(
                    new Button(
                        button.GetComponent<KMSelectable>(), button.GetComponent<MeshRenderer>(), button.transform.Find("Label").GetComponent<TextMesh>()
                    )
                );
            }
        }


        //for each left edge, instantiate an edge prefab in the middle of the two buttons it connects
        for (int ix = 0; ix < buttons.Count; ix++)
        {
            int row = ix / size;
            int col = ix % size;

            if (col != 0)
            {
                Button button = buttons[ix];
                Button leftButton = buttons[ix - 1];
                leftEdges[ix] = GetNewEdge(leftButton, button, xScale, zScale, false);
            }

            if (row != 0)
            {
                Button button = buttons[ix];
                Button upButton = buttons[ix - size];
                upEdges[ix] = GetNewEdge(button, upButton, xScale, zScale, true);
            }
        }
    }

    GameObject GetNewEdge(Button b1, Button b2, float xScale, float zScale, bool isVertical)
    {
        //Make the edge appear above the button
        const float yPosition = 0.02584f;
        const float scaleFactor = .9f;

        Vector3 edgePosition = Vector3.Lerp(b1.MeshRenderer.gameObject.transform.localPosition, b2.MeshRenderer.gameObject.transform.localPosition, 0.5f);
        edgePosition.y = yPosition;
        GameObject edge = Instantiate(edgePrefab, transform);
        edge.transform.localPosition = edgePosition;
        edge.transform.localScale = new Vector3(xScale * scaleFactor, edgePrefab.transform.localScale.y, zScale * scaleFactor);
        if (isVertical) edge.transform.Rotate(0, 90, 0);
        return edge;
    }

    /// <summary>
    /// Updates the visuals of the modules to reflect a puzzle state
    /// </summary>
    void UpdateVisuals(PuzzleState state)
    {
        for (int ix = 0; ix < size * size; ix++)
        { 
            Cell c = state.Cells[ix];
            Button b = buttons[ix];
            GameObject leftEdge = leftEdges[ix];
            GameObject upEdge = upEdges[ix];

            //change the background color
            b.MeshRenderer.sharedMaterial = c.state == CellState.Empty ? defaultMaterial : c.state == CellState.One ? yellowMaterial : blueMaterial;

            //change the text
            b.Label.text = c.state == CellState.Empty ? "" : c.state == CellState.One ? "1": "0";

            //set edges that exist
            if(leftEdge != null)
                ChangeEdgeSymbol(leftEdge, c.Left.Constraint, false);

            if (upEdge != null)
                ChangeEdgeSymbol(upEdge, c.Up.Constraint, true);
        }
    }

    void ChangeEdgeSymbol(GameObject edge, EdgeState state, bool isVertical)
    {
        //if the text is "", hide the gameobject
        string text = state == EdgeState.X ? "X" : state == EdgeState.Equal ? "=" : "";
        edge.SetActive(text != "");
        edge.transform.Find("Text").GetComponent<TextMesh>().text = text;

        //if the edge is vertical and the text is X, rotate the text 90 degrees
        if (isVertical && text == "X")
        {
            edge.transform.Rotate(0, 90, 0);
        }
    }

    private void Log(string message)
    {
        Debug.Log($"[Binary Puzzle Plus #{ModuleId}] {message}");
    }








}