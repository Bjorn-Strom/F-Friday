module Shared

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

let measurementList = [ Kg; G; Mg; L; Dl; Ml; Ms; Ss; Ts; Stk ]

let measurementToString =
  function
  | Kg  -> "Kg"
  | G   -> "G"
  | Mg  -> "Mg"
  | L   -> "L"
  | Dl  -> "Dl"
  | Ml  ->"Ml"
  | Ms  -> "Ms"
  | Ss  -> "Ss"
  | Ts  -> "Ts"
  | Stk -> "Stk"

let stringToMeasurement =
  function
  | "Kg"  -> Kg
  | "G"   -> G
  | "Mg"  -> Mg
  | "L"   -> L
  | "Dl"  -> Dl
  | "Ml"  -> Ml
  | "Ms"  -> Ms
  | "Ss"  -> Ss
  | "Ts"  -> Ts
  | "Stk" -> Stk
  | _     -> Stk

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

let mealList =
  [ Breakfast
    Lunch
    Dinner
    Desert ]

let mealToNorwegian meal =
  match meal with
  | Breakfast -> "Frokost"
  | Lunch -> "Lunsj"
  | Dinner -> "Middag"
  | Desert -> "Dessert"

let norwegianToMeal name =
    match name with
    | "Frokost" -> Breakfast
    | "Lunsj" -> Lunch
    | "Middag" -> Dinner
    | "Dessert" -> Desert
    | _ -> Dinner

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

[<RequireQualifiedAccess>]
module List =
  let replaceIndex index newItem list =
    List.mapi (fun currentIndex oldItem -> if currentIndex = index then newItem else oldItem) list