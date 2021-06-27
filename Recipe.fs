module Recipe

type Measurement = 
    | Kg
    | G 
    | Mg 
    | L 
    | Dl 
    | Ml
    | Ms 
    | Ss 
    | Ts 
    | Stk 

type Ingredient =
    { Volume: float
      Measurement: Measurement
      Name: string }

let ingredient volume measurement name = 
    { Volume = volume
      Measurement = measurement 
      Name = name }

type Meal =
    | Breakfast
    | Lunch
    | Dinner
    | Desert

type Recipe =
    { Id: System.Guid
      Title: string
      Description: string
      Meal: Meal
      Time: float
      Steps: string list
      Ingredients: Ingredient list
      Portions: int
    }

let createRecipe title description meal time steps ingredients portions =
    { Id = System.Guid.NewGuid()
      Title = title
      Description = description
      Meal = meal
      Time = time
      Steps = steps
      Ingredients = ingredients
      Portions = portions }


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
    "Pytt i panne"
    "Det evige hvilestedet til gamle middager."
    Dinner
    20.
    [ "Stek baconet og del det inn i biter"
      "Del potetene og inn i terninger og hakk løk."
      "Stek poteten og løken sammen i bacon-fettet"
      "Bland inn baconet"
      "Del paprikaen i biter og dryss over"]
    [ ingredient 2. Stk "Bacon"
      ingredient 4. Stk "Kokte poteter"
      ingredient 1. Stk "Løk"
      ingredient 0.25 Stk "Paprika"]
      2)

let getAllRecipes () = fakabase.GetRecipes ()
let addRecipe newRecipe =
    fakabase.AddRecipe newRecipe
let updateRecipe recipeToUpdate =
    fakabase.UpdateRecipe recipeToUpdate
let deleteRecipe id =
    fakabase.DeleteRecipe id