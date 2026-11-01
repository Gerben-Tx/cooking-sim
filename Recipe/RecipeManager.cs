using System.Collections.Generic;
using System.Linq;
using Godot;

namespace CookingSim.Recipe;

public partial class RecipeManager : Node {
    public static RecipeManager Instance { get; private set; }
    [Export] public Recipe[] Recipes { get; private set; }

    public override void _Ready() {
        Instance = this;

        Recipes = [
            GD.Load<Recipe>("res://Recipe/Recipes/burger.tres"),
            GD.Load<Recipe>("res://Recipe/Recipes/hotdog.tres")
        ];
    }

    public IEnumerable<Recipe> FindRecipesByStartingIngredient(
        string sceneFilePath
    ) {
        return Recipes.ToList().Where(recipe =>
            recipe.Ingredients.Any(ingredient =>
                ingredient.Position == IngredientPosition.Start && ingredient.Scene.GetPath() == sceneFilePath
            )
        );
    }

    public IEnumerable<Ingredient> FindIngredientsBySceneFilePathInRecipes(Recipe[] recipes, string sceneFilePath) {
        List<Ingredient> ingredients = [];
        foreach (Recipe recipe in recipes) {
            foreach (Ingredient ingredient in recipe.Ingredients) {
                if (ingredient.Scene.GetPath() != sceneFilePath) {
                    continue;
                }

                ingredients.Add(ingredient);
            }
        }

        return ingredients.AsEnumerable();
    }
}