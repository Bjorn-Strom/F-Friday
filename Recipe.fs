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
    { Amount: float
      Measurement: Measurement
      Name: string }

let ingredient amount measurement name = 
    { Amount = amount
      Measurement = measurement 
      Name = name }

type Meal =
    | Breakfast
    | Lunch
    | Dinner
    | Desert

type Time =
    | Minutes of int
    | Hours of float

type Portions = Portions of int
type Recipe =
    { Id: System.Guid
      Title: string
      Description: string
      Meal: Meal
      Time: Time
      Steps: string list
      Ingredients: Ingredient list
      Portions: Portions }

let createRecipe meal title description time steps ingredients portions =
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
     Dinner
     "Kokt potet"
     "En skikkelig, potensielt smakløs, klassiker som du som inngår i ganske mange andre retter."
     (Minutes 20)
     [ "Skrubb og skyll potetene"
       "Del potetene i 2"
       "Kok dem i 10-15 minutter til de er gjennomkokte" ]
     [ ingredient 800. G "Potet"
       ingredient 1. L "Vann"
       ingredient 1. Ts "Salt" ]
     (Portions 4))

fakabase.AddRecipe 
  (createRecipe
    Dinner
    "Pytt i panne"
    "Det evige hvilestedet til gamle middager."
    (Minutes 20)
    [ "Stek baconet og del det inn i biter"
      "Del potetene og inn i terninger og hakk løk."
      "Stek poteten og løken sammen i bacon-fettet"
      "Bland inn baconet"
      "Del paprikaen i biter og dryss over"]
    [ ingredient 2. Stk "Bacon"
      ingredient 4. Stk "Kokte poteter"
      ingredient 1. Stk "Løk"
      ingredient 0.25 Stk "Paprika"]
    (Portions 2))

let getAllRecipes () = fakabase.GetRecipes ()
let addRecipe newRecipe =
    fakabase.AddRecipe newRecipe
let updateRecipe recipeToUpdate =
    fakabase.UpdateRecipe recipeToUpdate
let deleteRecipe id =
    fakabase.DeleteRecipe id