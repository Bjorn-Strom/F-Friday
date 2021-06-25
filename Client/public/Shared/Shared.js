import { Record, Union } from "../.fable/fable-library.3.2.4/Types.js";
import { int32_type, list_type, class_type, record_type, string_type, union_type, float64_type } from "../.fable/fable-library.3.2.4/Reflection.js";
import { newGuid } from "../.fable/fable-library.3.2.4/Guid.js";

export class Measurement extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Kg", "G", "Mg", "L", "Dl", "Ml", "Ms", "Ss", "Ts", "Stk"];
    }
}

export function Measurement$reflection() {
    return union_type("Shared.Measurement", [], Measurement, () => [[["Item", float64_type]], [["Item", float64_type]], [["Item", float64_type]], [["Item", float64_type]], [["Item", float64_type]], [["Item", float64_type]], [["Item", float64_type]], [["Item", float64_type]], [["Item", float64_type]], [["Item", float64_type]]]);
}

export class Ingredient extends Record {
    constructor(Measurement, Name) {
        super();
        this.Measurement = Measurement;
        this.Name = Name;
    }
}

export function Ingredient$reflection() {
    return record_type("Shared.Ingredient", [], Ingredient, () => [["Measurement", Measurement$reflection()], ["Name", string_type]]);
}

export function ingredient(volume, measurement, name) {
    return new Ingredient(measurement(volume), name);
}

export class Meal extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Breakfast", "Lunch", "Dinner", "Desert"];
    }
}

export function Meal$reflection() {
    return union_type("Shared.Meal", [], Meal, () => [[], [], [], []]);
}

export class Recipe extends Record {
    constructor(Id, Title, Meal, Time, Steps, Ingredients, Portions, SubRecipes) {
        super();
        this.Id = Id;
        this.Title = Title;
        this.Meal = Meal;
        this.Time = Time;
        this.Steps = Steps;
        this.Ingredients = Ingredients;
        this.Portions = (Portions | 0);
        this.SubRecipes = SubRecipes;
    }
}

export function Recipe$reflection() {
    return record_type("Shared.Recipe", [], Recipe, () => [["Id", class_type("System.Guid")], ["Title", string_type], ["Meal", Meal$reflection()], ["Time", float64_type], ["Steps", list_type(string_type)], ["Ingredients", list_type(Ingredient$reflection())], ["Portions", int32_type], ["SubRecipes", list_type(class_type("System.Guid"))]]);
}

export function createRecipe(title, meal, time, steps, ingredients, portions, subRecipes) {
    return new Recipe(newGuid(), title, meal, time, steps, ingredients, portions, subRecipes);
}

