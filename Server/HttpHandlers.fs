module HttpHandlers

open Giraffe
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks

let getRecipes: HttpHandler =
    fun (next: HttpFunc) (context: HttpContext) ->
        json (Database.getAllRecipes ()) next context
let postRecipe: HttpHandler =
    fun (next: HttpFunc) (context: HttpContext) ->
        task {
            let! newRecipe = context.BindJsonAsync<Shared.Recipe>()
            Database.addRecipe newRecipe
            return! getRecipes next context
        }
let putRecipe: HttpHandler =
     fun (next: HttpFunc) (context: HttpContext) ->
        task {
            let! recipeToUpdate = context.BindJsonAsync<Shared.Recipe>()
            Database.updateRecipe recipeToUpdate
            return! json recipeToUpdate next context
        }
let deleteRecipe (id: System.Guid): HttpHandler =
        Database.deleteRecipe id
        text $"Deleted recipe with id: {id}"