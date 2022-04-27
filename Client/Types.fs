module Types

open Shared

type Ingredient =
    { Volume: float
      Measurement: Measurement
      Name: string
      RecipeId: System.Guid }

let ingredient volume measurement name recipeId =
    { Volume = volume
      Measurement = measurement
      Name = name
      RecipeId = recipeId }

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

let createRecipe recipeId title description meal time steps ingredients portions =
    { Id = recipeId
      Title = title
      Description = description
      Meal = meal
      Time = time
      Steps = steps
      Ingredients = ingredients
      Portions = portions }

[<RequireQualifiedAccess>]
module List =
  let replaceIndex index newItem list =
    List.mapi (fun currentIndex oldItem -> if currentIndex = index then newItem else oldItem) list

type RemoteData<'t> =
    | Fetching
    | Data of 't
    | Failure of string

type View =
    | Home
    | RecipeDetails of Recipe
    | Breakfasts
    | Lunches
    | Dinners
    | Desserts
    | NewRecipe
