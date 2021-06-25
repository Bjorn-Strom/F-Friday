module Client

open Feliz
open Fetch
open Thoth.Json
open Fss
open Fss.Feliz

open Shared

// Banner: Tittel: Frokost - Lunsj - Middag - Dessert - Min Handleliste | SØK |
// Hovedside:
// Uten valg: Tilfeldig oppskrift
// Med valg: Liste over oppskrifter
// Med oppskrift valgt: Se den oppskriften
// 

type RemoteData<'t> =
    | Fetching
    | Data of 't
    | Failure of string

// Fonts
let headingFont = FontFamily.custom "Nunito"
let textFont = FontFamily.custom "Raleway"

type ButtonColor =
    | Transparent

[<ReactComponent>]
let Button (text: string) onClick color =
    Html.button [
        prop.text text
        prop.onClick onClick
        prop.fss [
            Border.none
            textFont
            FontSize' (px 18)
            headingFont
            match color with
            | Transparent ->
                BackgroundColor.transparent
            Hover [
                Cursor.pointer
                Color.blue
            ]

        ]
    ]

[<ReactComponent>]
let SearchBar () =
    let (searchTerm, setSearchTerm) = React.useState ""
    Html.input [
        prop.type' "text"
        prop.value searchTerm
        prop.onChange setSearchTerm
        prop.placeholder "Søk etter oppskrift"
        prop.fss [
            Height' (pct 100)
            textFont
            BoxSizing.borderBox
            Padding' (px 5)
            PaddingLeft' (px 20)
            BorderRadius' (px 10)
            Border.none
        ]
    ]

[<ReactComponent>]
let Menu() =
    Html.nav [
        prop.fss [
            BackgroundColor.green
            Width' (vw 100.)
            Height' (px 70)
            MarginLeft' (px -8)
            MarginTop' (px -10)
            Display.flex
            JustifyContent.center
        ]
        prop.children [
            Html.div [
                prop.fss [
                    Display.flex
                    FlexDirection.row
                    AlignItems.center
                    JustifyContent.spaceBetween
                    Width' (vw 50.)
                ]
                prop.children [
                    Html.h1 [
                        prop.fss [ headingFont ]
                        prop.text "Slafs!" 
                    ]
                    Html.div [
                        SearchBar ()
                        Button "Frokost" (fun _ -> ()) Transparent
                        Button "Lunsj" (fun _ -> ()) Transparent
                        Button "Middag" (fun _ -> ()) Transparent
                        Button "Dessert" (fun _ -> ()) Transparent
                        Button "Min handleliste" (fun _ -> ()) Transparent
                    ]
                ]
            ]
        ]
    ]

let selectIngredients recipe =
    recipe.Ingredients
    |> List.map (fun i -> $"{i.Volume} {i.Measurement} {i.Name}")

let stepCounter = counterStyle [ ]


[<ReactComponent>]
let Recipe recipe =
    Html.article [
        prop.fss [
            Width' (vw 50.)
            Display.flex
            FlexDirection.column
        ]
        prop.children [
            Html.div [
                Html.h1 [
                    prop.fss [ 
                        headingFont
                        JustifyContent.center
                    ]
                    prop.text recipe.Title
                ]
                Html.p [
                    prop.fss [ headingFont ]
                    prop.text $"{mealToNorwegian recipe.Meal} på {recipe.Time} minutter."
                ]
            ]
            Html.p [
                prop.fss [ textFont ]
                prop.text recipe.Description
            ]
            Html.div [
                prop.fss [
                    Display.grid
                    GridTemplateColumns.values [ fr 0.5; fr 1.5]
                    GridColumnGap' (px 20)
                ]
                prop.children [
                    Html.div [
                        prop.children [
                            Html.h3 [
                                prop.fss [ headingFont ]
                                prop.text "Ingredienser"
                            ]
                            Html.p $"For {recipe.Portions} porsjoner: "
                            yield! (selectIngredients recipe
                                    |> List.map (fun i ->
                                        Html.div [
                                            prop.fss [ TextTransform.lowercase ]
                                            prop.text i
                                        ]))
                        ]
                    ]
                    Html.div [
                        prop.children [
                            Html.h3 [
                                prop.fss [ headingFont ]
                                prop.text "Steg"
                            ]
                            yield! (recipe.Steps
                                    |> List.map (fun s ->
                                        Html.p [
                                            prop.fss [
                                                CounterIncrement.increment stepCounter
                                                Before [
                                                    Content.counter (stepCounter, ". ")
                                                ]
                                            ]
                                            prop.text s
                                        ]))
                        ]
                    ]
                ]
            ]
        ]
    ]

[<ReactComponent>]
let Container (recipes: Recipe list) =
    let (currentRecipe, setCurrentRecipe) = React.useState<Recipe> (List.head recipes)
    Html.div [
        prop.fss [
            Display.flex
            FlexDirection.column
            AlignItems.center
        ]

        prop.children [
            Menu()
            Recipe currentRecipe
        ]
    ]

[<ReactComponent>]
let App() =
    let (recipes, setRecipes) = React.useState<Recipe list RemoteData>(Fetching)

    React.useEffect((fun () -> 
        fetch "http://localhost:5000/api/recipes" []
        |> Promise.bind (fun result -> result.text())
        |> Promise.map (fun result -> Decode.Auto.fromString<Recipe list>(result, caseStrategy=CamelCase))
        |> Promise.map (fun result -> 
            match result with
            | Ok recipes -> Data recipes
            | Error e -> Failure e)
        |> Promise.map setRecipes
        |> Promise.start)
        , [| |])

    match recipes with 
    | Fetching -> Html.div [
        prop.text "Laster..."
        ]
    | Data recipes -> Container recipes
    | Failure e -> Html.div [
        prop.fss [ textFont ]
        prop.text $"En feil skjedde under henting av oppskrifter: {e}"
    ]

open Browser.Dom

ReactDOM.render(App(), document.getElementById "root")