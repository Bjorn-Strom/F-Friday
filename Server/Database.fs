module Database

open Shared

open System
open Dapper.FSharp
open Dapper.FSharp.PostgreSQL
open Npgsql

let connectionString =
    let credentials =
        let databaseUrl =
            Environment.GetEnvironmentVariable "DATABASE_URL"

        let usernamePassword =
            (databaseUrl.Split("@")[0])
                .Replace("postgres://", "")

        let hostPortDatabase = databaseUrl.Split("@")[1]
        let hostPort = hostPortDatabase.Split("/")[0]

        {| Database = hostPortDatabase.Split("/")[1]
           User = usernamePassword.Split(":")[0]
           Password = usernamePassword.Split(":")[1]
           Host = hostPort.Split(":")[0]
           Port = hostPort.Split(":")[1] |}

    $"User ID={credentials.User};Password={credentials.Password};Host={credentials.Host};Port={credentials.Port};Database={credentials.Database};Pooling=true;SSL Mode=Require;TrustServerCertificate=True;"

let connection = new NpgsqlConnection(connectionString)

[<CLIMutable>]
type RecipeDbModel =
    { Id: string
      Title: string
      Description: string
      Meal: string
      Time: float
      Steps: string array
      Portions: int }

[<CLIMutable>]
type IngredientDbModel =
    { Volume: float
      Measurement: string
      Name: string
      Recipe: string }

let recipeTable = table'<RecipeDbModel> "Recipe"
let ingredientTable = table'<IngredientDbModel> "Ingredient"

let private recipeToDb (recipe: Recipe) =
    { Id = recipe.Id.ToString()
      Title = recipe.Title
      Description = recipe.Description
      Meal = recipe.Meal |> stringifyMeal
      Time = recipe.Time
      Steps = List.toArray recipe.Steps
      Portions = recipe.Portions }

let private recipeDbToDomain (recipe: RecipeDbModel) ingredients =
    { Id = Guid.Parse(recipe.Id)
      Title = recipe.Title
      Description = recipe.Description
      Meal = stringToMeal recipe.Meal
      Time = recipe.Time
      Steps = Array.toList recipe.Steps
      Ingredients = ingredients
      Portions = recipe.Portions }

let private ingredientToDb (ingredient: Ingredient) recipeId =
    { Volume = ingredient.Volume
      Measurement = ingredient.Measurement |> measurementToString
      Name = ingredient.Name
      Recipe = recipeId }

let private ingredientDbToDomain (ingredient: IngredientDbModel) =
    { Volume = ingredient.Volume
      Measurement = stringToMeasurement ingredient.Measurement
      Name = ingredient.Name }

let private recipeToDbModels recipe =
    let recipeToInsert = recipeToDb recipe

    let ingredientsToInsert =
        recipe.Ingredients
        |> List.map (fun i -> ingredientToDb i recipeToInsert.Id)

    recipeToInsert, ingredientsToInsert

let getAllRecipes () =
    task {
        let! recipeDbModels =
            select {
                for r in recipeTable do
                    selectAll
            }
            |> connection.SelectAsync<RecipeDbModel>

        let! ingredientDbModels =
            select {
                for i in ingredientTable do
                    selectAll
            }
            |> connection.SelectAsync<IngredientDbModel>

        let recipeDomainModels =
            Seq.map
                (fun r ->
                    let ingredientsForRecipe =
                        ingredientDbModels
                        |> Seq.filter (fun i -> i.Recipe = r.Id)
                        |> Seq.map ingredientDbToDomain
                        |> Seq.toList

                    recipeDbToDomain r ingredientsForRecipe)
                recipeDbModels
            |> Seq.toList

        printfn $"Got {List.length recipeDomainModels} recipe(s)"

        return recipeDomainModels
    }

let addRecipe newRecipe =
    task {
        let recipeToInsert, ingredientsToInsert = recipeToDbModels newRecipe

        let! insertedRecipe =
            insert {
                into recipeTable
                value recipeToInsert
            }
            |> connection.InsertAsync

        printfn $"Inserted {insertedRecipe} recipe(s)"

        let! insertedIngredients =
            insert {
                into ingredientTable
                values ingredientsToInsert
            }
            |> connection.InsertAsync

        printfn $"Inserted {insertedIngredients} ingredient(s)"
    }

let updateRecipe recipeToUpdate =
    task {
        let recipeToUpdate, ingredientsToUpdate = recipeToDbModels recipeToUpdate

        let! updatedRecipe =
            update {
                for r in recipeTable do
                    set recipeToUpdate
                    where (r.Id = recipeToUpdate.Id)
            }
            |> connection.UpdateAsync

        printfn $"Updated {updatedRecipe} recipe(s)"

        // For å oppdatere alle ingrediense så sletter vi de gamle og inserter de nye
        let! deletedIngredients =
            delete {
                for i in ingredientTable do
                    where (i.Recipe = recipeToUpdate.Id)
            }
            |> connection.DeleteAsync

        printfn $"Deleted {deletedIngredients} ingredients."

        let! insertedIngredients =
            insert {
                into ingredientTable
                values ingredientsToUpdate
            }
            |> connection.InsertAsync

        printfn $"Inserted {insertedIngredients} ingredient(s)"
    }

let deleteRecipe (id: Guid) =
    task {
        let id = id.ToString()

        let! ingredientsDeleted =
            delete {
                for i in ingredientTable do
                    where (i.Recipe = id)
            }
            |> connection.DeleteAsync

        printfn $"Deleted {ingredientsDeleted} ingredient(s)"

        let! recipeDeleted =
            delete {
                for r in recipeTable do
                    where (r.Id = id)
            }
            |> connection.DeleteAsync

        printfn $"Deleted {recipeDeleted} recipe(s)"
    }
