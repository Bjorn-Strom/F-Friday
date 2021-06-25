import { Union } from "./.fable/fable-library.3.2.4/Types.js";
import { list_type, union_type, string_type } from "./.fable/fable-library.3.2.4/Reflection.js";
import { FontSize$0027, FontFamily_custom_Z721C83C5 } from "./.fable/Fss-lib.2.0.2/css/Font.fs.js";
import { createElement } from "react";
import { createObj } from "./.fable/fable-library.3.2.4/Util.js";
import { Feliz_prop__prop_fss_Static_Z3BB76C00 } from "./.fable/Fss-lib-feliz.1.0.0/FssFeliz.fs.js";
import { singleton, append, delay, toList } from "./.fable/fable-library.3.2.4/Seq.js";
import { BorderRadius$0027, Border_get_none } from "./.fable/Fss-lib.2.0.2/css/Border.fs.js";
import { fr, counterStyle, vw, pct, px } from "./.fable/Fss-lib.2.0.2/Functions.fs.js";
import { ColorBase$1__get_green, ColorBase$1__get_blue, ColorBase$1__get_transparent } from "./.fable/Fss-lib.2.0.2/Types/Color.fs.js";
import { Background_BackgroundColor } from "./.fable/Fss-lib.2.0.2/css/Background.fs.js";
import { Hover } from "./.fable/Fss-lib.2.0.2/PseudoClass.fs.js";
import { Cursor_get_pointer } from "./.fable/Fss-lib.2.0.2/css/Cursor.fs.js";
import { Color } from "./.fable/Fss-lib.2.0.2/css/Color.fs.js";
import { head, empty, map, singleton as singleton_1, ofArray } from "./.fable/fable-library.3.2.4/List.js";
import { useReact_useEffect_Z101E1A95, useFeliz_React__React_useState_Static_1505 } from "./.fable/Feliz.1.45.0/React.fs.js";
import { Width$0027, Height$0027 } from "./.fable/Fss-lib.2.0.2/css/ContentSize.fs.js";
import { Position_BoxSizing_get_borderBox } from "./.fable/Fss-lib.2.0.2/css/Position.fs.js";
import { PaddingLeft$0027, Padding$0027 } from "./.fable/Fss-lib.2.0.2/css/Padding.fs.js";
import { MarginTop$0027, MarginLeft$0027 } from "./.fable/Fss-lib.2.0.2/css/Margin.fs.js";
import { Display_get_grid, Display_get_flex } from "./.fable/Fss-lib.2.0.2/css/Display.fs.js";
import { FlexDirection_get_column, JustifyContent_get_spaceBetween, AlignItems_get_center, FlexDirection_get_row, JustifyContent_get_center } from "./.fable/Fss-lib.2.0.2/css/Flex.fs.js";
import { Interop_reactApi } from "./.fable/Feliz.1.45.0/Interop.fs.js";
import { interpolate, toText } from "./.fable/fable-library.3.2.4/String.js";
import { Recipe$reflection, mealToNorwegian } from "./Shared/Shared.js";
import { GridColumnGap$0027, GridTemplateColumns_values_Z1ACB363B } from "./.fable/Fss-lib.2.0.2/css/Grid.fs.js";
import { TextTransform_get_lowercase } from "./.fable/Fss-lib.2.0.2/css/Text.fs.js";
import { CounterIncrement_increment_46C3C923 } from "./.fable/Fss-lib.2.0.2/css/CounterStyle.fs.js";
import { Before } from "./.fable/Fss-lib.2.0.2/PseudoElement.fs.js";
import { ContentClass__counter_Z6D206E48 } from "./.fable/Fss-lib.2.0.2/Types/Content.fs.js";
import { Content_Content } from "./.fable/Fss-lib.2.0.2/css/Content.fs.js";
import { fetch$ } from "./.fable/Fable.Fetch.2.3.1/Fetch.fs.js";
import { Auto_fromString_Z5CB6BD } from "./.fable/Thoth.Json.5.1.0/Decode.fs.js";
import { CaseStrategy } from "./.fable/Thoth.Json.5.1.0/Types.fs.js";
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

export const headingFont = FontFamily_custom_Z721C83C5("Nunito");

export const textFont = FontFamily_custom_Z721C83C5("Raleway");

export class ButtonColor extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Transparent"];
    }
}

export function ButtonColor$reflection() {
    return union_type("Client.ButtonColor", [], ButtonColor, () => [[]]);
}

export function Button(buttonInputProps) {
    const color = buttonInputProps.color;
    const onClick = buttonInputProps.onClick;
    const text = buttonInputProps.text;
    return createElement("button", createObj(ofArray([["children", text], ["onClick", onClick], Feliz_prop__prop_fss_Static_Z3BB76C00(toList(delay(() => append(singleton(Border_get_none()), delay(() => append(singleton(textFont), delay(() => append(singleton(FontSize$0027(px(18))), delay(() => append(singleton(headingFont), delay(() => append(singleton(ColorBase$1__get_transparent(Background_BackgroundColor)), delay(() => singleton(Hover(ofArray([Cursor_get_pointer(), ColorBase$1__get_blue(Color)]))))))))))))))))])));
}

export function SearchBar() {
    const patternInput = useFeliz_React__React_useState_Static_1505("");
    const setSearchTerm = patternInput[1];
    const searchTerm = patternInput[0];
    return createElement("input", createObj(ofArray([["type", "text"], ["value", searchTerm], ["onChange", (ev) => {
        setSearchTerm(ev.target.value);
    }], ["placeholder", "Søk etter oppskrift"], Feliz_prop__prop_fss_Static_Z3BB76C00(ofArray([Height$0027(pct(100)), textFont, Position_BoxSizing_get_borderBox(), Padding$0027(px(5)), PaddingLeft$0027(px(20)), BorderRadius$0027(px(10)), Border_get_none()]))])));
}

export function Menu() {
    let children;
    return createElement("nav", createObj(ofArray([Feliz_prop__prop_fss_Static_Z3BB76C00(ofArray([ColorBase$1__get_green(Background_BackgroundColor), Width$0027(vw(100)), Height$0027(px(70)), MarginLeft$0027(px(-8)), MarginTop$0027(px(-10)), Display_get_flex(), JustifyContent_get_center()])), ["children", Interop_reactApi.Children.toArray([createElement("div", createObj(ofArray([Feliz_prop__prop_fss_Static_Z3BB76C00(ofArray([Display_get_flex(), FlexDirection_get_row(), AlignItems_get_center(), JustifyContent_get_spaceBetween(), Width$0027(vw(50))])), ["children", Interop_reactApi.Children.toArray([createElement("h1", createObj(ofArray([Feliz_prop__prop_fss_Static_Z3BB76C00(singleton_1(headingFont)), ["children", "Slafs!"]]))), (children = ofArray([createElement(SearchBar, null), createElement(Button, {
        text: "Frokost",
        onClick: (_arg1) => {
        },
        color: new ButtonColor(0),
    }), createElement(Button, {
        text: "Lunsj",
        onClick: (_arg2) => {
        },
        color: new ButtonColor(0),
    }), createElement(Button, {
        text: "Middag",
        onClick: (_arg3) => {
        },
        color: new ButtonColor(0),
    }), createElement(Button, {
        text: "Dessert",
        onClick: (_arg4) => {
        },
        color: new ButtonColor(0),
    }), createElement(Button, {
        text: "Min handleliste",
        onClick: (_arg5) => {
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
    return createElement("article", createObj(ofArray([Feliz_prop__prop_fss_Static_Z3BB76C00(ofArray([Width$0027(vw(50)), Display_get_flex(), FlexDirection_get_column()])), ["children", Interop_reactApi.Children.toArray([(children = ofArray([createElement("h1", createObj(ofArray([Feliz_prop__prop_fss_Static_Z3BB76C00(ofArray([headingFont, JustifyContent_get_center()])), ["children", recipe.Title]]))), createElement("p", createObj(ofArray([Feliz_prop__prop_fss_Static_Z3BB76C00(singleton_1(headingFont)), ["children", toText(interpolate("%P() på %P() minutter.", [mealToNorwegian(recipe.Meal), recipe.Time]))]])))]), createElement("div", {
        children: Interop_reactApi.Children.toArray(Array.from(children)),
    })), createElement("p", createObj(ofArray([Feliz_prop__prop_fss_Static_Z3BB76C00(singleton_1(textFont)), ["children", recipe.Description]]))), createElement("div", createObj(ofArray([Feliz_prop__prop_fss_Static_Z3BB76C00(ofArray([Display_get_grid(), GridTemplateColumns_values_Z1ACB363B(ofArray([fr(0.5), fr(1.5)])), GridColumnGap$0027(px(20))])), ["children", Interop_reactApi.Children.toArray([createElement("div", {
        children: Interop_reactApi.Children.toArray(Array.from(toList(delay(() => append(singleton(createElement("h3", createObj(ofArray([Feliz_prop__prop_fss_Static_Z3BB76C00(singleton_1(headingFont)), ["children", "Ingredienser"]])))), delay(() => {
            let value_8;
            return append(singleton((value_8 = toText(interpolate("For %P() porsjoner: ", [recipe.Portions])), createElement("p", {
                children: [value_8],
            }))), delay(() => map((i) => createElement("div", createObj(ofArray([Feliz_prop__prop_fss_Static_Z3BB76C00(singleton_1(TextTransform_get_lowercase())), ["children", i]]))), selectIngredients(recipe))));
        })))))),
    }), createElement("div", {
        children: Interop_reactApi.Children.toArray(Array.from(toList(delay(() => append(singleton(createElement("h3", createObj(ofArray([Feliz_prop__prop_fss_Static_Z3BB76C00(singleton_1(headingFont)), ["children", "Steg"]])))), delay(() => map((s) => createElement("p", createObj(ofArray([Feliz_prop__prop_fss_Static_Z3BB76C00(ofArray([CounterIncrement_increment_46C3C923(stepCounter), Before(singleton_1(ContentClass__counter_Z6D206E48(Content_Content, stepCounter, ". ")))])), ["children", s]]))), recipe.Steps))))))),
    })])]])))])]])));
}

export function Container(containerInputProps) {
    const recipes = containerInputProps.recipes;
    const patternInput = useFeliz_React__React_useState_Static_1505(head(recipes));
    const setCurrentRecipe = patternInput[1];
    const currentRecipe = patternInput[0];
    return createElement("div", createObj(ofArray([Feliz_prop__prop_fss_Static_Z3BB76C00(ofArray([Display_get_flex(), FlexDirection_get_column(), AlignItems_get_center()])), ["children", Interop_reactApi.Children.toArray([createElement(Menu, null), createElement(Recipe, currentRecipe)])]])));
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

