using CookingSim.Features.MergeableObjectFeature;
using Godot;

namespace CookingSim.Recipe;

[GlobalClass] // Makes the class available to the editor under "Create new resource"
[Tool] // Allows using this resource in the inspector (for adding it to an Ingredient[] for example)
public partial class Ingredient : Resource {
    [Export] public PackedScene Scene { get; set; }
    [Export] public IngredientGroup Group { get; set; }
    [Export] public IngredientPosition Position { get; set; } = IngredientPosition.Middle;
    [Export] public int Amount { get; set; } = 1;

    public bool IsCompleted { get; private set; }
    private int _currentAmount;

    public void MarkAdded() {
        _currentAmount += 1;
        IsCompleted = _currentAmount >= Amount;
    }
    
    public void MarkRemoved() {
        _currentAmount -= 1;
    }
}