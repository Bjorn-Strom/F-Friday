# F# Friday 6

Hei og velkommen til den sjett posten i en serie om programmeringsspråket F#!

Lenker til tidligere artikler:
- Del 1: [Introduksjon](https://blogg.bekk.no/f-friday-1-39f63618d2e4)
- Del 2: [Typesystemet](https://blogg.bekk.no/f-friday-2-typesystemet-3e7ee0554f0e)
- Del 3: [Backenden](https://blogg.bekk.no/f-friday-3-backend-7463edf0f94a)
- Del 4: [Frontent og React](https://blogg.bekk.no/f-friday-4-frontend-og-react-c356d34a6095)
- Del 5: [JSON-Dekodere](https://blogg.bekk.no/f-friday-5-json-dekodere-f2e6b4fe99c9)
- **Del 6: Databaser**

[Forrige gang](https://blogg.bekk.no/f-friday-5-json-dekodere-f2e6b4fe99c9) endret vi hvordan backenden fungerte. Vi tok nemmelig i bruk dekodere for å passe på at dataen vi får inn er riktig formatert. Denne dataen blir gjort on til datatypene våre og ble lagret i en in-memory database ved navn `Fakabase`.
Denne gangen skal vi endelig gjøre det - vi skal bytte til å bruke en faktisk database. 
Som en liten bonus skal vi også bruke et bibliotek som heter `FsToolkit.Errorhandling` så vi slipper å må matche på hver eneste result type vi har. Det vil gjøre koden vår mye mer lettleselig.

Dette blir spennende for vi skal nemmelig bruke muterende variabler, skrive klasser og bruke dependency injection! Dette blir helt villt, så følg med!

## Verktøy
F# har ganske mange forskjellige verktøy man kan bruke for å jobbe med databaser. De i listen under under er de jeg selv har erfaring med, men du kan godt google litt om du vil ha enda flere alternativer.

- [SqlProvider](https://github.com/fsprojects/SQLProvider) - Genererer typer fra databasen din mens du programmerer. Det betyr at koden din ikke vil kompilere dersom du har skrevet feil SQL. Behøver en aktiv tilgang til din database for å bygge koden din, som kan være litt irriterende å sette opp. Men når dette fungerer er det en uslåelig utvikleropplevelse.
- [Entity Framework]()(EF) - Selv om EF er mest kjent som et C# bibliotek fungerer det i F# også, men det er ikke like ergonomisk å jobbe med som noen av alternativene. I tillegg så liker jeg ikke å abstrahere bort SQL for et dårlig DSL.
- [RepoDb](https://github.com/mikependon/RepoDB) - Gir et enklere EF lignende API uten mye av de ekstra greiene som EF tar med, men har mange av de samme problemene.
- [Donald](https://github.com/pimbrouwers/Donald) - Tilbyr et lite lag med funksjoner for å gjøre det enklere å bruke Dapper i F#.

Vi skal gjøre det veldig enkelt og kun bruke Dapper.

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
let query = 
    "
    INSERT INTO Addresses (name, address)
    VALUES (@name, @address)
    RETURNING *;
    "
let parameters = dict [
        "name", box "Someone"
        "address, box "Somewhere"
    ]
connection.query<AddressDbModel>(query, parameter)
```

Her lager vi en spørring som legger til navn og adresse inn i addresseboka vår.
Legg merke til at parametre i Dapper starter med en '@'.

Deretter lager vi en dictionary, `Dictionary<string,obj>`, som består av key/value pairs.
Hvor keyen er den samme stringen vi bruker for parametrene til spørringen men uten '@' denne gangen.
Vi bruker funksjonen ´box´ på parametrene våre for å få dem gjort om til en `obj` type.

## F# og Dapper
Okei, da sletter vi `Fakabase` - all koden i `Database.fs` må gå!
Det vi ønsker å oppnå er å lagre typene vi opprettet forrige gang i databasen. Jeg har allerede lagd tabeller som matcher.

Det første vi må gjøre er å fikse et lite problem. Dapper forstår seg ikke på F# typer, vi må skrive egne klasser som dealer med konvertering fra F# type til noe Postgres kan forstå - og tilbake igjen.
Disse klassene kalles for `TypeHandlers` og er veldig enkle å lage. Vi trenger 3 av dem: `GuidHandler`, `MealHandler` og `MeasurementHandler`.
Så lag en klasse om arver fra `SqlMapper.Typehandler<T>` og implementerer metodene `Parse`, postgres -> F#, og `SetValue`, F# -> postgres.

I vårt tilfelle ser de slik ut: 
```fsharp
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
```

Disse må også registreres før Dapper kan bruke dem. Vi lager en hjelpefunksjon som ordner det:
````fsharp
let addTypeHandlers () =
    SqlMapper.RemoveTypeMap(typeof<Guid>)
    SqlMapper.AddTypeHandler(GuidHandler())
    SqlMapper.AddTypeHandler(MealHandler())
    SqlMapper.AddTypeHandler(MeasurementHandler())
````
Her må vi også fjerne den opprinnelige `Guid` handleren da vi ønsker å bruke vår egen.
Kall denne funksjonen i `Server.fs` filen så er vi sikre på at de er registrert så tidlig som mulig.

Før vi kan starte å kose oss skikkelig med SQL skal vi gjøre en siste ting. Jeg har nemmelig lyst til å lage
en klasse som lager, åpner og stenger database-connections for oss. Denne skal så dependency injectes inn.
Jeg tror det kan være nyttig å se hvor enkelt det er å bruke slike patterns fra F# også.

Her vil vi også lage en klasse. Den tar imot en string, som er connection-stringen vi trenger for å koble til databasen vår.
Vi vil også at denne klassen implementerer `IDisposable`. Klassen trenger å ha en metode som åpner og returnerer en connection, og vi trenger og rydde disse opp i `IDispose`
```fsharp
type DatabaseConnection (connectionString: string) =
    let connection: NpgsqlConnection = new NpgsqlConnection(connectionString)
    member this.getConnection () =
        connection.Open()
        connection
    interface IDisposable with
        member __.Dispose() =
            connection.Close()
            connection.Dispose()
```

Denne kan vi så injecte inn i `configureServices` funkjonen i `Server.fs` fila.

```fsharp
services.AddTransient<Database.DatabaseConnection>(fun _ ->
    new Database.DatabaseConnection(Database.connectionString)) |> ignore
```

Jeg tar inn en connectionstring som miljøvariabel og gjør litt magi på den for at den skal funke med heroku.
Dersom du bruker ikke bruker Heroku trenger du sikkert ikke den magien, men dersom du er nysgjerrig er det bare å se på `database.fs` inne på Github.

Nå som vi har injecta `DatabaseConnection` klassen vår, kan vi nå hente ut overalt hvor vi har tilgang til contexten vår.

## Spørringer

Nå som vi har satt opp systemet er vi klar til å kommunisere med databasen vår.

La oss starte med å hente alle oppskriftene!
Denne er litt spesiell for det vi egentlig vil er å hente ut alt fra oppskriftstabellen og joine den med ingredienstabellen.
Men Dapper konverterer det vi får til typer, så må vi da lage en egen type for hver forskjellige join vi gjør?!
Vi kan det om vi vil, men siden typene våre er så tett knyttet med hvordan ting er i databasen så kan vi hente ut alt og få Dapper til å lage 2 lister for oss.
En liste som har alle oppskriftene og en liste som har alle ingrediensene.
Så må vi bare knytte de sammen. Det er mulig det finnes en bedre måte å gjøre dette på - hvis du vet hvordan så send meg gjerne en melding!
Men akkurat nå løser vi det slik:
```fsharp
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
                    (fun (recipe: Recipe) (ingredient: Ingredient) ->
                        recipes <- recipes@[recipe]
                        if (ingredient :> obj <> null) then
                            ingredients <- ingredients@[ingredient]))
                
            let ingredients =
                ingredients
                |> List.groupBy (fun i -> i.Recipe)
                |> Map.ofList
            let recipes =
                recipes
                |> List.distinct
                |> List.map (fun r -> r, ingredients[r.Id])
                
            return Ok recipes
        with
            | ex -> return Error ex
    }
```
Vi har en enkel query vi ønsker å kjøre.
Vi lager 2 muterbare lister og putter alt vi får i de.
For ingrediensene sjekker vi også om den finnes før vi henter ut verdien.
Så mapper vi de to listene sammen til en liste så vi får typen: `Seq<Recipe * Ingredient list>` - altså en liste med tupler av oppskrift og ingredienser.
Alt dette mappes inn i en `try ... with` blokk. Dersom vi får en exception vet vi at det har skjedd en feil med spørringen eller databasen et sted.
I denne kodesnutten så gjør vi ting som føles litt ekle i et språk som F#. Vi muterer greier, bruker `try...with`, bruker expections. Men disse greien er nyttig og har sitt bruk og her har vi alt dette i en fint funksjonelt API.
Når vi bruker dette får vi en `Task<Result<DATA, exn>` som vi kan deale med på vanlig F# vis.

Når vi skal legge til oppskrifter er den heldigvis litt lettere. Vi følger samme pattern, men har litt flere queries her.
```fsharp
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
```

Her har vi 3 queries og vi bruker derfor en transaksjon.
Første querien legger til selve oppskriften. Vi bruker `RETURNING` til å hente den nye dataen rett ut.
Ingrediensene er en liste, den kan vi få Dapper til å iterere over, men da får vi ikke ut resultatet enkelt, så da må vi hente det ut manuelt.

Oppdatering av oppskrifter følger samme mal som det å legge til, bare litt andre queries.
```fsharp
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
```

Helt til slutt så skal vi kunne slette fra databasen også. Det er nok også den enkleste querien vi skal skrive.
```fsharp
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
```

## Feilhåndtering

La oss førsst starte med å kvitte oss med noen match cases for å gjøre koden mer lettleselig og samtidig gjøre feilhåndteringen vår litt bedre.
Hvis vi har et resultat fra en databasespørring som har denne typen `Task<Result<DbModel, exn>>`
Må vi vanligvis matche på den for å hente ut riktig resultat:

```fsharp
task {
    let! result = SomeDatabaseQuery()
    match result with
    | Ok result -> 
        let foo = gjørNoeMedResultatet(result)
        ...
        ...
    | Error e -> Feilmelding(e)
}
```

For et lite eksempel er jo det rimelig enkelt og rett frem, men hva om vi har flere spørringer? Eller spørringer som avhenger av svar på andre spørringer?
Da kan vi veldig fort få en slags pyramid of doom med matching.
Det finnes biblioteker for å deale med dette, og vi skal bruke et som heter `FsToolkit.ErrorHandling` som har en hel haug med hjelefunksjoner, den vi skal se på heter `taskResult`.
Ser vi på eksempelet over kan det brukes slik

```fsharp
taskResult {
    let! result = SomeDatabaseQuery() |> TaskResult.mapError Feilmelding
    let foo = gjørNoeMedResultatet(result)
    ...
    ...
}
```

Det eneste som gjenstår her er å deale med selve taskresulten, vi må på et eller annet tidspunkt matche på den. 
For å vise det skal vi gå igjennom den nye feilhåndteringen til SLAFS backenden.

I en ny fil `ErrorMessage.fs` lager jeg typer som dealer med de feilmeldingene jeg tror vi kan få

```fsharp
type Message = { Message: string }
type HttpStatus =
    | NotFound of string
    | BadRequest of string
    | InternalError of exn
```

`Message` er den typen vi serialiserer og sender ut fra endepunktet.
`HttpStatus` er hvordan vi representerer feil i systemet vårt. Her kan man jo definere flere typer feil. `Forbidden` er jo nyttig om man har en applikasjon med autentisering.

Vi trenger også noen HttpHandlers som kan lage feilene for oss. Det disse gjør er at de bl.a setter statuskoden for oss.
```fsharp
let notFound message = RequestErrors.NOT_FOUND message
let badRequest message = RequestErrors.BAD_REQUEST message
let internalError exn = ServerErrors.INTERNAL_ERROR exn
```

Så til slutt lager vi en funksjon som dealer med `TaskResult`ene våre.
```fsharp
let httpStatusResult result next context =
    task {
        match! result with
        | Ok result -> return! json result next context
        | Error (BadRequest e) -> return! badRequest { Message = e } next context
        | Error (NotFound e) -> return! notFound { Message = e } next context
        | Error (InternalError e) ->
            printfn "%A" (e.ToString())
            return! internalError { Message = "Det har skjedd en feil i backenden :(" } next context
```
Her tar vi altså resultatet, fra en `TaskResult`, matcher på den og sender det til en `HttpHandler`.
Om man hadde skikkelig logging ville dette være en fin plass å registrere at det skjer feil, kanskje også gi en god feilmelding til brukeren.

## HttpHandlers

La oss bruke databasespørringene våre sammen med `TaskResult` og feilhåndteringskoden vår.

For å hente oppskrifter er det lekende lett:
```fsharp
let getRecipes: HttpHandler =
    fun (next: HttpFunc) (context: HttpContext) ->
        let result =
            taskResult {
                let connection = getDatabaseConnection context
                let! recipes = Database.getAllRecipes connection |> TaskResult.mapError InternalError
                return
                    recipes
                    |> Seq.map Types.encodeRecipeAndIngredient
            }
        httpStatusResult result next context
```

Post blir litt mer komplisert, men mest fordi vi trenger å rulle tilbake transaction dersom noe feiler.

```
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
                return
                    newRecipe
                    |> Types.encodeRecipeAndIngredient
            }
        httpStatusResult result next context
```

Funksjonene for `put` og `delete` følger samme mønster, implementasjonen av dem kan du ordne selv. Alternativt kan du se koden på github.

Det var alt vi hadde for denne gang!
