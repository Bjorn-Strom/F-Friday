module Client

open Feliz
open Fetch
open Thoth.Json
open Fss
open Fss.Feliz

open Shared

type RemoteData<'t> =
    | Fetching
    | Data of 't
    | Failure of string

type View =
    | RecipeDetails
    | Breakfasts
    | Lunches
    | Dinners
    | Desserts
    | NewRecipe
    | EditRecipe

// Fonts
let headingFont = FontFamily.custom "Nunito"
let textFont = FontFamily.custom "Raleway"

type ButtonColor =
    | Transparent
    | Green

[<ReactComponent>]
let Button (text: string) onClick color =
    Html.button [
        prop.text text
        prop.onClick onClick
        prop.fss [
            Border.none
            FontSize' (px 18)
            headingFont
            match color with
            | Transparent ->
                BackgroundColor.transparent
            | Green ->
                BackgroundColor.green
            Hover [
                Cursor.pointer
                Color.blue
            ]

        ]
    ]

[<ReactComponent>]
let Menu setView =
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
                    Html.h1 "Slafs!"
                    Html.div [
                        Button "+" (fun _ -> setView NewRecipe) Transparent
                        Button "Frokost" (fun _ -> setView Breakfasts) Transparent
                        Button "Lunsj" (fun _ -> setView Lunches) Transparent
                        Button "Middag" (fun _ -> setView Dinners) Transparent
                        Button "Dessert" (fun _ -> setView Desserts) Transparent
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
let MealView recipes meal setRecipeView =
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
                |> List.map (fun r -> Button r.Title (fun _ -> setRecipeView r) Transparent)
        ]
    ]

[<ReactComponent>]
let NewRecipeView () =
    let (title, setTitle) = React.useState ""
    let (description, setDescription) = React.useState ""
    let (meal, setMeal) = React.useState Breakfast
    let (time, setTime) = React.useState 0.
    let (steps, setSteps) = React.useState<Map<int, string>> Map.empty
    let (ingredients, setIngredients) = React.useState<Map<int, Ingredient>> Map.empty
    let (portions, setPortions) = React.useState 0

    let setIngredients key value =
        setIngredients (ingredients.Add(key, value))

    let saveRecipe () =
        let stepList =
            steps
            |> Map.toList
            |> List.map (fun (_, value) -> value)
        let ingredientList =
            ingredients
            |> Map.toList
            |> List.map (fun (_, value) -> value)
        let recipe = createRecipe title description meal time stepList ingredientList portions

        let properties =
            [ RequestProperties.Method HttpMethod.POST
              requestHeaders [ ContentType "application/json" ]
              RequestProperties.Body (unbox(Encode.Auto.toString(4, recipe, caseStrategy = CamelCase))) ]
        fetch "http://localhost:5000/api/recipe" properties
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
            FormElement "Tid" [
                Html.input [
                    prop.type' "number"
                    prop.onChange (float >> setTime)
                    prop.value time
                ]
            ]
            FormElement "Steg"
                (steps
                |> Map.toList
                |> List.map (fun (key, value) ->
                    Html.textarea [
                        prop.value value
                        prop.onChange (fun s -> setSteps (steps.Add(key, s)))
                    ]))

            Button "Nytt steg" (fun _ -> setSteps (steps.Add(steps.Count, ""))) ButtonColor.Green


            FormElement "Ingredienser"
                (ingredients
                 |> Map.toList
                 |> List.map (fun (key, value) -> IngredientElement key value))

            Button "Ny ingrediens" (fun _ -> setIngredients ingredients.Count (ingredient 0.0 Kg "")) ButtonColor.Green

            FormElement "Porsjoner" [
                Html.input [
                    prop.type' "number"
                    prop.onChange (int >> setPortions)
                ]
            ]

            Button "Lagre oppskrift" (fun _ -> saveRecipe ()) ButtonColor.Green
        ]
    ]


[<ReactComponent>]
let Container (recipes: Recipe list) =
    let (currentRecipe, setCurrentRecipe) = React.useState<Recipe> (List.head recipes)
    let (view, setView) = React.useState<View> RecipeDetails
    let setRecipeView recipe =
        setCurrentRecipe recipe
        setView RecipeDetails
    Html.div [
        prop.fss [
            Display.flex
            FlexDirection.column
            AlignItems.center
            !> FssTypes.Html.All [ textFont ]
            !> FssTypes.Html.Header [ headingFont ]
        ]

        prop.children [
            Menu setView
            match view with
            | RecipeDetails -> Recipe currentRecipe
            | Breakfasts -> MealView recipes Breakfast setRecipeView
            | Lunches -> MealView recipes Lunch setRecipeView
            | Dinners -> MealView recipes Dinner setRecipeView
            | Desserts -> MealView recipes Desert setRecipeView
            | NewRecipe -> NewRecipeView ()
            | EditRecipe -> Html.h1 "Edit a recipe"
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