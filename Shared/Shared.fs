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
  | "Kg"  -> Some Kg
  | "G"   -> Some G
  | "Mg"  -> Some Mg
  | "L"   -> Some L
  | "Dl"  -> Some Dl
  | "Ml"  -> Some Ml
  | "Ms"  -> Some Ms
  | "Ss"  -> Some Ss
  | "Ts"  -> Some Ts
  | "Stk" -> Some Stk
  | _     -> None

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
    | "Frokost" -> Some Breakfast
    | "Lunsj" -> Some Lunch
    | "Middag" -> Some Dinner
    | "Dessert" -> Some Desert
    | _ -> None
    
let stringToMeal name =
    match name with
    | "Breakfast" -> Some Breakfast
    | "Lunch" -> Some Lunch
    | "Dinner" -> Some Dinner
    | "Desert" -> Some Desert
    | _ -> None