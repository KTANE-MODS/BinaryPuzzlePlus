using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    private MeshRenderer resetButtonMeshRender;
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
                         { "Text", "If true, the puzzle will enforce a rule where row/columns must be distinct. Default is true." }
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
        ModuleId = ModuleIdCounter++;
        
        Module = GetComponent<KMBombModule>();
        
        resetButtonMeshRender = resetButton.GetComponent<MeshRenderer>();
        resetButtonMeshRender.sharedMaterial = yellowMaterial;
        
        ConfigureModSettings();

        resetButton.AddInteractionPunch(.1f);
        resetButton.OnInteract += delegate ()
        {
            ResetGrid();
            return false;
        };
    }

    void Start()
    {
        PuzzleGenerator.Size = size;
        PuzzleGenerator.DistinctRows = distinctRows;
        solution = PuzzleGenerator.GenerateSolution();
        givenPuzzle = PuzzleGenerator.ReduceConstraints(solution);

        //todo change the solution so it shows the same edges as the given puzzle
        for (int i = 0; i < size * size; i++)
        {
            if (givenPuzzle.Cells[i].Left != null)
            {
                solution.Cells[i].Left.Constraint = givenPuzzle.Cells[i].Left.Constraint;
                
            }
            if (givenPuzzle.Cells[i].Up != null)
            {
                solution.Cells[i].Up.Constraint = givenPuzzle.Cells[i].Up.Constraint;
                
            }
            
        }

        
        ConfigurePuzzleGrid();
        ResetGrid();


        //Make the button interactable
        KMSelectable parent = GetComponent<KMSelectable>();
        

        foreach (var button in buttons)
        {
            
            button.Selectable.Parent = parent;
            

            button.Selectable.AddInteractionPunch(.1f);
            button.Selectable.OnInteract += delegate ()
            {
                ButtonPress(button);
                return false;
            };
        }

        parent.Children = parent.Children.Concat(buttons.Select(b => b.Selectable)).ToArray();
        parent.UpdateChildrenProperly();
        

        Log("Puzzle Settings");
        Log($"Size: {size}");
        Log($"Distinct Rows: {distinctRows}");
        Log("Solution\n" + solution.ToString());
        Log("Given\n" + givenPuzzle.ToString());
    }

    private void ConfigureModSettings()
    {
        var modConfig = new ModConfig<BinaryPuzzlePlusSettings>("BinaryPuzzlePlusSettings");
        
        var defaultSetings = new BinaryPuzzlePlusSettings();
        

        modSettings = modConfig.Settings;
        
        modConfig.Settings = modSettings;
        
        Log($"Given mod settings\nSize: {modSettings.Size}\nDistinctRows: {modSettings.DistinctRows}");

        //Validate the values
        if (modSettings.Size % 2 != 0 || (modSettings.Size < 6 && modSettings.Size > 10))
        {
            Log($"Invalid mod setting size. Recieved: {modSettings.Size}. Using default value of {defaultSetings.Size}");
            size = defaultSetings.Size;
        }

        else
        {
            
            size = modConfig.Settings.Size;
        }

        distinctRows = modConfig.Settings.DistinctRows;
        
    }

    /// <summary>
    /// Instantiates buttons in a grid based on the size variables
    /// </summary>
    private void ConfigurePuzzleGrid()
    {
        const float scaleFactor = 1.18f;
        buttons = new List<Button>();

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


        //for each left edge, instantiate an edge prefab in the middle of the two buttons
        for (int ix = 0; ix < buttons.Count; ix++)
        {
            int row = ix / size;
            int col = ix % size;

            Button button = buttons[ix];
            button.Cell = givenPuzzle.Cells[ix];

            if (col != 0 && givenPuzzle.Cells[ix].Left != null && givenPuzzle.Cells[ix].Left.Constraint != EdgeState.None)
            {
                Button leftButton = buttons[ix - 1];
                GameObject edge = GetNewEdge(leftButton, button, xScale, zScale, false);
                SpriteRenderer sprite = edge.transform.Find("Sprite").GetComponent<SpriteRenderer>();
                sprite.sprite = button.Cell.Left.Constraint == EdgeState.Equal ? EqualSprite : XSprite;
            }

            if (row != 0 && givenPuzzle.Cells[ix].Up != null && givenPuzzle.Cells[ix].Up.Constraint != EdgeState.None)
            {
                Button upButton = buttons[ix - size];
                GameObject edge = GetNewEdge(button, upButton, xScale, zScale, true);
                SpriteRenderer sprite = edge.transform.Find("Sprite").GetComponent<SpriteRenderer>();
                sprite.sprite = button.Cell.Up.Constraint == EdgeState.Equal ? EqualSprite : XSprite;

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

            //change the background color
            b.MeshRenderer.sharedMaterial = c.state == CellState.Empty ? defaultMaterial : c.state == CellState.One ? yellowMaterial : blueMaterial;
            

            //change the text
            b.Label.text = c.state == CellState.Empty ? "" : c.state == CellState.One ? "1" : "0";
        }
    }

    private void ButtonPress(Button button)
    {
        //if the cell is given, don't do anything
        int buttonIndex = buttons.IndexOf(button);
        if (moduleSolved || givenPuzzle.Cells[buttonIndex].state != CellState.Empty)
            return;

        //cycle through the states
        button.Cell.state = (CellState)(((int)button.Cell.state + 1) % 3);
        currentState.Cells[buttonIndex].state = button.Cell.state;
        UpdateVisuals(currentState);

        //Check if the puzzle is solved
        if (IsSolved())
        {
            Solve();
        }
    }

    private void ResetGrid()
    {
        if (moduleSolved)
            return;
        currentState = new PuzzleState(givenPuzzle);

        for (int i = 0; i < size * size; i++)
        {
            Cell puzzleCell = currentState.Cells[i];
            //update the cells to match the buttons
            buttons[i].Cell = puzzleCell;
        }
        UpdateVisuals(currentState);
    }

    private bool IsSolved()
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
        resetButtonMeshRender.sharedMaterial = blueMaterial;
        moduleSolved = true;
        UpdateVisuals(currentState);
    }

    private void Log(string message)
    {
        Debug.Log($"[Binary Puzzle Plus #{ModuleId}] {message}");
    }

    
}