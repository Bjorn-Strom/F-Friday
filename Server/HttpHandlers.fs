module HttpHandlers

open Giraffe
open Microsoft.AspNetCore.Http

let getRecipes: HttpHandler =
    fun (next: HttpFunc) (context: HttpContext) ->
        task {
            let! recipes = Database.getAllRecipes ()
            let result =
                match recipes with
                | Ok recipes -> json recipes
                | Error e ->
                    context.SetStatusCode 400
                    json e
            return! result next context
        }

let postRecipe: HttpHandler =
    fun (next: HttpFunc) (context: HttpContext) ->
        task {
            let! newRecipe = context.BindJsonAsync<Shared.Recipe>()
            let! recipe = Database.addRecipe newRecipe
            let result =
                match recipe with
                | Ok recipe -> json recipe
                | Error e ->
                    context.SetStatusCode 400
                    json e
            return! result next context
        }

let putRecipe: HttpHandler =
    fun (next: HttpFunc) (context: HttpContext) ->
        task {
            let! recipeToUpdate = context.BindJsonAsync<Shared.Recipe>()
            let! recipe = Database.updateRecipe recipeToUpdate
            let result =
                match recipe with
                | Ok recipe -> json recipe
                | Error e ->
                    context.SetStatusCode 400
                    json e
                    
            return! result next context
        }

let deleteRecipe (id: System.Guid) : HttpHandler =
    fun (next: HttpFunc) (context: HttpContext) ->
        task {
            let! delete = Database.deleteRecipe id
            
            let result =
                match delete with
                    | Ok _ -> json $"Deleted recipe with id: {id}"
                    | Error e ->
                        context.SetStatusCode 400
                        json e
                
            return! result next context
        }
