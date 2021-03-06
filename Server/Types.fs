module Types

open Shared
open Thoth.Json.Net

// Helpers
let private validateStringLength (name: string) maxLength (toValidate: string)  =
    if toValidate.Length = 0 then
        Decode.fail $"\"{name}\" må være lenger enn 0 tegn"
    else if toValidate.Length > maxLength then
        Decode.fail $"{name} kan ikke være lenger enn {maxLength} tegn"
    else
        Decode.succeed toValidate
    
type IngredientDbModel =
    { Volume: float
      Measurement: Measurement
      Name: string
      RecipeId: System.Guid }
    
module IngredientDbModel =
    let private validateMeasurement (measurement: string) =
        match stringToMeasurement measurement with
        | Some measurement -> Decode.succeed measurement
        | None -> Decode.fail $"{measurement} er ikke en gyldig målenhet."
        
    let decoder: Decoder<IngredientDbModel> =
        Decode.object (fun get ->
        {
            Volume = get.Required.Field "volume" Decode.float
            Measurement = get.Required.Field "measurement"
                              (Decode.string
                               |> Decode.andThen validateMeasurement)
            Name = get.Required.Field "name"
                       (Decode.string
                        |> Decode.andThen(validateStringLength "name" 32))
            RecipeId = get.Required.Field "recipeId" Decode.guid
        })
    let decodeList: Decoder<IngredientDbModel list> =
        Decode.object (fun get -> get.Required.Field "ingredients" (Decode.list decoder))
            
    let encoder ingredient = 
       Encode.object [
            "volume", Encode.float ingredient.Volume
            "measurement", Encode.string (ingredient.Measurement.ToString())
            "name", Encode.string ingredient.Name
            "recipeId", Encode.guid ingredient.RecipeId
        ]
        
type RecipeDbModel =
    { Id: System.Guid
      Title: string
      Description: string
      Meal: Meal
      Time: float
      Steps: string array
      Portions: int }
    
module RecipeDbModel =
    let private validateStep steps =
        if Array.isEmpty steps then
            Decode.fail "Oppskrifter trenger minst ett steg"
        else
            Decode.succeed steps
    let private validatePortion portion =
        if portion = 0 then
            Decode.fail "Oppskrifter må ha minst 1 porsjon"
        else
            Decode.succeed portion
    let private validateMeal (meal: string) =
        match stringToMeal meal with
        | Some meal -> Decode.succeed meal
        | None -> Decode.fail $"{meal} er ikke et gyldig måltid."

    let decoder: Decoder<RecipeDbModel> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.guid
                Title = get.Required.Field "title"
                            (Decode.string
                             |> Decode.andThen (validateStringLength "title" 64))
                Description = get.Required.Field "description" Decode.string
                Meal = get.Required.Field "meal"
                           (Decode.string
                            |> Decode.andThen validateMeal)
                Time = get.Required.Field "time" Decode.float
                Steps = get.Required.Field "steps"
                            (Decode.array Decode.string
                             |> Decode.andThen validateStep)
                Portions = get.Required.Field "portions"
                               (Decode.int
                                |> Decode.andThen validatePortion)
            })
        
let encodeRecipeAndIngredient (recipe, ingredients) =
    Encode.object [
        "id", Encode.guid recipe.Id
        "title", Encode.string recipe.Title
        "description", Encode.string recipe.Description
        "meal", Encode.string (recipe.Meal.ToString())
        "time", Encode.float recipe.Time
        "steps", recipe.Steps |> Array.map Encode.string |> Encode.array
        "portions", Encode.int recipe.Portions
        "ingredients",
            ingredients
            |> List.map IngredientDbModel.encoder
            |> Encode.list
    ]