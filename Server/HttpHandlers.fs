module HttpHandlers

open Giraffe
open Microsoft.AspNetCore.Http

let getRecipes: HttpHandler =
    fun (next: HttpFunc) (context: HttpContext) ->
        task {
            let! recipes = Database.getAllRecipes ()
            return! json recipes next context
        }

let postRecipe: HttpHandler =
    fun (next: HttpFunc) (context: HttpContext) ->
        task {
            let! newRecipe = context.BindJsonAsync<Shared.Recipe>()
            do! Database.addRecipe newRecipe
            return! json newRecipe next context
        }

let putRecipe: HttpHandler =
    fun (next: HttpFunc) (context: HttpContext) ->
        task {
            let! recipeToUpdate = context.BindJsonAsync<Shared.Recipe>()
            do! Database.updateRecipe recipeToUpdate
            return! json recipeToUpdate next context
        }

let deleteRecipe (id: System.Guid) : HttpHandler =
    fun (next: HttpFunc) (context: HttpContext) ->
        task {
            do! Database.deleteRecipe id
            return! text $"Deleted recipe with id: {id}" next context
        }
