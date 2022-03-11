# F# Friday 5

Hei og velkommen til den femte posten i en serie om programmeringsspråket F#!

Lenker til tidligere artikler:
- Del 1: [Introduksjon](https://blogg.bekk.no/f-friday-1-39f63618d2e4)
- Del 2: [Typesystemet](https://blogg.bekk.no/f-friday-2-typesystemet-3e7ee0554f0e)
- Del 3: [Backenden](https://blogg.bekk.no/f-friday-3-backend-7463edf0f94a)
- Del 4: [Frontent og React](https://blogg.bekk.no/f-friday-4-frontend-og-react-c356d34a6095)
- **Del 5: Databaser**

[Forrige gang](https://blogg.bekk.no/f-friday-4-frontend-og-react-c356d34a6095) lagde vi en enkelt frontend i Feliz som kunne snakke med backenden vår. Når vi først lagde backenden vår for en stund siden valgte vi å brukte vi et dictionary til å holde på alle oppskriftene våre. Denne gangen skal vi bytte ut denne in-memory databasen med PostgreSQL.

## Verktøy
F# har ganske mange forskjellige verktøy man kan bruke for å jobbe med databaser. De som kommer under er de jeg selv har erfaring med, men du kan godt google litt om du vil ha enda flere alternativer.

- [SqlProvider](https://github.com/fsprojects/SQLProvider) - Genererer typer fra databasen din mens du programmerer. Det betyr at koden din ikke vil kompilere dersom du har skrevet feil SQL. Behøver en aktiv tilgang til din database for å bygge koden din, som kan være litt irriterende.
- [Entity Framework]()(EF) - Selv om EF er mest kjent som et C# bibliotek fungerer det i F# også, men det er ikke like ergonomisk å jobbe med som noen av alternativene.
- [RepoDb](https://github.com/mikependon/RepoDB) - Gir et enklere EF lignende API.
- [Dapper.FSharp](https://github.com/Dzoukr/Dapper.FSharp) - En lettvekts wrapper på Dapper som gjør det enkelt å bruke Dapper med Giraffe.

Vi kommer til å bruke Dapper i Slafs da det er veldig enkelt å komme igang med og passer veldig bra sammen med Giraffe.

## Dapper
[Dapper](https://github.com/DapperLib/Dapper) er, som de selv sier, en "simple object mapper for .Net". Det er en lettvekts måte å kommunisere med databaser på.
Det er et nyttig lite verktøy og heldigvis er det laget en wrapper på Dapper til F# som vi skal bruke. Denne wrapperen bruker [computation expressions](https://fsharpforfunandprofit.com/series/computation-expressions/)
til på lage et lite SQL DSL som returnerer tasks. Hvis vi tenker tilbake på backend episoden i denne artikkelserien så husker vi at Giraffe, som vi bruker til å håndtere routing, også bruker tasks.
Om litt skal vi se på hvor enkelt det er å få disse til å jobbe sammen.

La oss si vi har en addressebok som har en tabell og databasemodell som ser slik ut:
```fsharp
type AddressDbModel {
    Name: string
    Address: string
}
```

Så kan vi bruke dapper til å skrive en insert query slik:
```fsharp
// Først må vi definere tabellene våre
let AddressTable = Table'<AddressDbModel> "Addresses"
insert {
    into AddressTable
    value someNewAddress
} |> dbConnection.InsertAsync
```
Vi kan lese ting ut like enkelt
```fsharp
select {
    for a in AddressTable do
    selectAll
} |> dbConnection.SelectAsync<AddressDbModel>
```


## Forbered sjiraffen

Alle disse blokkene returnerer tasks, så la oss skrive om Giraffe koden vår til å bruke tasks også. Da blir det enkelt å få disse to biblioteken til å snakke sammen. Fra forrige gang mangler `getRecipes` og `deleteRecipe` tasks. Det legger vi enkelt til og HttpHandlerene våre ser nå slik ut:
```fsharp
let getRecipes: HttpHandler =
    fun (next: HttpFunc) (context: HttpContext) ->
        task {
            let! recipes = Database.getAllRecipes ()
          return! json recipes next context
        }
let deleteRecipe (id: System.Guid) : HttpHandler =
    fun (next: HttpFunc) (context: HttpContext) ->
        task {
            do! Database.deleteRecipe id
            return! text $"Deleted recipe with id: {id}" next context
        }
```

## Modeller og funksjoner
Det vi trener gå gjøre er å opprette en databasekobling til en databasen vår.
Denne datgabase nhar allerede fått opprettett tabeller for oppskrifter og ingredienser.
For å gjøre det så enkelt som mulig er det opprettet 2 tabeller, 1 med oppskrifter og 1 med ingredienser. 

Det aller første vi trenger er noen databasemodeller. Altså typer som sier hvordan dataen vår faktisk ser ut i databasen. Denne typen må matche den tabellen den tilhører.
Vi kan ikke bruke domene-modellene våre til dette da disse modellene er forskjellige.
Det vi også kommer til å trenge er funksjoner som konverterer mellom de forskjellige modell-typene.

Vi starter med databasemodellene våre:
For oppskrift
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

Nå som modellene og hjelpefunksjonene våre er klare kan vi koble oss til databasen.
Vi lager tabellene våre i samme slengen så alt er klart til å brukes
```fsharp
let connection = new NpgsqlConnection(connectionString)
let recipeTable = table'<RecipeDbModel> "Recipe"
let ingredientTable = table'<IngredientDbModel> "Ingredient"
```

## Slafs: Database time
La oss starte med å hente ut alle oppskriftene våre. Slett hele fakbase, in-memory databasen, vi hadde fra før.
Vi lager en ny funksjon som også heter `getAllRecipes` funksjonen.
Det vi trenger å gjøre der er å hente alle oppskriftene og alle ingrediensene.
Så bruker vi `Recipe` stringen og mapper over alle oppskriftene og ingrediensene og lager domene modeller ut av dem.

```fsharp
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
```

Neste funksjon blir å legge til nye oppskrifter.
Her tar vi inn en oppskrift og kaller `recipeToDbModels` funksjonen vår.
Da får vi alle oppskriftene og ingrediensene våre. De inserter vi rett inn i databasen.
`addRecipe` blir da:
```fsharp
let addRecipe newRecipe =
    task {
        let recipeToInsert, ingredientsToInsert = recipeToDbModels newRecipe

        // Legg inn alle oppskriftene
        let! insertedRecipe =
            insert {
                into recipeTable
                value recipeToInsert
            }
            |> connection.InsertAsync

        printfn $"Inserted {insertedRecipe} recipe(s)"

        // Legg inn alle ingrediensene
        let! insertedIngredients =
            insert {
                into ingredientTable
                values ingredientsToInsert
            }
            |> connection.InsertAsync

        printfn $"Inserted {insertedIngredients} ingredient(s)"
    }
```

Når det kommer til å oppdatere oppskrifter så starter vi med å oppdatere selve oppskriften,
Deretter sletter vi alle ingrediense og legger de til på nytt.
Shazam! Oppdatert oppskrift.
```fsharp
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
```

Avslutningsvis trenger vi bare å slette basert oppskrifter og ingredienser basert på de IDene vi har.
Ez pz
```fsharp
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
```

Litt kjedelig å skrive "enkle" CRUD funksjoner.
Men det er også veldig enkelt å skrive disse i F# og det er enkelt å knytte dem sammen med Giraffe på den måten.

## Fin

Og med det er vi ferdig med SLAFS.
Det var en digg reise.

Og vi har lært en masse greier!
Vi kan masse om F# syntax og om typesystemet.
Deretter satte vi opp en enkelt backend med Giraffe og en enkel in memory database så vi kunne kjøre noen enkle requests.
Så skrev vi en enkel frontend med F# og React hvor vi hentet informasjon fra backenden vår.
Og tilslutt byttet vi ut in memory databasen med en enkel PostgreSQL database hvor vi bruke Dapper og knyttet den sammen med Giraffe.

Nå som vi har brukt F# litt og vet hva det kan brukes til skal vi fortsette å se på spennende temaer fremover.
Neste gang skal vi se på hvordan man kan starte å bruke F# i et vanlig JavaScript prosjekt.
Det blir bra!

