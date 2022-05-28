module Database

open System
open Dapper
open Npgsql

open Types

type DatabaseConnection (connectionString: string) =
    let connection: NpgsqlConnection = new NpgsqlConnection(connectionString)
    member this.getConnection () =
        connection.Open()
        connection
    interface IDisposable with
        member __.Dispose() =
            connection.Close()
            connection.Dispose()

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

let getAllRecipes (transaction: NpgsqlTransaction) =
    task {
        let query =
            "
            SELECT * FROM Recipe
            INNER JOIN ingredient i on recipe.id = i.recipe
            "
            
        let mutable recipes = []
        let mutable ingredients = []
        try
            let! _ =
                transaction.Connection.QueryAsync(
                    query,
                    (fun (recipe: RecipeDbModel) (ingredient: IngredientDbModel) ->
                        recipes <- recipes@[recipe]
                        if (ingredient :> obj <> null) then
                            ingredients <- ingredients@[ingredient]),
                    transaction = transaction)
                
            let ingredients =
                ingredients
                |> List.groupBy (fun i -> i.Recipe)
                |> Map.ofList
            let recipes =
                recipes
                |> List.map (fun r -> r, ingredients[r.Id])
                
            return Ok recipes
        with
            | ex -> return Error ex
    }
    

let addRecipe recipe ingredients (transaction: NpgsqlTransaction) =
    task {
        let recipeToInsert, ingredientsToInsert = recipeToDbModels recipe ingredients
        let insertRecipeQuery =
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
        let ingredientRecipeIdParameters = dict [ "recipe", box recipeToInsert.Id ]
        
        try
            let! recipe = transaction.Connection.QuerySingleAsync<RecipeDbModel>(insertRecipeQuery, recipeParameters, transaction)
            let! _ = transaction.Connection.ExecuteAsync(insertIngredientQuery, ingredientsToInsert, transaction)
            let! ingredients = transaction.Connection.QueryAsync<IngredientDbModel>(getIngredientsQuery, ingredientRecipeIdParameters, transaction)
            
            return Ok (recipe, ingredients |> Seq.toList)
        with
            | ex -> return Error ex
    }

let updateRecipe recipe ingredients (transaction: NpgsqlTransaction) =
    task {
        let recipeToUpdate, ingredientsToUpdate = recipeToDbModels recipe ingredients
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
        
        let updateIngredientQuery =
            "
            UPDATE Ingredient
            SET volume = @volume,
                measurement = @measurement,
                name = @name,
                recipe = @recipe
            WHERE recipe = @recipe
            "
        let getIngredientsQuery =
            "
            SELECT * FROM Ingredient
            WHERE Recipe = @recipe;
            "
        let ingredientParameters = dict [ "recipe", box recipeToUpdate.Id ]
            
        try
            let! recipe = transaction.Connection.QuerySingleAsync<RecipeDbModel>(recipeQuery, recipeParameters, transaction)
            let! _ = transaction.Connection.ExecuteAsync(updateIngredientQuery, ingredientsToUpdate, transaction)
            let! ingredients = transaction.Connection.QueryAsync<IngredientDbModel>(getIngredientsQuery, ingredientParameters, transaction)
            return Ok (recipe, ingredients |> Seq.toList)
        with
            | ex -> return Error ex
    }

let deleteRecipe (id: Guid) (connection: NpgsqlConnection) =
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
        
        try
            let! _ = connection.ExecuteAsync(query, parameters)
            return Ok ()
        with
            | ex -> return Error ex
    }