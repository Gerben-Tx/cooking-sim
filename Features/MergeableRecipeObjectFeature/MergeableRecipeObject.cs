using System.Collections.Generic;
using System.Linq;
using CookingSim.Features.MergeableObjectFeature;
using CookingSim.Recipe;
using Godot;

namespace CookingSim.Features.MergeableRecipeObjectFeature;

public partial class MergeableRecipeObject : MergeableBase, IFeature {
    public static string FeatureName => "MergeableRecipeObjectFeature";

    private List<Recipe.Recipe> _recipesImPartOf;
    private List<Recipe.Recipe> _recipesBodyIsPartOf;

    public override void _Ready() {
        base._Ready();

        _recipesImPartOf = RecipeManager.Instance.FindRecipesByStartingIngredient(Owner.GetSceneFilePath()).ToList();
        // Mark myself (as ingredient) as completed in all recipes where I am a part of
        RecipeManager.Instance.FindIngredientsBySceneFilePathInRecipes(
                _recipesImPartOf.ToArray(),
                Owner.GetSceneFilePath())
            .ToList()
            .ForEach(ingredient => ingredient.MarkAdded());
    }

    protected override bool IsMergingAllowed(
        Node3D body
    ) {
        bool baseResult = base.IsMergingAllowed(body);
        if (!baseResult) {
            return false;
        }

        _recipesBodyIsPartOf = _recipesImPartOf.Where(recipe => recipe.ContainsIngredient(body.GetSceneFilePath()))
            .ToList();
        if (_recipesBodyIsPartOf.Count == 0) {
            // This object is not allowed to merge with
            return false;
        }

        return true;
    }

    protected override void Merge(
        Node3D body
    ) {
        bool continueMerge = true;
        
        // Mark the ingredient as completed in all recipes where body is a part of
        foreach (Recipe.Recipe recipe in _recipesBodyIsPartOf) {
            Ingredient bodyAsIngredient = recipe.Ingredients.First(i => i.Scene.GetPath() == body.GetSceneFilePath());
            bodyAsIngredient.MarkAdded();

            bool recipeCompleted = recipe.IsCompleted();
            
            if (bodyAsIngredient.Position == IngredientPosition.End && !recipeCompleted) {
                // If the ingredient is the last ingredient in the recipe
                // and it will not complete the recipe, mark it as removed
                // and we will ignore the merge request
                bodyAsIngredient.MarkRemoved();
                continueMerge = false;
            } else if (recipeCompleted) {
                GD.Print("Recipe completed!");
            }
        }

        if (!continueMerge) {
            return;
        }
        
        base.Merge(body);
    }
}