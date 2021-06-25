import { createElement } from "react";
import { useReact_useEffect_Z101E1A95, useFeliz_React__React_useState_Static_1505 } from "./.fable/Feliz.1.45.0/React.fs.js";
import { map, singleton, empty } from "./.fable/fable-library.3.2.4/List.js";
import { FSharpResult$2 } from "./.fable/fable-library.3.2.4/Choice.js";
import { fetch$ } from "./.fable/Fable.Fetch.2.3.1/Fetch.fs.js";
import { Auto_fromString_Z5CB6BD } from "./.fable/Thoth.Json.5.1.0/Decode.fs.js";
import { CaseStrategy } from "./.fable/Thoth.Json.5.1.0/Types.fs.js";
import { Recipe$reflection } from "./Shared/Shared.js";
import { list_type } from "./.fable/fable-library.3.2.4/Reflection.js";
import { Interop_reactApi } from "./.fable/Feliz.1.45.0/Interop.fs.js";
import { render } from "react-dom";

export function Menu() {
    return createElement("div", {
        children: "Foo",
    });
}

export function App() {
    const patternInput = useFeliz_React__React_useState_Static_1505(new FSharpResult$2(0, empty()));
    const setRecipes = patternInput[1];
    const recipes = patternInput[0];
    useReact_useEffect_Z101E1A95(() => {
        let pr_3;
        let pr_2;
        let pr_1;
        const pr = fetch$("http://localhost:5000/api/recipes", empty());
        pr_1 = (pr.then(((result) => result.text())));
        pr_2 = (pr_1.then(((result_1) => Auto_fromString_Z5CB6BD(result_1, new CaseStrategy(1), void 0, {
            ResolveType: () => list_type(Recipe$reflection()),
        }))));
        pr_3 = (pr_2.then(setRecipes));
        pr_3.then();
    }, []);
    let recipes_2;
    if (recipes.tag === 1) {
        const e = recipes.fields[0];
        recipes_2 = singleton(createElement("h1", {
            children: [e],
        }));
    }
    else {
        const recipes_1 = recipes.fields[0];
        recipes_2 = map((r) => createElement("div", {
            children: r.Title,
        }), recipes_1);
    }
    return createElement("div", {
        children: Interop_reactApi.Children.toArray(Array.from(recipes_2)),
    });
}

render(createElement(App, null), document.getElementById("root"));

