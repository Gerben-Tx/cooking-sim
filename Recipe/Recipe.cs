using System;
using System.Collections.Generic;
using System.Linq;
using CookingSim.Features.MergeableObjectFeature;
using Godot;

namespace CookingSim.Recipe;

[GlobalClass] // Makes the class available to the editor under "Create new resource"
[Tool] // Allows using this resource in the inspector (for adding it to a Recipe[] for example)
public partial class Recipe : Resource {
    [Export] public Ingredient[] Ingredients { get; set; }

    public bool ContainsIngredient(
        string sceneFilePath
    ) {
        return Ingredients.ToList().Any(ingredient => ingredient.Scene.GetPath() == sceneFilePath);
    }

    public bool IsCompleted() {
        // Check AND group
        IEnumerable<Ingredient> ingredientsInAndGroup = Ingredients.Where(ingredient => ingredient.Group.Type == IngredientGroupType.And);
        bool andGroupCompleted = true;
        foreach (Ingredient ingredient in ingredientsInAndGroup) {
            // All ingredients in the group have to be completed
            bool groupCompleted = GetIngredientsByGroupId(ingredient.Group.Id).All(x => x.IsCompleted);
            if (groupCompleted) continue;
            
            andGroupCompleted = false;
            break;
        }
        
        // Check AND/OR group
        IEnumerable<Ingredient> ingredientsInAndOrGroup = Ingredients.Where(ingredient => ingredient.Group.Type == IngredientGroupType.AndOr);
        bool andOrGroupCompleted = true;
        foreach (Ingredient ingredient in ingredientsInAndOrGroup) {
            // At least one ingredient in the group has to be completed
            bool groupCompleted = GetIngredientsByGroupId(ingredient.Group.Id).Any(x => x.IsCompleted);
            if (groupCompleted) continue;
            
            andOrGroupCompleted = false;
            break;       
        }
        
        // Check OR group
        IEnumerable<Ingredient> ingredientsInOrGroup = Ingredients.Where(ingredient => ingredient.Group.Type == IngredientGroupType.Or);
        bool orGroupCompleted = true;
        foreach (Ingredient ingredient in ingredientsInOrGroup) {
            // Only one ingredient in the group has to be completed
            bool groupCompleted = GetIngredientsByGroupId(ingredient.Group.Id).Count(x => x.IsCompleted) == 1;
            if (groupCompleted) continue;
            
            orGroupCompleted = false;
            break;      
        }
        
        return andGroupCompleted && andOrGroupCompleted && orGroupCompleted;
    }

    private List<Ingredient> GetIngredientsByGroupId(
        int groupId
    ) {
        return Ingredients.Where(ingredient => ingredient.Group.Id == groupId).ToList();
    }
}