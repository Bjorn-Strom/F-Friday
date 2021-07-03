import { Union } from "./.fable/fable-library.3.2.8/Types.js";
import { list_type, union_type, string_type } from "./.fable/fable-library.3.2.8/Reflection.js";
import { FontSize$0027, FontFamily_custom_Z721C83C5 } from "./.fable/Fss-lib.2.0.2/css/Font.fs.js";
import { createElement } from "react";
import { equals, createObj } from "./.fable/fable-library.3.2.8/Util.js";
import { Feliz_prop__prop_fss_Static_Z3BB76C00 } from "./.fable/Fss-lib-feliz.1.0.0/FssFeliz.fs.js";
import { singleton, append, delay, toList } from "./.fable/fable-library.3.2.8/Seq.js";
import { Border_get_none } from "./.fable/Fss-lib.2.0.2/css/Border.fs.js";
import { fr, counterStyle, vw, px } from "./.fable/Fss-lib.2.0.2/Functions.fs.js";
import { ColorBase$1__get_blue, ColorBase$1__get_transparent, ColorBase$1__get_green } from "./.fable/Fss-lib.2.0.2/Types/Color.fs.js";
import { Background_BackgroundColor } from "./.fable/Fss-lib.2.0.2/css/Background.fs.js";
import { Hover } from "./.fable/Fss-lib.2.0.2/PseudoClass.fs.js";
import { Cursor_get_pointer } from "./.fable/Fss-lib.2.0.2/css/Cursor.fs.js";
import { Color } from "./.fable/Fss-lib.2.0.2/css/Color.fs.js";
import { head, filter, singleton as singleton_1, empty, map, ofArray } from "./.fable/fable-library.3.2.8/List.js";
import { Height$0027, Width$0027 } from "./.fable/Fss-lib.2.0.2/css/ContentSize.fs.js";
import { MarginTop$0027, MarginLeft$0027 } from "./.fable/Fss-lib.2.0.2/css/Margin.fs.js";
import { Display_get_grid, Display_get_flex } from "./.fable/Fss-lib.2.0.2/css/Display.fs.js";
import { FlexDirection_get_column, JustifyContent_get_spaceBetween, AlignItems_get_center, FlexDirection_get_row, JustifyContent_get_center } from "./.fable/Fss-lib.2.0.2/css/Flex.fs.js";
import { Interop_reactApi } from "./.fable/Feliz.1.45.0/Interop.fs.js";
import { interpolate, toText } from "./.fable/fable-library.3.2.8/String.js";
import { Measurement as Measurement_1, ingredient, norwegianToMeal, mealList, stringToMeasurement, measurementList, measurementToString, Ingredient, Recipe$reflection, createRecipe, Meal, mealToNorwegian } from "./Shared/Shared.js";
import { GridColumnGap$0027, GridTemplateColumns_values_Z1ACB363B } from "./.fable/Fss-lib.2.0.2/css/Grid.fs.js";
import { TextTransform_get_lowercase } from "./.fable/Fss-lib.2.0.2/css/Text.fs.js";
import { CounterIncrement_increment_46C3C923 } from "./.fable/Fss-lib.2.0.2/css/CounterStyle.fs.js";
import { Before } from "./.fable/Fss-lib.2.0.2/PseudoElement.fs.js";
import { ContentClass__counter_Z6D206E48 } from "./.fable/Fss-lib.2.0.2/Types/Content.fs.js";
import { Content_Content } from "./.fable/Fss-lib.2.0.2/css/Content.fs.js";
import { useReact_useEffect_Z101E1A95, useFeliz_React__React_useState_Static_1505 } from "./.fable/Feliz.1.45.0/React.fs.js";
import { FSharpMap__get_Count, toList as toList_1, FSharpMap__Add, empty as empty_1 } from "./.fable/fable-library.3.2.8/Map.js";
import { fetch$, Types_RequestProperties } from "./.fable/Fable.Fetch.2.3.1/Fetch.fs.js";
import { Auto_toString_5A41365E } from "./.fable/Thoth.Json.5.1.0/Encode.fs.js";
import { CaseStrategy } from "./.fable/Thoth.Json.5.1.0/Types.fs.js";
import { parse } from "./.fable/fable-library.3.2.8/Double.js";
import { parse as parse_1 } from "./.fable/fable-library.3.2.8/Int32.js";
import { op_BangGreater } from "./.fable/Fss-lib.2.0.2/Selector.fs.js";
import { Html_Html } from "./.fable/Fss-lib.2.0.2/Types/Html.fs.js";
import { Auto_fromString_Z5CB6BD } from "./.fable/Thoth.Json.5.1.0/Decode.fs.js";
import { render } from "react-dom";

export class RemoteData$1 extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Fetching", "Data", "Failure"];
    }
}

export function RemoteData$1$reflection(gen0) {
    return union_type("Client.RemoteData`1", [gen0], RemoteData$1, () => [[], [["Item", gen0]], [["Item", string_type]]]);
}

export class View extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["RecipeDetails", "Breakfasts", "Lunches", "Dinners", "Desserts", "NewRecipe", "EditRecipe"];
    }
}

export function View$reflection() {
    return union_type("Client.View", [], View, () => [[], [], [], [], [], [], []]);
}

export const headingFont = FontFamily_custom_Z721C83C5("Nunito");

export const textFont = FontFamily_custom_Z721C83C5("Raleway");

export class ButtonColor extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Transparent", "Green"];
    }
}

export function ButtonColor$reflection() {
    return union_type("Client.ButtonColor", [], ButtonColor, () => [[], []]);
}

export function Button(buttonInputProps) {
    const color = buttonInputProps.color;
    const onClick = buttonInputProps.onClick;
    const text = buttonInputProps.text;
    return createElement("button", createObj(ofArray([["children", text], ["onClick", onClick], Feliz_prop__prop_fss_Static_Z3BB76C00(toList(delay(() => append(singleton(Border_get_none()), delay(() => append(singleton(FontSize$0027(px(18))), delay(() => append(singleton(headingFont), delay(() => append((color.tag === 1) ? singleton(ColorBase$1__get_green(Background_BackgroundColor)) : singleton(ColorBase$1__get_transparent(Background_BackgroundColor)), delay(() => singleton(Hover(ofArray([Cursor_get_pointer(), ColorBase$1__get_blue(Color)]))))))))))))))])));
}

export function Menu(menuInputProps) {
    let children;
    const setView = menuInputProps.setView;
    return createElement("nav", createObj(ofArray([Feliz_prop__prop_fss_Static_Z3BB76C00(ofArray([ColorBase$1__get_green(Background_BackgroundColor), Width$0027(vw(100)), Height$0027(px(70)), MarginLeft$0027(px(-8)), MarginTop$0027(px(-10)), Display_get_flex(), JustifyContent_get_center()])), ["children", Interop_reactApi.Children.toArray([createElement("div", createObj(ofArray([Feliz_prop__prop_fss_Static_Z3BB76C00(ofArray([Display_get_flex(), FlexDirection_get_row(), AlignItems_get_center(), JustifyContent_get_spaceBetween(), Width$0027(vw(50))])), ["children", Interop_reactApi.Children.toArray([createElement("h1", {
        children: ["Slafs!"],
    }), (children = ofArray([createElement(Button, {
        text: "+",
        onClick: (_arg1) => {
            setView(new View(5));
        },
        color: new ButtonColor(0),
    }), createElement(Button, {
        text: "Frokost",
        onClick: (_arg2) => {
            setView(new View(1));
        },
        color: new ButtonColor(0),
    }), createElement(Button, {
        text: "Lunsj",
        onClick: (_arg3) => {
            setView(new View(2));
        },
        color: new ButtonColor(0),
    }), createElement(Button, {
        text: "Middag",
        onClick: (_arg4) => {
            setView(new View(3));
        },
        color: new ButtonColor(0),
    }), createElement(Button, {
        text: "Dessert",
        onClick: (_arg5) => {
            setView(new View(4));
        },
        color: new ButtonColor(0),
    }), createElement(Button, {
        text: "Min handleliste",
        onClick: (_arg6) => {
        },
        color: new ButtonColor(0),
    })]), createElement("div", {
        children: Interop_reactApi.Children.toArray(Array.from(children)),
    }))])]])))])]])));
}

export function selectIngredients(recipe) {
    return map((i) => toText(interpolate("%P() %P() %P()", [i.Volume, i.Measurement, i.Name])), recipe.Ingredients);
}

export const stepCounter = counterStyle(empty());

export function Recipe(recipe) {
    let children;
    return createElement("article", createObj(ofArray([Feliz_prop__prop_fss_Static_Z3BB76C00(ofArray([Width$0027(vw(50)), Display_get_flex(), FlexDirection_get_column()])), ["children", Interop_reactApi.Children.toArray([(children = ofArray([createElement("h1", createObj(ofArray([Feliz_prop__prop_fss_Static_Z3BB76C00(singleton_1(JustifyContent_get_center())), ["children", recipe.Title]]))), createElement("p", createObj(ofArray([Feliz_prop__prop_fss_Static_Z3BB76C00(singleton_1(headingFont)), ["children", toText(interpolate("%P() på %P() minutter.", [mealToNorwegian(recipe.Meal), recipe.Time]))]])))]), createElement("div", {
        children: Interop_reactApi.Children.toArray(Array.from(children)),
    })), createElement("p", {
        children: [recipe.Description],
    }), createElement("div", createObj(ofArray([Feliz_prop__prop_fss_Static_Z3BB76C00(ofArray([Display_get_grid(), GridTemplateColumns_values_Z1ACB363B(ofArray([fr(0.5), fr(1.5)])), GridColumnGap$0027(px(20))])), ["children", Interop_reactApi.Children.toArray([createElement("div", {
        children: Interop_reactApi.Children.toArray(Array.from(toList(delay(() => append(singleton(createElement("h3", {
            children: ["Ingredienser"],
        })), delay(() => {
            let value_6;
            return append(singleton((value_6 = toText(interpolate("For %P() porsjoner: ", [recipe.Portions])), createElement("p", {
                children: [value_6],
            }))), delay(() => map((i) => createElement("div", createObj(ofArray([Feliz_prop__prop_fss_Static_Z3BB76C00(singleton_1(TextTransform_get_lowercase())), ["children", i]]))), selectIngredients(recipe))));
        })))))),
    }), createElement("div", {
        children: Interop_reactApi.Children.toArray(Array.from(toList(delay(() => append(singleton(createElement("h3", {
            children: ["Steg"],
        })), delay(() => map((s) => createElement("p", createObj(ofArray([Feliz_prop__prop_fss_Static_Z3BB76C00(ofArray([CounterIncrement_increment_46C3C923(stepCounter), Before(singleton_1(ContentClass__counter_Z6D206E48(Content_Content, stepCounter, ". ")))])), ["children", s]]))), recipe.Steps))))))),
    })])]])))])]])));
}

export function MealView(mealViewInputProps) {
    const setRecipeView = mealViewInputProps.setRecipeView;
    const meal = mealViewInputProps.meal;
    const recipes = mealViewInputProps.recipes;
    return createElement("div", createObj(ofArray([Feliz_prop__prop_fss_Static_Z3BB76C00(ofArray([Display_get_flex(), FlexDirection_get_column()])), ["children", Interop_reactApi.Children.toArray(Array.from(toList(delay(() => {
        let value;
        return append(singleton((value = toText(interpolate("%P() oppskrifter", [mealToNorwegian(meal)])), createElement("h1", {
            children: [value],
        }))), delay(() => map((r_1) => createElement(Button, {
            text: r_1.Title,
            onClick: (_arg1) => {
                setRecipeView(r_1);
            },
            color: new ButtonColor(0),
        }), filter((r) => equals(r.Meal, meal), recipes))));
    }))))]])));
}

export function NewRecipeView() {
    let arg10_3, arg10_4, arg10_5, arg10_6, arg10_7, arg10_8, arg10_9;
    const patternInput = useFeliz_React__React_useState_Static_1505("");
    const title = patternInput[0];
    const setTitle = patternInput[1];
    const patternInput_1 = useFeliz_React__React_useState_Static_1505("");
    const setDescription = patternInput_1[1];
    const description = patternInput_1[0];
    const patternInput_2 = useFeliz_React__React_useState_Static_1505(new Meal(0));
    const setMeal = patternInput_2[1];
    const meal = patternInput_2[0];
    const patternInput_3 = useFeliz_React__React_useState_Static_1505(0);
    const time = patternInput_3[0];
    const setTime = patternInput_3[1];
    const patternInput_4 = useFeliz_React__React_useState_Static_1505(empty_1());
    const steps = patternInput_4[0];
    const setSteps = patternInput_4[1];
    const patternInput_5 = useFeliz_React__React_useState_Static_1505(empty_1());
    const setIngredients = patternInput_5[1];
    const ingredients = patternInput_5[0];
    const patternInput_6 = useFeliz_React__React_useState_Static_1505(0);
    const setPortions = patternInput_6[1];
    const portions = patternInput_6[0] | 0;
    const setIngredients_1 = (key, value) => {
        setIngredients(FSharpMap__Add(ingredients, key, value));
    };
    const saveRecipe = () => {
        const stepList = map((tupledArg) => {
            const value_1 = tupledArg[1];
            return value_1;
        }, toList_1(steps));
        const ingredientList = map((tupledArg_1) => {
            const value_2 = tupledArg_1[1];
            return value_2;
        }, toList_1(ingredients));
        const recipe = createRecipe(title, description, meal, time, stepList, ingredientList, portions);
        const properties = ofArray([new Types_RequestProperties(0, "POST"), new Types_RequestProperties(1, {
            ["Content-Type"]: "application/json",
        }), new Types_RequestProperties(2, Auto_toString_5A41365E(4, recipe, new CaseStrategy(1), void 0, void 0, {
            ResolveType: Recipe$reflection,
        }))]);
        const pr = fetch$("http://localhost:5000/api/recipe", properties);
        pr.then();
    };
    const Label = (text) => createElement("label", {
        children: text,
    });
    const FormElement = (title_1, children) => createElement("div", createObj(ofArray([Feliz_prop__prop_fss_Static_Z3BB76C00(ofArray([Display_get_flex(), FlexDirection_get_column()])), ["children", Interop_reactApi.Children.toArray(Array.from(toList(delay(() => append(singleton(Label(title_1)), delay(() => children))))))]])));
    const IngredientElement = (key_3, value_6) => {
        let arg10, arg10_1, arg10_2;
        return createElement("div", createObj(ofArray([Feliz_prop__prop_fss_Static_Z3BB76C00(singleton_1(Display_get_flex())), ["children", Interop_reactApi.Children.toArray([(arg10 = singleton_1(createElement("input", {
            type: "number",
            value: value_6.Volume,
            onChange: (ev) => {
                setIngredients_1(key_3, new Ingredient(parse(ev.target.value), value_6.Measurement, value_6.Name));
            },
        })), FormElement("Volum", arg10)), (arg10_1 = singleton_1(createElement("select", {
            children: Interop_reactApi.Children.toArray(Array.from(map((n_1) => createElement("option", {
                children: [n_1],
            }), map((_arg1_1) => measurementToString(_arg1_1), measurementList)))),
            value: measurementToString(value_6.Measurement),
            onChange: (ev_1) => {
                setIngredients_1(key_3, new Ingredient(value_6.Volume, stringToMeasurement(ev_1.target.value), value_6.Name));
            },
        })), FormElement("Enhet", arg10_1)), (arg10_2 = singleton_1(createElement("input", {
            type: "text",
            value: value_6.Name,
            onChange: (ev_2) => {
                setIngredients_1(key_3, new Ingredient(value_6.Volume, value_6.Measurement, ev_2.target.value));
            },
        })), FormElement("Navn", arg10_2))])]])));
    };
    return createElement("div", {
        children: Interop_reactApi.Children.toArray([createElement("h1", {
            children: ["Ny oppskrift"],
        }), (arg10_3 = singleton_1(createElement("input", {
            onChange: (ev_3) => {
                setTitle(ev_3.target.value);
            },
            value: title,
        })), FormElement("Tittel", arg10_3)), (arg10_4 = singleton_1(createElement("input", {
            onChange: (ev_4) => {
                setDescription(ev_4.target.value);
            },
            value: description,
        })), FormElement("Beskrivelse", arg10_4)), (arg10_5 = singleton_1(createElement("select", {
            children: Interop_reactApi.Children.toArray(Array.from(map((m_1) => createElement("option", {
                children: [m_1],
            }), map((meal_1) => mealToNorwegian(meal_1), mealList)))),
            onChange: (ev_5) => {
                setMeal(norwegianToMeal(ev_5.target.value));
            },
        })), FormElement("Måltid", arg10_5)), (arg10_6 = singleton_1(createElement("input", {
            type: "number",
            onChange: (ev_6) => {
                setTime(parse(ev_6.target.value));
            },
            value: time,
        })), FormElement("Tid", arg10_6)), (arg10_7 = map((tupledArg_2) => {
            const key_23 = tupledArg_2[0] | 0;
            const value_39 = tupledArg_2[1];
            return createElement("textarea", {
                value: value_39,
                onChange: (ev_7) => {
                    setSteps(FSharpMap__Add(steps, key_23, ev_7.target.value));
                },
            });
        }, toList_1(steps)), FormElement("Steg", arg10_7)), createElement(Button, {
            text: "Nytt steg",
            onClick: (_arg3) => {
                setSteps(FSharpMap__Add(steps, FSharpMap__get_Count(steps), ""));
            },
            color: new ButtonColor(1),
        }), (arg10_8 = map((tupledArg_3) => {
            const key_26 = tupledArg_3[0] | 0;
            const value_43 = tupledArg_3[1];
            return IngredientElement(key_26, value_43);
        }, toList_1(ingredients)), FormElement("Ingredienser", arg10_8)), createElement(Button, {
            text: "Ny ingrediens",
            onClick: (_arg4) => {
                setIngredients_1(FSharpMap__get_Count(ingredients), ingredient(0, new Measurement_1(0), ""));
            },
            color: new ButtonColor(1),
        }), (arg10_9 = singleton_1(createElement("input", {
            type: "number",
            onChange: (ev_8) => {
                setPortions(parse_1(ev_8.target.value, 511, false, 32));
            },
        })), FormElement("Porsjoner", arg10_9)), createElement(Button, {
            text: "Lagre oppskrift",
            onClick: (_arg5) => {
                saveRecipe();
            },
            color: new ButtonColor(1),
        })]),
    });
}

export function Container(containerInputProps) {
    const recipes = containerInputProps.recipes;
    const patternInput = useFeliz_React__React_useState_Static_1505(head(recipes));
    const setCurrentRecipe = patternInput[1];
    const currentRecipe = patternInput[0];
    const patternInput_1 = useFeliz_React__React_useState_Static_1505(new View(0));
    const view = patternInput_1[0];
    const setView = patternInput_1[1];
    const setRecipeView = (recipe) => {
        setCurrentRecipe(recipe);
        setView(new View(0));
    };
    return createElement("div", createObj(ofArray([Feliz_prop__prop_fss_Static_Z3BB76C00(ofArray([Display_get_flex(), FlexDirection_get_column(), AlignItems_get_center(), op_BangGreater(new Html_Html(1), singleton_1(textFont)), op_BangGreater(new Html_Html(44), singleton_1(headingFont))])), ["children", Interop_reactApi.Children.toArray(Array.from(toList(delay(() => append(singleton(createElement(Menu, {
        setView: setView,
    })), delay(() => ((view.tag === 1) ? singleton(createElement(MealView, {
        recipes: recipes,
        meal: new Meal(0),
        setRecipeView: setRecipeView,
    })) : ((view.tag === 2) ? singleton(createElement(MealView, {
        recipes: recipes,
        meal: new Meal(1),
        setRecipeView: setRecipeView,
    })) : ((view.tag === 3) ? singleton(createElement(MealView, {
        recipes: recipes,
        meal: new Meal(2),
        setRecipeView: setRecipeView,
    })) : ((view.tag === 4) ? singleton(createElement(MealView, {
        recipes: recipes,
        meal: new Meal(3),
        setRecipeView: setRecipeView,
    })) : ((view.tag === 5) ? singleton(createElement(NewRecipeView, null)) : ((view.tag === 6) ? singleton(createElement("h1", {
        children: ["Edit a recipe"],
    })) : singleton(createElement(Recipe, currentRecipe))))))))))))))]])));
}

export function App() {
    const patternInput = useFeliz_React__React_useState_Static_1505(new RemoteData$1(0));
    const setRecipes = patternInput[1];
    const recipes = patternInput[0];
    useReact_useEffect_Z101E1A95(() => {
        let pr_4;
        let pr_3;
        let pr_2;
        let pr_1;
        const pr = fetch$("http://localhost:5000/api/recipes", empty());
        pr_1 = (pr.then(((result) => result.text())));
        pr_2 = (pr_1.then(((result_1) => Auto_fromString_Z5CB6BD(result_1, new CaseStrategy(1), void 0, {
            ResolveType: () => list_type(Recipe$reflection()),
        }))));
        pr_3 = (pr_2.then(((result_2) => {
            if (result_2.tag === 1) {
                const e = result_2.fields[0];
                return new RemoteData$1(2, e);
            }
            else {
                const recipes_1 = result_2.fields[0];
                return new RemoteData$1(1, recipes_1);
            }
        })));
        pr_4 = (pr_3.then(setRecipes));
        pr_4.then();
    }, []);
    switch (recipes.tag) {
        case 1: {
            const recipes_2 = recipes.fields[0];
            return createElement(Container, {
                recipes: recipes_2,
            });
        }
        case 2: {
            const e_1 = recipes.fields[0];
            return createElement("div", createObj(ofArray([Feliz_prop__prop_fss_Static_Z3BB76C00(singleton_1(textFont)), ["children", toText(interpolate("En feil skjedde under henting av oppskrifter: %P()", [e_1]))]])));
        }
        default: {
            return createElement("div", {
                children: "Laster...",
            });
        }
    }
}

render(createElement(App, null), document.getElementById("root"));

