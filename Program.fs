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