module Types

open Shared

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
    | EditRecipe
