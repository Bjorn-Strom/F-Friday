import { MatchFailureException, toString, Record, Union } from "../.fable/fable-library.3.2.8/Types.js";
import { int32_type, list_type, class_type, record_type, string_type, float64_type, union_type } from "../.fable/fable-library.3.2.8/Reflection.js";
import { mapIndexed, ofArray } from "../.fable/fable-library.3.2.8/List.js";
import { newGuid } from "../.fable/fable-library.3.2.8/Guid.js";

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
    return union_type("Shared.Measurement", [], Measurement, () => [[], [], [], [], [], [], [], [], [], []]);
}

export const measurementList = ofArray([new Measurement(0), new Measurement(1), new Measurement(2), new Measurement(3), new Measurement(4), new Measurement(5), new Measurement(6), new Measurement(7), new Measurement(8), new Measurement(9)]);

export function measurementToString(_arg1) {
    switch (_arg1.tag) {
        case 1: {
            return "G";
        }
        case 2: {
            return "Mg";
        }
        case 3: {
            return "L";
        }
        case 4: {
            return "Dl";
        }
        case 5: {
            return "Ml";
        }
        case 6: {
            return "Ms";
        }
        case 7: {
            return "Ss";
        }
        case 8: {
            return "Ts";
        }
        case 9: {
            return "Stk";
        }
        default: {
            return "Kg";
        }
    }
}

export function stringToMeasurement(_arg1) {
    switch (_arg1) {
        case "Kg": {
            return new Measurement(0);
        }
        case "G": {
            return new Measurement(1);
        }
        case "Mg": {
            return new Measurement(2);
        }
        case "L": {
            return new Measurement(3);
        }
        case "Dl": {
            return new Measurement(4);
        }
        case "Ml": {
            return new Measurement(5);
        }
        case "Ms": {
            return new Measurement(6);
        }
        case "Ss": {
            return new Measurement(7);
        }
        case "Ts": {
            return new Measurement(8);
        }
        case "Stk": {
            return new Measurement(9);
        }
        default: {
            return new Measurement(9);
        }
    }
}

export class Ingredient extends Record {
    constructor(Volume, Measurement, Name) {
        super();
        this.Volume = Volume;
        this.Measurement = Measurement;
        this.Name = Name;
    }
}

export function Ingredient$reflection() {
    return record_type("Shared.Ingredient", [], Ingredient, () => [["Volume", float64_type], ["Measurement", Measurement$reflection()], ["Name", string_type]]);
}

export function ingredient(volume, measurement, name) {
    return new Ingredient(volume, measurement, name);
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

export const mealList = ofArray([new Meal(0), new Meal(1), new Meal(2), new Meal(3)]);

export function mealToNorwegian(meal) {
    switch (meal.tag) {
        case 1: {
            return "Lunsj";
        }
        case 2: {
            return "Middag";
        }
        case 3: {
            return "Dessert";
        }
        default: {
            return "Frokost";
        }
    }
}

export function norwegianToMeal(_arg1) {
    switch (_arg1) {
        case "Frokost": {
            return new Meal(0);
        }
        case "Lunsj": {
            return new Meal(1);
        }
        case "Middag": {
            return new Meal(2);
        }
        case "Dessert": {
            return new Meal(3);
        }
        default: {
            return new Meal(2);
        }
    }
}

export function stringifyMeal(meal) {
    let copyOfStruct = meal;
    return toString(copyOfStruct);
}

export function stringToMeal(_arg1) {
    switch (_arg1) {
        case "Breakfast": {
            return new Meal(0);
        }
        case "Lunch": {
            return new Meal(1);
        }
        case "Dinner": {
            return new Meal(2);
        }
        case "Desert": {
            return new Meal(3);
        }
        default: {
            throw (new MatchFailureException("/Users/bjornivar/Documents/Functional/F-Friday/Shared/Shared.fs", 84, 4));
        }
    }
}

export class Recipe extends Record {
    constructor(Id, Title, Description, Meal, Time, Steps, Ingredients, Portions) {
        super();
        this.Id = Id;
        this.Title = Title;
        this.Description = Description;
        this.Meal = Meal;
        this.Time = Time;
        this.Steps = Steps;
        this.Ingredients = Ingredients;
        this.Portions = (Portions | 0);
    }
}

export function Recipe$reflection() {
    return record_type("Shared.Recipe", [], Recipe, () => [["Id", class_type("System.Guid")], ["Title", string_type], ["Description", string_type], ["Meal", Meal$reflection()], ["Time", float64_type], ["Steps", list_type(string_type)], ["Ingredients", list_type(Ingredient$reflection())], ["Portions", int32_type]]);
}

export function createRecipe(title, description, meal, time, steps, ingredients, portions) {
    return new Recipe(newGuid(), title, description, meal, time, steps, ingredients, portions);
}

export function List_replaceIndex(index, newItem, list) {
    return mapIndexed((currentIndex, oldItem) => ((currentIndex === index) ? newItem : oldItem), list);
}

