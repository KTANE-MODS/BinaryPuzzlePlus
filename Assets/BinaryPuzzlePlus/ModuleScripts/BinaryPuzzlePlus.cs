using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using System;

public class BinaryPuzzlePlus : MonoBehaviour
{
    [SerializeField]
    private Sprite XSprite;
    [SerializeField]
    private Sprite EqualSprite;

    [SerializeField]
    private GameObject buttonPrefab;
    [SerializeField]
    private GameObject edgePrefab;
    [SerializeField]
    private KMSelectable resetButton;

    private GameObject[] leftEdges;
    private GameObject[] upEdges;

    private List<Button> buttons;
    private KMBombModule Module;
    private static int ModuleIdCounter = 1;
    private int ModuleId;

    [Header("Puzzle Settings")]
    [Range(6, 10)]
    [Tooltip("The dimension of the puzzle. Must be between 6 and 10 and even.")]
    [SerializeField]
    private int size;
    [Tooltip("If rows/columns should be distinct from one another")]
    [SerializeField]
    private bool distinctRows;

    [Header("Button Materials")]
    [SerializeField]
    private Material defaultMaterial;
    [SerializeField]
    private Material yellowMaterial;
    [SerializeField]
    private Material blueMaterial;

    #region Mod Settings
    BinaryPuzzlePlusSettings modSettings;

    static Dictionary<string, object>[] TweaksEditorSettings = new Dictionary<string, object>[] {
        new Dictionary<string, object> {
             { "Filename", "BinaryPuzzlePlus.json" },
             { "Name", "BinaryPuzzlePlusSettings" },
             { "Listing", new List<Dictionary<string, object>>{
                    new Dictionary<string, object> {
                         { "Key", "Size" },
                         { "Text", "The row/column size of the puzzle. Valid values are 6, 8, and 10. Default is 6." }
                    },
                    new Dictionary<string, object> {
                         { "Key", "DistinctRows" },
                         { "Text", "If true, the puzzle will enforce a rule where row/columns must be distinct. Default is false." }
                    }
             } }
        }
    };

    #endregion


    private PuzzleState solution; //the generated solution to the puzzle
    private PuzzleState givenPuzzle; //what the player is given to complete the puzzle

    private PuzzleState currentState;

    private bool moduleSolved = false;

    void Awake()
    {
        Debug.Log(1);
        ModuleId = ModuleIdCounter++;
        Debug.Log(2);
        Module = GetComponent<KMBombModule>();
        Debug.Log(3);
        resetButton.GetComponent<MeshRenderer>().sharedMaterial = yellowMaterial;
        Debug.Log(4);
        ConfigureModSettings();
        Debug.Log(5);
    }

    void Start()
    {
        Debug.Log(6);
        PuzzleGenerator.Size = size;
        Debug.Log(7);
        PuzzleGenerator.DistinctRows = distinctRows;
        Debug.Log(8);
        solution = PuzzleGenerator.GenerateSolution();
        Debug.Log(9);
        givenPuzzle = PuzzleGenerator.ReduceConstraints(solution);
        Debug.Log(10);

        //todo change the solution so it shows the same edges as the given puzzle
        for (int i = 0; i < size * size; i++)
        {
            Debug.Log(11);
            if (givenPuzzle.Cells[i].Left != null)
            { 
                solution.Cells[i].Left.Constraint = givenPuzzle.Cells[i].Left.Constraint;
                Debug.Log(12);
            }
            if (givenPuzzle.Cells[i].Up != null)
            { 
                solution.Cells[i].Up.Constraint = givenPuzzle.Cells[i].Up.Constraint;
                Debug.Log(13);
            }
            Debug.Log(14);
        }

        Debug.Log(15);
        ConfigurePuzzleGrid();
        Debug.Log(16);

        currentState = new PuzzleState(givenPuzzle);
        Debug.Log(17);

        buttons.ForEach(b => b.Cell = currentState.Cells[buttons.IndexOf(b)]);
        Debug.Log(18);


        UpdateVisuals(currentState);
        Debug.Log(19);

        //Make the button interactable
        KMSelectable parent = GetComponent<KMSelectable>();
        Debug.Log(20);

        foreach (var button in buttons)
        {
            Debug.Log(22);
            button.Selectable.Parent = parent;
            Debug.Log(23);

            button.Selectable.OnInteract += delegate ()
            {
                ButtonPress(button);
                return false;
            };
        }

        Debug.Log(24);
        parent.Children = parent.Children.Concat(buttons.Select(b => b.Selectable)).ToArray();
        Debug.Log(25);
        parent.UpdateChildrenProperly();
        Debug.Log(26);

        Log("Puzzle Settings");
        Log($"Size: {size}");
        Log($"Distinct Rows: {distinctRows}");
        Log("Solution\n" + solution.ToString());
        Log("Given\n" + givenPuzzle.ToString());
    }

private void ConfigureModSettings()
{
        var modConfig = new ModConfig<BinaryPuzzlePlusSettings>("BinaryPuzzlePlusSettings");
        Debug.Log(27);
        var defaultSetings = new BinaryPuzzlePlusSettings();
        Debug.Log(28);

        modSettings = modConfig.Settings;
        Debug.Log(29);
        modConfig.Settings = modSettings;
        Debug.Log(30);
        Log($"Given mod settings\nSize: {modSettings.Size}\nDistinctRows: {modSettings.DistinctRows}");

        //Validate the values
        if (modSettings.Size % 2 != 0 || (modSettings.Size < 6 && modSettings.Size > 10))
        {
            Log($"Invalid mod setting size. Recieved: {modSettings.Size}. Using default value of {defaultSetings.Size}");
            size = defaultSetings.Size;
        }

        else
        {
            Debug.Log(31);
            size = modConfig.Settings.Size;
        }

        distinctRows = modConfig.Settings.DistinctRows;
        Debug.Log(32);
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
        if(isVertical) edge.transform.Rotate(0, 90, 0);
        return edge;
    }

    /// <summary>
    /// Updates the visuals of the modules to reflect a puzzle state
    /// </summary>
    void UpdateVisuals(PuzzleState state)
    {
        Debug.Log(33);
        for (int ix = 0; ix < size * size; ix++)
        {

            Cell c = state.Cells[ix];
            Debug.Log(34); 
            Button b = buttons[ix];
            Debug.Log(35);
            GameObject leftEdge = leftEdges[ix];
            Debug.Log(36);
            GameObject upEdge = upEdges[ix];
            Debug.Log(37);

            //change the background color
            b.MeshRenderer.sharedMaterial = c.state == CellState.Empty ? defaultMaterial : c.state == CellState.One ? yellowMaterial : blueMaterial;
            Debug.Log(38);

            //change the text
            b.Label.text = c.state == CellState.Empty ? "" : c.state == CellState.One ? "1" : "0";
            Debug.Log(39);

            //set edges that exist
            if (leftEdge != null)
            { 
                ChangeEdgeSymbol(leftEdge, c.Left.Constraint, false);
                Debug.Log(40);
            }

            if (upEdge != null)
            {
                ChangeEdgeSymbol(upEdge, c.Up.Constraint, true);
                Debug.Log(41);
            }
        }
    }

    void ChangeEdgeSymbol(GameObject edge, EdgeState state, bool isVertical)
    {
        //if the text is "", hide the gameobject
        if (state == EdgeState.None)
        {
            edge.SetActive(false);
            return;
        }

        edge.transform.Find("Sprite").GetComponent<SpriteRenderer>().sprite = state == EdgeState.Equal ? EqualSprite : XSprite;
    }
    private void ButtonPress(Button button)
    {
        //if the cell is given, don't do anything
        if (moduleSolved || givenPuzzle.Cells[buttons.IndexOf(button)].state != CellState.Empty)
            return;

        //cycle through the states
        button.Cell.state = (CellState)(((int)button.Cell.state + 1) % 3);
        currentState.Cells[buttons.IndexOf(button)].state = button.Cell.state;
        UpdateVisuals(currentState);

        //Check if the puzzle is solved
        if (Solved())
        {
            Solve();
        }
    }

    private bool Solved()
    {
        for (int i = 0; i < size * size; i++)
        {
            if (currentState.Cells[i].state != solution.Cells[i].state)
                return false;
        }
        return true;
    }

    private void Solve()
    {
        Log("Puzzle Solved");
        Module.HandlePass();
        resetButton.transform.Find("Label").GetComponent<TextMesh>().text = "SOLVED";
        moduleSolved = true;
        UpdateVisuals(currentState);
    }

    private void Log(string message)
    {
        Debug.Log($"[Binary Puzzle Plus #{ModuleId}] {message}");
    }

    
}