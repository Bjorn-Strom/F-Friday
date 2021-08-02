module Store

open Feliz

open Types
open Shared

type StoreAction =
    | SetRecipes of Recipe list RemoteData
    | SetCurrentView of View

type Store =
    { Recipes: Recipe list RemoteData
      View: View }

let initialStore =
    { Recipes = Fetching
      View = Home }

let StoreReducer state action =
    match action with
    | SetRecipes recipes -> { state with Recipes = recipes }
    | SetCurrentView view -> { state with View = view }

let storeContext = React.createContext()

[<ReactComponent>]
let StoreProvider children =
    let (state, dispatch) = React.useReducer(StoreReducer, initialStore)
    React.contextProvider(storeContext, (state, dispatch), React.fragment [children])

[<Hook>]
let useStore() =
   React.useContext(storeContext)