using CookingSim.Features.MergeableObjectFeature;
using Godot;

namespace CookingSim.Recipe;

[GlobalClass]
[Tool]
public partial class IngredientGroup : Resource {
    [Export] public int Id { get; set; }
    [Export] public IngredientGroupType Type { get; set; }
}