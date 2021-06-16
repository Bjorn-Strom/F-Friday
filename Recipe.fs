module Recipe

type Measurement = 
    | Kg of float
    | G of float
    | Mg of float
    | L of float
    | Dl of float
    | Ml of float
    | Ms of float
    | Ss of float
    | Ts of float
    | Stk of float

type Ingredient =
    { Measurement: Measurement
      Name: string
    }

let ingredient volume measurement name = 
    { Measurement = measurement volume
      Name = name
    }

type Meal =
    | Breakfast
    | Lunch
    | Dinner
    | Desert

type Recipe =
    { Id: System.Guid
      Title: string
      Meal: Meal
      Time: float
      Steps: string list
      Ingredients: Ingredient list
      Portions: int
      SubRecipes: System.Guid list
    }

let createRecipe title meal time steps ingredients portions subRecipes =
    { Id = System.Guid.NewGuid()
      Title = title
      Meal = meal
      Time = time
      Steps = steps
      Ingredients = ingredients
      Portions = portions
      SubRecipes = subRecipes
    }

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
        recipes.Remove(recipeId)


let fakabase = Fakabase()
let koktPotet = (createRecipe 
           "Kokt potet"
            Dinner
            20.
            [ "Skrubb og skyll potetene"
              "Del potetene i 2"
              "Kok dem i 10-15 minutter til de er gjennomkokte"
            ]
            [ ingredient 800. G "Potet"; ingredient 1. L "Vann"; ingredient 1. Ts "Salt" ]
            4
            [])
fakabase.AddRecipe koktPotet
fakabase.AddRecipe (createRecipe
            "Koteletter med kokt potet"
            Dinner
            5.
            [  "Grill kotelettene i 3-4 minutt på hver side, dryss med salt og pepper"
            ] 
            [ ingredient 1. Ts "Salt"; ingredient 0.5 Ts "Pepper"; ingredient 4. Stk "Kotelett" ]
            4
            [ koktPotet.Id ])

let getAllRecipes () = fakabase.GetRecipes ()
let addRecipe newRecipe =
    fakabase.AddRecipe newRecipe
let updateRecipe recipeToUpdate =
    fakabase.UpdateRecipe recipeToUpdate
let deleteRecipe id =
    fakabase.DeleteRecipe id