module HttpHandlers

open Giraffe
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks

let getRecipes: HttpHandler =
    fun (next: HttpFunc) (context: HttpContext) ->
        json (Recipe.getAllRecipes ()) next context
let postRecipe: HttpHandler =
    fun (next: HttpFunc) (context: HttpContext) ->
        task {
            let! newRecipe = context.BindJsonAsync<Recipe.Recipe>()
            Recipe.addRecipe newRecipe
            return! getRecipes next context
        }
let putRecipe: HttpHandler =
     fun (next: HttpFunc) (context: HttpContext) ->
        task {
            let! recipeToUpdate = context.BindJsonAsync<Recipe.Recipe>()
            Recipe.updateRecipe recipeToUpdate
            return! json recipeToUpdate next context
        }
let deleteRecipe (id: System.Guid): HttpHandler =
        Recipe.deleteRecipe id
        text $"Deleted recipe with id: {id}"