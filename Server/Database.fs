module Database

open System
open Dapper
open Npgsql

open Shared

// Bare no ræl for å parse database url vi får fra heroku
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
    
let createConnection () =
    let connection = new NpgsqlConnection(connectionString)
    connection.Open()
    connection
    
let cleanupConnection (connection: NpgsqlConnection) =
    connection.Close()
    connection.Dispose()
    
let createTransaction () =
    let connection = createConnection ()
    let transaction = connection.BeginTransaction()
    
    transaction
    
let cleanupTransaction (transaction: NpgsqlTransaction) =
    cleanupConnection transaction.Connection
    transaction.Dispose()
    
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
    
let private recipeAndIngredientDbModelsToDomain recipes ingredients =
     recipes
     |> List.map
        (fun r ->
            let ingredientsForRecipe =
                ingredients
                |> List.filter (fun i -> i.Recipe = r.Id)
                |> List.map ingredientDbToDomain

            recipeDbToDomain r ingredientsForRecipe)
    
let getAllRecipes () =
    task {
        let transaction = createTransaction ()
        let recipes =
            let query =
                "
                SELECT * FROM
                Recipe
                "
            try
                transaction.Connection.Query<RecipeDbModel>(query)
                |> Seq.toList
                |> Ok
            with
                | ex -> Error ex.Message
                

        let ingredients =
            let query =
                "
                SELECT * FROM
                Ingredient
                "
            try
                transaction.Connection.Query<IngredientDbModel>(query)
                |> Seq.toList
                |> Ok
            with
                | ex -> Error ex.Message
                
        cleanupTransaction transaction
                
        return
            match recipes, ingredients with
            | Ok recipes, Ok ingredients ->
                let recipeDomainModels = recipeAndIngredientDbModelsToDomain recipes ingredients

                printfn $"Got {List.length recipeDomainModels} recipe(s)"

                Ok recipeDomainModels
            | Error recipeError, Ok _ -> Error $"Feil under henting av oppskrifter: {recipeError}"
            | Ok _, Error ingredientError ->  Error $"Feil under henting av ingredienser: {ingredientError}"
            | Error recipeError, Error ingredientError ->
                [
                    $"Feil under henting av oppskrifter: {recipeError}."
                    $"Feil under henting av ingredienser: {ingredientError}."
                ]
                |> String.concat ""
                |> Error
    }
    
let addRecipe newRecipe =
    task {
        let recipeToInsert, ingredientsToInsert = recipeToDbModels newRecipe
        let recipeQuery =
            "
            INSERT INTO Recipe (id, description, meal, portions, steps, title, time)
            VALUES (@id, @description, @meal, @portions, @steps, @title, @time)
            RETURNING *;
            "
        let recipeParameters = dict [
            "id", box recipeToInsert.Id
            "description", box recipeToInsert.Description
            "meal", box recipeToInsert.Meal
            "portions", box recipeToInsert.Portions
            "steps", box recipeToInsert.Steps
            "title", box recipeToInsert.Title
            "time", box recipeToInsert.Time
        ]
        
        let ingredientParameters = DynamicParameters()

        let ingredientValues =
            ingredientsToInsert
            |> List.mapi (fun i ingredient ->
                ingredientParameters.Add($"volume{i}", ingredient.Volume)
                ingredientParameters.Add($"measurement{i}", ingredient.Measurement)
                ingredientParameters.Add($"name{i}", ingredient.Name)
                ingredientParameters.Add($"recipe{i}", ingredient.Recipe)
                $"(@volume{i}, @measurement{i}, @name{i}, @recipe{i})")
        
        let ingredientQuery =
            $"""
            INSERT INTO Ingredient (volume, measurement, name, recipe)
            VALUES {String.concat "," ingredientValues}
            RETURNING *;
            """
        
        let transaction = createTransaction ()
        let result =
            try
                let recipe = transaction.Connection.Query<RecipeDbModel>(recipeQuery, recipeParameters, transaction) |> Seq.toList
                let ingredients = transaction.Connection.Query<IngredientDbModel>(ingredientQuery, ingredientParameters, transaction) |> Seq.toList
                transaction.Commit()
                recipeAndIngredientDbModelsToDomain recipe ingredients
                |> List.head
                |> Ok
            with
                | ex ->
                    transaction.Rollback()
                    Error ex.Message
                    
        cleanupTransaction transaction
                    
        return result
    }
let updateRecipe recipeToUpdate =
    task {
        let recipeToUpdate, ingredientsToUpdate = recipeToDbModels recipeToUpdate
        let recipeQuery =
            "
            UPDATE Recipe
            SET description = @description,
                meal = @meal,
                portions = @portions,
                steps = @steps,
                title = @title,
                time = @time
            WHERE id = @id
            RETURNING *;
            "
            
        let recipeParameters = dict [
            "id", box recipeToUpdate.Id
            "description", box recipeToUpdate.Description
            "meal", box recipeToUpdate.Meal
            "portions", box recipeToUpdate.Portions
            "steps", box recipeToUpdate.Steps
            "title", box recipeToUpdate.Title
            "time", box recipeToUpdate.Time
        ]
        
        let deleteIngredientsQuery =
            "
            DELETE FROM Ingredient
            WHERE Recipe = @recipe;
            "
        let insertIngredientQuery =
            "
            INSERT INTO Ingredient (volume, measurement, name, recipe)
            VALUES (@volume, @measurement, @name, @recipe)
            "
        let getIngredientsQuery =
            "
            SELECT * FROM Ingredient
            WHERE Recipe = @recipe;
            "
        let ingredientRecipeIdParameters = dict [ "recipe", box recipeToUpdate.Id ]
            
        let transaction = createTransaction ()
        let result =
            try
                let recipe = transaction.Connection.Query<RecipeDbModel>(recipeQuery, recipeParameters, transaction) |> Seq.toList
                transaction.Connection.Execute(deleteIngredientsQuery, ingredientRecipeIdParameters, transaction) |> ignore
                transaction.Connection.Execute(insertIngredientQuery, ingredientsToUpdate, transaction) |> ignore
                let ingredients = transaction.Connection.Query<IngredientDbModel>(getIngredientsQuery, ingredientRecipeIdParameters, transaction) |> Seq.toList
                transaction.Commit()
                recipeAndIngredientDbModelsToDomain recipe ingredients
                |> List.head
                |> Ok
            with
                | ex ->
                    transaction.Rollback()
                    Error ex.Message
                    
        cleanupTransaction transaction
        
        return result
    }

let deleteRecipe (id: Guid) =
    task {
        let query =
            "
            DELETE FROM
            Ingredient
            WHERE recipe = @recipeId;
            
            DELETE FROM
            Recipe
            WHERE id = @recipeId; 
            "
            
        let parameters = dict [
            "recipeId", box (id.ToString())
        ]
        
        let connection = createConnection ()
        let result =
            try
                connection.Execute(query, parameters) |> ignore
                Ok ()
            with
                | ex ->
                    Error ex.Message
                    
        cleanupConnection connection
        
        return result

    }
