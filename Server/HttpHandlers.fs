module HttpHandlers

open Giraffe
open Thoth.Json.Net
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks

let private badRequest errorMessage next (context: HttpContext) =
    let errorMessage = {| Error = errorMessage |}
    context.SetStatusCode(400)
    json errorMessage next context
    
let decodeRecipeAndIngredientHelper (context: HttpContext) =
    task {
        let! body = context.ReadBodyFromRequestAsync()
        let decodeRecipe = Decode.fromString Types.RecipeDbModel.decoder body
        let decodeIngredient = Decode.fromString Types.IngredientDbModel.decodeList body
        
        match decodeRecipe, decodeIngredient with
        | Ok recipe, Ok ingredients ->
            return Ok (recipe, ingredients)
        |  Error e1, Error e2 ->
            return Error $"Feil under decoding av oppskrifter {e1} og ingredienser {e2}."
        | Error e, _ ->
            return Error $"Feil under decoding av oppskrifter {e}."
        |  _, Error e ->
            return Error $"Feil under decoding av ingredienser {e}."
    }
    
let getRecipes: HttpHandler =
    fun (next: HttpFunc) (context: HttpContext) ->
        let encodedRecipes =
            Database.getAllRecipes ()
            |> List.map Types.encodeRecipeAndIngredient
            
        json encodedRecipes next context
        
let postRecipe: HttpHandler =
    fun (next: HttpFunc) (context: HttpContext) ->
        task {
            let! decodedRecipeAndIngredients = decodeRecipeAndIngredientHelper context
            match decodedRecipeAndIngredients with
            | Ok (recipe, ingredients) ->
                let newRecipes =
                    Database.addRecipe recipe ingredients
                    |> List.map Types.encodeRecipeAndIngredient
                return! json newRecipes next context
            | Error e ->
                return! badRequest e next context
        }
let putRecipe: HttpHandler =
     fun (next: HttpFunc) (context: HttpContext) ->
        task {
            let! decodedRecipeAndIngredients = decodeRecipeAndIngredientHelper context
            match decodedRecipeAndIngredients with
            | Ok (recipe, ingredients) ->
                let newRecipes =
                    Database.updateRecipe recipe ingredients
                    |> Types.encodeRecipeAndIngredient
                return! json newRecipes next context
            | Error e ->
                return! badRequest e next context
        }
        
let deleteRecipe (id: System.Guid): HttpHandler =
    fun (next: HttpFunc) (context: HttpContext) ->
        task {
            Database.deleteRecipe id
            return! json $"Deleted recipe with id: {id}" next context
        }