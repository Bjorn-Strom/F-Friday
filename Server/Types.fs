module Types

open Thoth.Json.Net

open Shared

// Helpers
let private validateStringLength (name: string) maxLength (toValidate: string) =
    if toValidate.Length = 0 then
        Decode.fail $"\"{name}\" må være lenger enn 0 tegn"
    else if toValidate.Length > maxLength then
        Decode.fail $"{name} kan ikke være lenger enn {maxLength} tegn"
    else
        Decode.succeed toValidate

type IngredientDomainModel =
    { Volume: float
      Measurement: Measurement
      Name: string
      Recipe: System.Guid }

type RecipeDomainModel =
    { Id: System.Guid
      Title: string
      Description: string
      Meal: Meal
      Time: float
      Steps: string array
      Portions: int }

[<CLIMutable>]
type RecipeDbModel =
    { Id: string
      Title: string
      Description: string
      Meal: string
      Time: float
      Steps: string array
      Portions: int }

[<CLIMutable>]
type IngredientDbModel =
    { Volume: float
      Measurement: string
      Name: string
      Recipe: string }

module RecipeDomainModel =
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

    let decoder: Decoder<RecipeDomainModel> =
        Decode.object
            (fun get ->
                { Id = get.Required.Field "id" Decode.guid
                  Title =
                      get.Required.Field
                          "title"
                          (Decode.string
                           |> Decode.andThen (validateStringLength "title" 64))
                  Description = get.Required.Field "description" Decode.string
                  Meal = get.Required.Field "meal" (Decode.string |> Decode.andThen validateMeal)
                  Time = get.Required.Field "time" Decode.float
                  Steps =
                      get.Required.Field
                          "steps"
                          (Decode.array Decode.string
                           |> Decode.andThen validateStep)
                  Portions = get.Required.Field "portions" (Decode.int |> Decode.andThen validatePortion) })

    let toDbModel (domain: RecipeDomainModel) : RecipeDbModel =
        { Id = domain.Id.ToString()
          Title = domain.Title
          Description = domain.Description
          Meal = domain.Meal.ToString()
          Time = domain.Time
          Steps = domain.Steps
          Portions = domain.Portions }

module IngredientDomainModel =
    let private validateMeasurement (measurement: string) =
        match stringToMeasurement measurement with
        | Some measurement -> Decode.succeed measurement
        | None -> Decode.fail $"{measurement} er ikke en gyldig målenhet."

    let decoder: Decoder<IngredientDomainModel> =
        Decode.object
            (fun get ->
                { Volume = get.Required.Field "volume" Decode.float
                  Measurement =
                      get.Required.Field
                          "measurement"
                          (Decode.string
                           |> Decode.andThen validateMeasurement)
                  Name =
                      get.Required.Field
                          "name"
                          (Decode.string
                           |> Decode.andThen (validateStringLength "name" 32))
                  Recipe = get.Required.Field "recipeId" Decode.guid })

    let decodeList: Decoder<IngredientDomainModel list> =
        Decode.object (fun get -> get.Required.Field "ingredients" (Decode.list decoder))

    let encoder (ingredient: IngredientDbModel) =
        Encode.object [ "volume", Encode.float ingredient.Volume
                        "measurement", Encode.string (ingredient.Measurement.ToString())
                        "name", Encode.string ingredient.Name
                        "recipeId", Encode.string ingredient.Recipe ]

    let toDbModel (ingredient: IngredientDomainModel) : IngredientDbModel =
        { Volume = ingredient.Volume
          Measurement = ingredient.Measurement.ToString()
          Name = ingredient.Name
          Recipe = ingredient.Recipe.ToString() }

let encodeRecipeAndIngredient (recipe: RecipeDbModel, ingredients: IngredientDbModel seq) =
    Encode.object [ "id", Encode.string recipe.Id
                    "title", Encode.string recipe.Title
                    "description", Encode.string recipe.Description
                    "meal", Encode.string (recipe.Meal.ToString())
                    "time", Encode.float recipe.Time
                    "steps", recipe.Steps
                             |> Array.map Encode.string
                             |> Encode.array
                    "portions", Encode.int recipe.Portions
                    "ingredients", ingredients
                                   |> Seq.map IngredientDomainModel.encoder
                                   |> Encode.seq
                ]

module RecipeDbModel =
    let toDomain (dbModel: RecipeDbModel) : RecipeDomainModel =
        { Id = System.Guid.Parse dbModel.Id
          Title = dbModel.Title
          Description = dbModel.Description
          Meal =
              stringToMeal dbModel.Meal
              |> Option.defaultValue Breakfast
          Time = dbModel.Time
          Steps = dbModel.Steps
          Portions = dbModel.Portions }

module IngredientDbModel =
    let toDomain (dbModel: IngredientDbModel) : IngredientDomainModel =
        { Volume = dbModel.Volume
          Measurement =
              stringToMeasurement dbModel.Measurement
              |> Option.defaultValue Stk
          Name = dbModel.Name
          Recipe = System.Guid.Parse dbModel.Recipe }

let recipeToDbModels (recipe: RecipeDomainModel) (ingredients: IngredientDomainModel list) =
    let recipeToInsert = RecipeDomainModel.toDbModel recipe

    let ingredientsToInsert =
        ingredients
        |> List.map (fun i -> IngredientDomainModel.toDbModel i)

    recipeToInsert, ingredientsToInsert
