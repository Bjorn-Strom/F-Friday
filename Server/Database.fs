module Database

open System
open Dapper
open Npgsql

open Types

type GuidHandler () =
    inherit SqlMapper.TypeHandler<Guid>()
    
    override this.Parse(value) =
        Guid.Parse(string value)
        
    override this.SetValue(parameter, guid) =
        parameter.Value <- guid.ToString()
        
type MealHandler () =
    inherit SqlMapper.TypeHandler<Shared.Meal>()
    
    override this.Parse(value) =
        let value = string value
        Shared.stringToMeal value
        |> function
            | Some meal -> meal
            | None -> failwith $"{value} is not a valid meal type."
            
    override this.SetValue(parameter, meal) =
        parameter.Value <- meal.ToString()
        
type MeasurementHandler () =
    inherit SqlMapper.TypeHandler<Shared.Measurement>()
    
    override this.Parse(value) =
        let value = string value
        Shared.stringToMeasurement value
        |> function
            | Some measurement -> measurement
            | None -> failwith $"{value} is not a valid measurement type."
            
    override this.SetValue(parameter, measurement) =
        parameter.Value <- measurement.ToString()
        
        
let addTypeHandlers () =
    SqlMapper.RemoveTypeMap(typeof<Guid>)
    SqlMapper.AddTypeHandler(GuidHandler())
    SqlMapper.AddTypeHandler(MealHandler())
    SqlMapper.AddTypeHandler(MeasurementHandler())

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

let getAllRecipes (connection: NpgsqlConnection) =
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
                connection.QueryAsync(
                    query,
                    (fun (recipe: Recipe) (ingredient: Ingredient option) ->
                        recipes <- recipes@[recipe]
                        if ingredient.IsSome then
                            ingredients <- ingredients@[ingredient.Value]))
                
            let ingredients =
                ingredients
                |> List.groupBy (fun i -> i.Recipe)
                |> Map.ofList
            let recipes =
                recipes
                |> Seq.distinct
                |> Seq.map (fun r ->
                    r, List.toSeq ingredients[r.Id])
                
            return Ok recipes
        with
            | ex -> return Error ex
    }

let addRecipe recipe ingredients (transaction: NpgsqlTransaction) =
    task {
        let insertRecipeQuery =
            "
            INSERT INTO Recipe (id, description, meal, portions, steps, title, time)
            VALUES (@id, @description, @meal, @portions, @steps, @title, @time)
            RETURNING *;
            "
        let recipeParameters = dict [
            "id", box recipe.Id
            "description", box recipe.Description
            "meal", box recipe.Meal
            "portions", box recipe.Portions
            "steps", box recipe.Steps
            "title", box recipe.Title
            "time", box recipe.Time
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
        let ingredientRecipeIdParameters = dict [ "recipe", box recipe.Id ]
        
        try
            let! recipe = transaction.Connection.QuerySingleAsync<Recipe>(insertRecipeQuery, recipeParameters, transaction)
            let! _ = transaction.Connection.ExecuteAsync(insertIngredientQuery, ingredients, transaction)
            let! ingredients = transaction.Connection.QueryAsync<Ingredient>(getIngredientsQuery, ingredientRecipeIdParameters, transaction)
            
            return Ok (recipe, ingredients)
        with
            | ex -> return Error ex
    }

let updateRecipe recipe ingredients (transaction: NpgsqlTransaction) =
    task {
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
            "id", box recipe.Id
            "description", box recipe.Description
            "meal", box recipe.Meal
            "portions", box recipe.Portions
            "steps", box recipe.Steps
            "title", box recipe.Title
            "time", box recipe.Time
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
        let ingredientParameters = dict [ "recipe", box recipe.Id ]
            
        try
            let! recipe = transaction.Connection.QuerySingleAsync<Recipe>(recipeQuery, recipeParameters, transaction)
            let! _ = transaction.Connection.ExecuteAsync(updateIngredientQuery, ingredients, transaction)
            let! ingredients = transaction.Connection.QueryAsync<Ingredient>(getIngredientsQuery, ingredientParameters, transaction)
            return Ok (recipe, ingredients)
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
            "recipeId", box id
        ]
        
        try
            let! numberOfRowsDeleted = connection.ExecuteAsync(query, parameters)
            return Ok numberOfRowsDeleted
        with
            | ex -> return Error ex
    }