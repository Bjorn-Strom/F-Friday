module Store

open Feliz

open Types
open Shared

type Store =
    { Recipes: Recipe list RemoteData
      View: View }
type StoreAction =
    | SetRecipes of Recipe list RemoteData
    | AddRecipe of Recipe
    | SetCurrentView of View

let StoreReducer state action =
    match action with
    | AddRecipe recipe ->
        let newRecipes =
            match state.Recipes with
            | Data recipes -> Data (recipe :: recipes)
            | _ -> Data [recipe]
        { state with Recipes = newRecipes }
    | SetRecipes recipes -> { state with Recipes = recipes }
    | SetCurrentView view -> { state with View = view }

let initialStore =
    { Recipes = Fetching
      View = Home }

let storeContext = React.createContext()

[<ReactComponent>]
let StoreProvider children =
    let (state, dispatch) = React.useReducer(StoreReducer, initialStore)
    React.contextProvider(storeContext, (state, dispatch), React.fragment [children])

[<Hook>]
let useStore() =
   React.useContext(storeContext)