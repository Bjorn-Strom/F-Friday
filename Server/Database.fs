module Database

open Types
open Shared

type Fakabase () =
    let recipes = System.Collections.Generic.Dictionary<System.Guid, RecipeDbModel * IngredientDbModel list>()

    member __.GetRecipes () =
        List.ofSeq recipes.Values
        
    member __.AddRecipe (newRecipe: RecipeDbModel) (ingredients: IngredientDbModel list) = 
        recipes.Add(newRecipe.Id, (newRecipe, ingredients))
        List.ofSeq recipes.Values

    member __.UpdateRecipe (recipeToUpdate: RecipeDbModel) (ingredients: IngredientDbModel list) =
        recipes.[recipeToUpdate.Id] <- (recipeToUpdate, ingredients)
        recipes.[recipeToUpdate.Id]

    member __.DeleteRecipe (recipeId: System.Guid) =
        recipes.Remove(recipeId) |> ignore


let fakabase = Fakabase()
let newGuid = System.Guid.NewGuid()
fakabase.AddRecipe
    {
        Id = newGuid
        Title = "Kokt potet"
        Description = "En skikkelig, potensielt smakløs, klassiker som du som inngår i ganske mange andre retter."
        Meal = Dinner
        Time = 20.
        Steps =
            [| "Skrubb og skyll potetene"
               "Del potetene i 2"
               "Kok dem i 10-15 minutter til de er gjennomkokte" |]
        Portions = 4
        }
    [
        { Volume = 800.; Measurement = G; Name = "Potet"; RecipeId = newGuid  }
        { Volume = 1.; Measurement = L; Name = "Vann"; RecipeId = newGuid  }
        { Volume = 800.; Measurement = Ts; Name = "Salt"; RecipeId = newGuid  }
    ] |> ignore
     
let newGuid2 = System.Guid.NewGuid()
fakabase.AddRecipe
    {
        Id = newGuid2
        Title = "Koteletter med kokt potet"
        Description = "Oi oi oi så hærlig"
        Meal = Dinner
        Time = 5.
        Steps =
            [|  "Grill kotelettene i 3-4 minutt på hver side, dryss med salt og pepper" |] 
        Portions = 4
        }
    [
        { Volume = 1.; Measurement = Ts; Name = "Salt"; RecipeId = newGuid2  }
        { Volume = 0.5; Measurement = Ts; Name = "Pepper"; RecipeId = newGuid2  }
        { Volume = 4.; Measurement = Stk; Name = "Kotelett"; RecipeId = newGuid2  }
    ] |> ignore

let getAllRecipes () = fakabase.GetRecipes ()
let addRecipe recipe ingredients =
    fakabase.AddRecipe recipe ingredients
let updateRecipe recipe ingredients =
    fakabase.UpdateRecipe recipe ingredients
let deleteRecipe id =
    fakabase.DeleteRecipe id
    