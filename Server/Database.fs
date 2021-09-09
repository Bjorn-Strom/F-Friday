module Database

open Shared

// Alt dette skal vi erstatte med en database senere

type Fakabase () =
    let recipes = System.Collections.Generic.Dictionary<System.Guid, Recipe>()

    member __.GetRecipes () =
        List.ofSeq recipes.Values
        
    member __.AddRecipe (newRecipe: Recipe) = 
        recipes.Add(newRecipe.Id, newRecipe)

    member __.UpdateRecipe (recipeToUpdate: Recipe) =
        recipes.[recipeToUpdate.Id] <- recipeToUpdate

    member __.DeleteRecipe (recipeId: System.Guid) =
        recipes.Remove(recipeId) |> ignore


let fakabase = Fakabase()
fakabase.AddRecipe 
    (createRecipe 
         "Kokt potet"
         "En skikkelig, potensielt smakløs, klassiker som du som inngår i ganske mange andre retter."
         Dinner
         20.
         [ "Skrubb og skyll potetene"
           "Del potetene i 2"
           "Kok dem i 10-15 minutter til de er gjennomkokte" ]
         [ ingredient 800. G "Potet"
           ingredient 1. L "Vann"
           ingredient 1. Ts "Salt" ]
         4)
fakabase.AddRecipe 
        (createRecipe
            "Koteletter med kokt potet"
            "Oi oi oi så hærlig"
            Dinner
            5.
            [  "Grill kotelettene i 3-4 minutt på hver side, dryss med salt og pepper"
            ] 
            [ ingredient 1. Ts "Salt"; ingredient 0.5 Ts "Pepper"; ingredient 4. Stk "Kotelett" ]
            4)

let getAllRecipes () = fakabase.GetRecipes ()
let addRecipe newRecipe =
    fakabase.AddRecipe newRecipe
let updateRecipe recipeToUpdate =
    fakabase.UpdateRecipe recipeToUpdate
let deleteRecipe id =
    fakabase.DeleteRecipe id