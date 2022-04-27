import { Union } from "../.fable/fable-library.3.2.8/Types.js";
import { union_type } from "../.fable/fable-library.3.2.8/Reflection.js";
import { ofArray } from "../.fable/fable-library.3.2.8/List.js";

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
            return void 0;
        }
    }
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

export function norwegianToMeal(name) {
    switch (name) {
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
            return void 0;
        }
    }
}

