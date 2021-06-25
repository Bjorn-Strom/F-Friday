module Client

open Feliz
open Fetch
open Thoth.Json

open Shared

// Banner: Tittel: Frokost - Lunsj - Middag - Dessert - Min Handleliste | SØK |
// Hovedside:
// Uten valg: Tilfeldig oppskrift
// Med valg: Liste over oppskrifter
// Med oppskrift valgt: Se den oppskriften
// 

[<ReactComponent>]
let Menu() =
    Html.div [
        prop.text "Foo"
    ]

[<ReactComponent>]
let App() =
    let (recipes, setRecipes) = React.useState<Result<Recipe list, string>>(Ok [])

    React.useEffect((fun () -> 
        fetch "http://localhost:5000/api/recipes" []
        |> Promise.bind (fun result -> result.text())
        |> Promise.map (fun result -> Decode.Auto.fromString<Recipe list>(result, caseStrategy=CamelCase))
        |> Promise.map setRecipes
        |> Promise.start)
        , [| |])

    let recipes = 
        match recipes with 
            | Ok recipes -> 
                List.map (fun r -> 
                    Html.div [
                        prop.text r.Title
                    ]
                ) recipes
            | Error e -> [ Html.h1 e ]
    
    Html.div recipes

open Browser.Dom

ReactDOM.render(App(), document.getElementById "root")