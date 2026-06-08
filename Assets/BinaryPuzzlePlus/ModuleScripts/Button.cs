using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button {

    public KMSelectable Selectable { get; } //script that handles the interactivity of the button
    public MeshRenderer MeshRenderer { get; } //needed to change the material (color) of the button
    public TextMesh Label { get; } //text on the button
    public Cell Cell { get; }
    public Button(KMSelectable selectable, MeshRenderer meshRenderer, TextMesh label)
    {
        Selectable = selectable;
        MeshRenderer = meshRenderer;
        Label = label;
    }
}