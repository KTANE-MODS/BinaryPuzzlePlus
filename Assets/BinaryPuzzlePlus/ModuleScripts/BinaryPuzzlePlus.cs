using System.Collections.Generic;
using UnityEngine;

public class BinaryPuzzlePlus : MonoBehaviour
{

    [SerializeField]
    private GameObject buttonPrefab;
    [SerializeField]
    private KMSelectable resetButton;

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

    /// <summary>
    /// Instantiates buttons in a grid based on the size variables
    /// </summary>
    private void ConfigurePuzzleGrid()
    {
        buttons = new List<Button>();
        //instantiate the buttons base on the size

        //local starting/ending position of the puzzle grid
        Vector2 startingPosition = new Vector2(-0.074f, 0.0321f);
        Vector2 endingPosition = new Vector2(0.0789f, -0.0705f);

        //figure out the width and height of each button
        float desiredWidth = Mathf.Abs(endingPosition.x - startingPosition.x) / size;
        float desiredHeight = Mathf.Abs(endingPosition.y - startingPosition.y) / size;

        //find what to scale the buttons to in order to have the wanted width/height
        float currentWidh = buttonPrefab.GetComponent<Renderer>().bounds.size.x;
        float currentHeight = buttonPrefab.GetComponent<Renderer>().bounds.size.z;

        float xScale = (desiredWidth * buttonPrefab.transform.localScale.x) / currentWidh;
        float zScale = (desiredWidth * buttonPrefab.transform.localScale.z) / currentHeight;

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
    }

    void Awake()
    {
        ModuleId = ModuleIdCounter++;
    }

    void Start()
    {
        ConfigurePuzzleGrid();
        PuzzleGenerator.LongGeneration = longGeneration;
        PuzzleGenerator.Size = size;
        PuzzleGenerator.DistinctRows = distinctRows;
        PuzzleGenerator.ConstraintRemovalPercentage = constraintRemovalPercentage;

        solution = PuzzleGenerator.GeneratePuzzle();
        Debug.Log(solution.ToString());
    }

    void Update()
    {

    }

    private void Log(string message)
    {
        Debug.Log($"[Binary Puzzle Plus #{ModuleId}] {message}");
    }








}