# F# Friday 5

Hei og velkommen til den femte posten i en serie om programmeringsspråket F#!

Lenker til tidligere artikler:
- Del 1: [Introduksjon](https://blogg.bekk.no/f-friday-1-39f63618d2e4)
- Del 2: [Typesystemet](https://blogg.bekk.no/f-friday-2-typesystemet-3e7ee0554f0e)
- Del 3: [Backenden](https://blogg.bekk.no/f-friday-3-backend-7463edf0f94a)
- Del 4: [Frontent og React](https://blogg.bekk.no/f-friday-4-frontend-og-react-c356d34a6095)
- **Del 5: Databaser**

[Forrige gang](https://blogg.bekk.no/f-friday-4-frontend-og-react-c356d34a6095) lagde vi en enkelt frontend i Feliz som kunne snakke med backenden vår. Når vi først lagde backenden vår for en stund siden valgte vi å brukte vi en dictionary til å holde på alle oppskriftene våre. Nå, etter en litt lengre pause, skal vi bytte ut denne in-memory databasen med PostgreSQL.

## Verktøy
F# har ganske mange forskjellige verktøy man kan bruke for å jobbe med databaser. De i listen under under er de jeg selv har erfaring med, men du kan godt google litt om du vil ha enda flere alternativer.

- [SqlProvider](https://github.com/fsprojects/SQLProvider) - Genererer typer fra databasen din mens du programmerer. Det betyr at koden din ikke vil kompilere dersom du har skrevet feil SQL. Behøver en aktiv tilgang til din database for å bygge koden din, som kan være litt irriterende å sette opp. Men når dette fungerer er det en uslåelig utvikleropplevelse.
- [Entity Framework]()(EF) - Selv om EF er mest kjent som et C# bibliotek fungerer det i F# også, men det er ikke like ergonomisk å jobbe med som noen av alternativene. I tillegg så liker jeg ikke å abstrahere bort SQL for et dårlig DSL.
- [RepoDb](https://github.com/mikependon/RepoDB) - Gir et enklere EF lignende API uten mye av de ekstra greiene som EF tar med, men har mange av de samme problemene.

Vi skal gjøre det veldig enkelt og bruke Dapper. Gode gamle Dapper!

## Dapper
[Dapper](https://github.com/DapperLib/Dapper) er, som de selv sier, en "simple object mapper for .Net". Det er en lettvekts måte å kommunisere med databaser på og vi kan bruke dette rett i F#.

La oss si vi har en addressebok som har en tabell og databasemodell som ser slik ut:
```fsharp
type AddressDbModel {
    Name: string
    Address: string
}
```

Så kan vi bruke Dapper til å skrive en insert spørring slik:
```fsharp
let name = "Someone"
let address = "Somewhere Road"
let query = 
    "
    INSERT INTO Addresses (name, address)
    VALUES (@name, @address)
    RETURNING *;
    "
let parameters = dict [
        "name", box name
        "address, box address
    ]
connection.query<AddressDbModel>(query, parameter)
```

Vi starter med å lage noen variabler som holder på dataen vi ønske å inserte.
Så lager vi selve spørringen. Når du gir Dapper parametrene gir du de et vilkårlig navn som starter med @. Disse blir så erstattet av Dapper senere, det vil du at Dapper dealer med så du slipper SQL injections.

Deretter lager vi en dictionary, `Dictionary<string,obj>`, som består av key/value pairs. Hvor keyen er den samme stringen vi bruker for parametrene til spørringen men uten @
og value er den dataen du ønsker å sende inn. Da Dapper forventer `obj` datatypen, bruker vi `box` funksjonen.
Her bruker vi også PostgreSql sin `RETURNING` statement som betyr at den returnerer det som insertes. Da slipper vi å gjøre en ekstra spørring for å hente dataen ut igjen.

[//]: # (## Forbered sjiraffen)

[//]: # ()
[//]: # (Alle disse blokkene returnerer tasks, så la oss skrive om Giraffe koden vår til å bruke tasks også. Da blir det enkelt å få disse to biblioteken til å snakke sammen. Fra forrige gang mangler `getRecipes` og `deleteRecipe` tasks. Det legger vi enkelt til og HttpHandlerene våre ser nå slik ut:)

[//]: # (```fsharp)

[//]: # (let getRecipes: HttpHandler =)

[//]: # (    fun &#40;next: HttpFunc&#41; &#40;context: HttpContext&#41; ->)

[//]: # (        task {)

[//]: # (            let! recipes = Database.getAllRecipes &#40;&#41;)

[//]: # (          return! json recipes next context)

[//]: # (        })

[//]: # (let deleteRecipe &#40;id: System.Guid&#41; : HttpHandler =)

[//]: # (    fun &#40;next: HttpFunc&#41; &#40;context: HttpContext&#41; ->)

[//]: # (        task {)

[//]: # (            do! Database.deleteRecipe id)

[//]: # (            return! text $"Deleted recipe with id: {id}" next context)

[//]: # (        })

[//]: # (```)

## Modeller og funksjoner
Nå som vi vet hvordan Dapper fungerer kan vi lage noen hjelpefunksjoner som gjør det enklere å konvertere mellom datamodellene våre.

Det første vi trenger er noen databasemodeller. Altså typer som sier hvordan dataen vår faktisk ser ut i databasen. Denne typen må matche den tabellen den tilhører.
Det vi også kommer til å trenge er funksjoner som konverterer mellom de forskjellige modell-typene våre. Altså fra domene-modell til database-modell og tilbake igjen.

Vi starter med databasemodellene våre!

For oppskrifter:
```fsharp
// Databasemodell
[<CLIMutable>]
type RecipeDbModel =
    { Id: string
      Title: string
      Description: string
      Meal: string
      Time: float
      Steps: string array
      Portions: int }

// Domene til databasemodell
let private recipeToDb (recipe: Recipe) =
    { Id = recipe.Id.ToString()
      Title = recipe.Title
      Description = recipe.Description
      Meal = recipe.Meal |> stringifyMeal
      Time = recipe.Time
      Steps = List.toArray recipe.Steps
      Portions = recipe.Portions }

// Database til domenemodell
let private recipeDbToDomain (recipe: RecipeDbModel) ingredients =
    { Id = Guid.Parse(recipe.Id)
      Title = recipe.Title
      Description = recipe.Description
      Meal = stringToMeal recipe.Meal
      Time = recipe.Time
      Steps = Array.toList recipe.Steps
      Ingredients = ingredients
      Portions = recipe.Portions }
```

For ingredienser:
```fsharp
// Databasemodell
[<CLIMutable>]
type IngredientDbModel =
    { Volume: float
      Measurement: string
      Recipe: string // Fremmednøkkel til oppskrift }

// Domene til databasemodell
let private ingredientToDb (ingredient: Ingredient) recipeId =
    { Volume = ingredient.Volume
      Measurement = ingredient.Measurement |> measurementToString
      Name = ingredient.Name
      Recipe = recipeId }

// Database til domenemodell
let private ingredientDbToDomain (ingredient: IngredientDbModel) =
    { Volume = ingredient.Volume
      Measurement = stringToMeasurement ingredient.Measurement
      Name = ingredient.Name }
```

Disse er ganske straight forward typer og mappingsfunksjoner. Men den flittige leser la kanskje merke til attributten `[<CLIMutable>]`.
Records i F# blir tilslutt til nedstrippede klasser når all vår kode kompileres. Klasser som opprettes fra records får ingen constructors, men det er noe Dapper trenger.
Det attributten gjør er å lage en default constructor for den endelige nedstrippede klassen vår. Dette er noe vi ikke trenger å ha noe forholde til.

Vi kan også lage en liten hjelpefunksjon som tar inn en oppskrift of spytter ut databasemodeller til både oppskrifter og ingredienser.
Når vi nå skal legge til en ny oppskrift har vi kun en funksjon vi trenger å kalle.
```fsharp
let private recipeToDbModels recipe =
    let recipeToInsert = recipeToDb recipe

    let ingredientsToInsert =
        recipe.Ingredients
        |> List.map (fun i -> ingredientToDb i recipeToInsert.Id)

    recipeToInsert, ingredientsToInsert
```

## Slafs: Database time
La oss starte med å hente ut alle oppskriftene våre. Slett hele fakbase, in-memory databasen, vi hadde fra før.
Vi lager en ny funksjon som også heter `getAllRecipes`.
Det vi trenger å gjøre der er å hente alle oppskriftene og alle ingrediensene og så kombinere dem til domenemodeller.

```fsharp
let getAllRecipes () =
    task {
        use connection = new NpgsqlConnection(connectionString)
        let recipes =
            let query =
                "
                SELECT * FROM
                Recipe
                "
            try
                connection.Query<RecipeDbModel>(query)
                |> Seq.toList
                |> Ok
            with
                | ex -> Error ex.Message
```

I første del av funksjonen lager vi en connection til Postgres databasen vår. Så oppretter vi en query som vi prøver å hente oppskrifter fra.
Dersom alt går fint pakker vi resultatet inn i en Ok, dersom det feiler pakker vi resultatet inn i en Error. Så kan vi deale resultatet senere.
Her ser vi også at vi bruker keywordet `use`. `use` gjør akkurat det samme som `let` men brukes gjerne sammen med `IDisposable` for å sikre automatisk opprydding så snart den går ut av scope. NB: man må fortsatt manuelt stenge connection om man vil at den skal stenges.

Vi gjør noe tilsvarende for ingrediensene, men her stenger vi connection etter vi er ferdig.
```fsharp
        let ingredients =
            let query =
                "
                SELECT * FROM
                Ingredient
                "
            try
                connection.Query<IngredientDbModel>(query)
                |> Seq.toList
                |> Ok
            with
                | ex -> Error ex.Message
        connection.Close()
```

På dette tidspunket må vi deale med resultatet av database-oppslagene våre.
Enten så kan det ha gått greit for oppskriftene og ingrediensene, eller så har det skjedd en feil med en, eller begge av de.
Det kan vi enkelt sjekke med pattern matching, så kan vi også passe på å skrive gode feilmeldinger.

```fsharp
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
```

Neste funksjon blir å legge til nye oppskrifter.
Her tar vi inn en oppskrift og kaller `recipeToDbModels` funksjonen vår.
Da får vi alle oppskriftene og ingrediensene våre. De inserter vi rett inn i databasen slik som vi så i eksempelet tidligere.
Her bruker vi også en transaksjon når vi legger til, for sikkerhets skyld.
`addRecipe` blir da:
```fsharp
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
        
        use connection = new NpgsqlConnection(connectionString)
        connection.Open()
        use transaction = connection.BeginTransaction()
        let result =
            try
                let recipe = transaction.Connection.Query<RecipeDbModel>(recipeQuery, recipeParameters, transaction) |> Seq.toList
                transaction.Connection.Execute(insertIngredientQuery, ingredientsToInsert, transaction) |> ignore
                let ingredients = transaction.Connection.Query<IngredientDbModel>(getIngredientsQuery, ingredientRecipeIdParameters, transaction) |> Seq.toList
                transaction.Commit()
                recipeAndIngredientDbModelsToDomain recipe ingredients
                |> List.head
                |> Ok
            with
                | ex ->
                    transaction.Rollback()
                    Error ex.Message
                    
        connection.Close()
                    
        return result
    
```

Når det kommer til å oppdatere oppskrifter så starter vi med å oppdatere selve oppskriften,
Deretter sletter vi alle ingrediense og legger de til på nytt.
Shazam! Oppdatert oppskrift.

```fsharp
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
            
        use connection = new NpgsqlConnection(connectionString)
        connection.Open()
        use transaction = connection.BeginTransaction()
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
        
        connection.Close()
        
        return result
    }
```

Avslutningsvis trenger vi bare å slette basert oppskrifter og ingredienser basert på de IDene vi har.
Ez pz
```fsharp
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
        
        use connection = new NpgsqlConnection(connectionString)
        let result =
            try
                connection.Execute(query, parameters) |> ignore
                Ok ()
            with
                | ex ->
                    Error ex.Message
                    
        connection.Close()
        
        return result
    }
```

## Helt til slutt
Det vi trenger å gjøre helt til slutt er å endre litt på hvordan vi kaller disse funksjonene fra handlerene våre.
Nå returnerer nemmelig funksjonene et resultat vi må deale med.
For å vise hvordan dette kan gjøres så viser jeg kun `postRecipe` her. Så kan de andre være en hjemmelekse!

```fsharp
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
```

Her har vi en task som prøver å konvertere JSON vi får inn til oppskriftstypen vår.
Så kaller vi databaselager, og basert på resultatet lager vi retur-verdien.
Akkurat nå setter vi bare statuskoden til å være 400, uansett hva som skjer. Det er jo noe vi burde forbedre.

Neste gang skal vi se nærmere på hvordan vi kan sikre at det å konvertere JSON til domenetype ikke kan feile,
og at vi være sikre på at dataen vi får inn der gir mening.