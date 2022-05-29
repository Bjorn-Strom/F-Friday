module HttpHandlers

open Giraffe
open Npgsql
open Thoth.Json.Net
open FSharp.Control.Tasks
open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Http

open ErrorMessage

let getDatabaseConnection (context: HttpContext) = context.GetService<Database.DatabaseConnection>().getConnection()
let mapError res (transaction: NpgsqlTransaction) =
    res
    |> TaskResult.mapError (fun ex ->
        transaction.Rollback()
        InternalError ex) 

let decodeRecipeAndIngredientHelper (context: HttpContext) =
    taskResult {
        let! body = context.ReadBodyFromRequestAsync()
        let! decodeRecipe =
            Decode.fromString Types.Recipe.decoder body
            |> Result.mapError BadRequest
        let! decodeIngredient =
            Decode.fromString Types.Ingredient.decodeList body
            |> Result.mapError BadRequest
        
        return decodeRecipe, decodeIngredient
    }
    
let getRecipes: HttpHandler =
    fun (next: HttpFunc) (context: HttpContext) ->
        let result =
            taskResult {
                let connection = getDatabaseConnection context
                use transaction = connection.BeginTransaction()
                let! recipes = 
                    Database.getAllRecipes transaction
                    |> TaskResult.mapError (fun ex ->
                        transaction.Rollback()
                        InternalError ex)
                transaction.Commit()
                let encodedRecipes = Seq.map Types.encodeRecipeAndIngredient recipes
                return encodedRecipes
            }
        httpStatusResult result next context
        
let postRecipe: HttpHandler =
    fun (next: HttpFunc) (context: HttpContext) ->
        let result =
            taskResult {
                let! recipe, ingredients = decodeRecipeAndIngredientHelper context
                let connection = getDatabaseConnection context
                use transaction = connection.BeginTransaction()
                let! newRecipe =
                    Database.addRecipe recipe ingredients transaction
                    |> TaskResult.mapError (fun ex ->
                        transaction.Rollback()
                        InternalError ex)
                transaction.Commit()
                let encodedRecipe = Types.encodeRecipeAndIngredient newRecipe
                return encodedRecipe
            }
        httpStatusResult result next context
        
let putRecipe: HttpHandler =
     fun (next: HttpFunc) (context: HttpContext) ->
        let result = 
            taskResult {
                let! recipe, ingredients = decodeRecipeAndIngredientHelper context
                let connection = getDatabaseConnection context
                use transaction = connection.BeginTransaction()
                let! updatedRecipe =
                    Database.updateRecipe recipe ingredients transaction
                    |> TaskResult.mapError (fun ex ->
                        transaction.Rollback()
                        InternalError ex)
                transaction.Commit()
                let encodedRecipe = Types.encodeRecipeAndIngredient updatedRecipe
                return encodedRecipe
            }
        httpStatusResult result next context
        
let deleteRecipe (id: System.Guid): HttpHandler =
    fun (next: HttpFunc) (context: HttpContext) ->
        let result =
            taskResult {
                let connection = getDatabaseConnection context
                let! _ = Database.deleteRecipe id connection
                         |> TaskResult.mapError InternalError
                return $"Deleted recipe with id: {id}"
            }
        httpStatusResult result next context