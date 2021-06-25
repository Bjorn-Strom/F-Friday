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

type Ingredient =
    { Volume: float
      Measurement: Measurement
      Name: string
    }

let ingredient volume measurement name = 
    { Volume = volume
      Measurement = measurement 
      Name = name
    }

type Meal =
    | Breakfast
    | Lunch
    | Dinner
    | Desert

let mealToNorwegian meal =
  match meal with
  | Breakfast -> "Frokost"
  | Lunch -> "Lunsj"
  | Dinner -> "Middag"
  | Desert -> "Dessert"

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
      Portions = portions
    }