module Client

open Feliz
open Fetch
open Thoth.Json
open Fss
open Fss.Feliz

open Shared
open Types
open Store


// Fonts
let headingFont = FontFamily.custom "Nunito"
let textFont = FontFamily.custom "Raleway"

// Colors
let green = hex "1b5e20"
let greenLight = hex "4c8c4a"
let greenDark = hex "003300"
let white = hex "f5f5f6"

type ButtonColor =
    | Transparent
    | Green
    | Black
    | White

[<ReactComponent>]
let Button (text: string) onClick backgroundColor color =
    Html.button [
        prop.text text
        prop.onClick onClick
        prop.fss [
            Border.none
            Color' white
            FontSize' (px 18)
            headingFont
            Cursor.pointer
            match backgroundColor with
            | Transparent -> BackgroundColor.transparent
            | Green -> BackgroundColor' green
            | Black -> BackgroundColor.black
            | White -> BackgroundColor' white
            match color with
            | Transparent -> Color.transparent
            | Green -> Color' green
            | Black -> Color.black
            | White -> Color' white
            Hover [
                Color' greenDark
            ]

        ]
    ]

[<ReactComponent>]
let Menu () =
    let (_, dispatch) = useStore()
    Html.nav [
        prop.fss [
            BackgroundColor' green
            Width' (vw 100.)
            Height' (px 70)
            MarginLeft' (px -8)
            MarginTop' (px -10)
            Color' white
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
                    Html.h1 "Slafs!"
                    Html.div [
                        Button "+" (fun _ -> dispatch <| SetCurrentView NewRecipe) Transparent White
                        Button "Frokost" (fun _ -> dispatch <| SetCurrentView Breakfasts) Transparent White
                        Button "Lunsj" (fun _ -> dispatch <| SetCurrentView  Lunches) Transparent White
                        Button "Middag" (fun _ -> dispatch <| SetCurrentView  Dinners) Transparent White
                        Button "Dessert" (fun _ -> dispatch <| SetCurrentView  Desserts) Transparent White
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
                    prop.fss [ JustifyContent.center ]
                    prop.text recipe.Title
                ]
                Html.p [
                    prop.fss [ headingFont ]
                    prop.text $"{mealToNorwegian recipe.Meal} på {recipe.Time} minutter."
                ]
            ]
            Html.p recipe.Description

            Html.div [
                prop.fss [
                    Display.grid
                    GridTemplateColumns.values [ fr 0.5; fr 1.5]
                    GridColumnGap' (px 20)
                ]
                prop.children [
                    Html.div [
                        prop.children [
                            Html.h3 "Ingredienser"
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
                            Html.h3 "Steg"

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
let Homeview() =
    Html.div [
        Html.h3 "Velkommen til SLAFS!"
        Html.text "Et helt greit sted å finne oppskrifter på."
    ]

[<ReactComponent>]
let MealView meal setRecipeView =
    let (state, _) = useStore()
    match state.Recipes with
    | Data recipes ->
        Html.div [
            prop.fss [
                Display.flex
                FlexDirection.column
            ]
            prop.children [
                Html.h1 $"{mealToNorwegian meal} oppskrifter"
                yield!
                    recipes
                    |> List.filter (fun r -> r.Meal = meal)
                    |> List.map (fun r -> Button r.Title (fun _ -> setRecipeView r) Transparent Black)
            ]
        ]
    | Fetching -> Html.text "Laster oppskrifter..."
    | Failure e -> Html.text $"En feil har forekommet {e}"

[<ReactComponent>]
let NewRecipeView () =
    let (_, dispatch) = useStore()

    let (title, setTitle) = React.useState ""
    let (description, setDescription) = React.useState ""
    let (meal, setMeal) = React.useState Breakfast
    let (time, setTime) = React.useState 0.
    let (steps, setSteps) = React.useState<string list> List.empty
    let (ingredients, setIngredients) = React.useState<Ingredient list> List.empty
    let (portions, setPortions) = React.useState 0

    let setSteps key value =
        if List.length steps > key then
            let newList = List.replaceIndex key value steps
            setSteps newList
        else
            setSteps (steps @ [value])

    let setIngredients key value =
        if List.length ingredients > key then
            let newList = List.replaceIndex key value ingredients
            setIngredients newList
        else
            setIngredients (ingredients @ [value])

    let saveRecipe () =
        let recipe = createRecipe title description meal time steps ingredients portions

        let properties =
            [ RequestProperties.Method HttpMethod.POST
              requestHeaders [ ContentType "application/json" ]
              RequestProperties.Body (unbox(Encode.Auto.toString(4, recipe, caseStrategy = CamelCase))) ]
        (*
        fetch "http://0.0.0.0:80/api/recipe" properties
        |> Promise.map(fun _ ->
            dispatch (AddRecipe recipe)
            dispatch (SetCurrentView (RecipeDetails recipe)))
        |> Promise.start
        *)

        promise {
            do fetch "http://slafs.herokuapp.com/api/recipe" properties |> ignore
            do dispatch (AddRecipe recipe)
            do dispatch (SetCurrentView (RecipeDetails recipe))
        }
        |> Promise.start

    let Label (text: string) = Html.label [ prop.text text ]

    let FormElement title (children: ReactElement seq) =
        Html.div [
            prop.fss [
                Display.flex
                FlexDirection.column
            ]
            prop.children [
                Label title
                yield! children
            ]
        ]

    let IngredientElement key value =
        Html.div [
            prop.fss [ Display.flex ]
            prop.children [
                FormElement "Volum" [
                    Html.input [
                        prop.type' "number"
                        prop.value value.Volume
                        prop.onChange (fun n -> setIngredients key {value with Volume = (float n)})
                    ]
                ]
                FormElement "Enhet" [
                    Html.select [
                        prop.children ((List.map measurementToString measurementList)
                                       |> List.map (fun n -> Html.option n))
                        prop.value (measurementToString value.Measurement)
                        prop.onChange (fun m -> setIngredients key {value with Measurement = (stringToMeasurement m) })
                    ]
                ]
                FormElement "Navn" [
                    Html.input [
                        prop.type' "text"
                        prop.value value.Name
                        prop.onChange (fun n -> setIngredients key {value with Name = n})
                    ]
                ]
            ]
        ]

    Html.div [
        prop.children [
            Html.h1 "Ny oppskrift"

            FormElement "Tittel" [
                Html.input [
                    prop.onChange setTitle
                    prop.value title
                ]
            ]
            FormElement "Beskrivelse" [
                Html.input [
                    prop.onChange setDescription
                    prop.value description
                ]
            ]
            FormElement "Måltid" [
                Html.select [
                    prop.children
                        (mealList
                        |> List.map mealToNorwegian
                        |> List.map (fun m -> Html.option m))
                    prop.onChange (norwegianToMeal >> setMeal)
                ]
            ]
            FormElement "Minutter" [
                Html.input [
                    prop.type' "number"
                    prop.onChange (float >> setTime)
                    prop.value time
                ]
            ]
            FormElement "Steg"
                (steps
                |> List.mapi (fun key value ->
                    Html.textarea [
                        prop.value value
                        prop.onChange (fun s -> setSteps key s)
                    ]))

            Button "Nytt steg" (fun _ -> setSteps (List.length steps) "") ButtonColor.Green Black

            FormElement "Ingredienser"
                (ingredients
                 |> List.mapi (fun key value -> IngredientElement key value))

            Button "Ny ingrediens" (fun _ -> setIngredients (List.length ingredients) (ingredient 0.0 Kg "")) ButtonColor.Green Black

            FormElement "Porsjoner" [
                Html.input [
                    prop.type' "number"
                    prop.onChange (int >> setPortions)
                ]
            ]

            Button "Lagre oppskrift" (fun _ -> saveRecipe ()) ButtonColor.Green Black
        ]
    ]


[<ReactComponent>]
let PageView() =
    let (state, dispatch) = useStore()
    let setRecipeView recipe = dispatch (SetCurrentView (RecipeDetails recipe))
    let MealView meal = MealView meal setRecipeView


    Html.div [
        prop.fss [
            Display.flex
            FlexDirection.column
            AlignItems.center
            !> FssTypes.Html.All [ textFont ]
            !> FssTypes.Html.Header [ headingFont ]
        ]

        prop.children [
            Menu ()
            match state.View with
            | Home -> Homeview ()
            | RecipeDetails recipe -> Recipe recipe
            | Breakfasts -> MealView Breakfast
            | Lunches -> MealView Lunch
            | Dinners -> MealView Dinner
            | Desserts -> MealView Desert
            | NewRecipe -> NewRecipeView ()
        ]
    ]



[<ReactComponent>]
let Container() =
    let (state, dispatch) = useStore()

    Hooks.useEffectOnce((fun () ->
        fetch "http://slafs.herokuapp.com/api/recipes" []
        |> Promise.bind (fun result -> result.text())
        |> Promise.map (fun result -> Decode.Auto.fromString<Recipe list>(result, caseStrategy=CamelCase))
        |> Promise.map (fun result ->
            match result with
            | Ok recipes -> Data recipes
            | Error e -> Failure e)
        |> Promise.map (fun r -> dispatch (SetRecipes r))
        |> Promise.start))

    match state.Recipes with
    | Fetching -> Html.div [ prop.text "Laster..." ]
    | Data _ -> PageView()
    | Failure e -> Html.div [
        prop.fss [ textFont ]
        prop.text $"En feil skjedde under henting av oppskrifter: {e}"
    ]


[<ReactComponent>]
let App () = StoreProvider <| Container ()

open Browser.Dom

ReactDOM.render(App(), document.getElementById "root")