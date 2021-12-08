# F# Friday 5

Hei og velkommen til den femte posten i en serie om programmeringsspråket F#!

Lenker til tidligere artikler:
- Del 1: [Introduksjon](https://blogg.bekk.no/f-friday-1-39f63618d2e4)
- Del 2: [Typesystemet](https://blogg.bekk.no/f-friday-2-typesystemet-3e7ee0554f0e)
- Del 3: [Backenden](https://blogg.bekk.no/f-friday-3-backend-7463edf0f94a)
- Del 4: [Frontent og React](https://blogg.bekk.no/f-friday-4-frontend-og-react-c356d34a6095)
- **Del 5: Databaser**

[Forrige gang](https://blogg.bekk.no/f-friday-4-frontend-og-react-c356d34a6095) laget vi en enkelt frontend i react som kunne snakke med backenden vår. Når vi først lagde backenden vår brukte vi et dictionary til å holde på alle oppskriftene våre. Denne gangen skal vi bytte ut denne in-memory databasen med PostgreSQL.

## Dagens plan
Vi skal starte julebordet med å se på noen av de mange databasemuligheter som finnes til F# før vi bruker `Dapper` til å implementere funksjonaliteten vi trenger for å oppdatere oppskriftene våre.

## Verktøy
I C# bruker man, i min erfaring, Entity Framework (EF) og selv om det funker fint er det flott å ha noen alternativer.
F# har en del spennende alternativer og under er en liten liste:
- [SqlProvider](https://github.com/fsprojects/SQLProvider) - Genererer typer fra databasen din mens du programmerer. Det betyr at koden din ikke vil kompilere dersom du har skrevet feil SQL. Behøver en aktiv tilgang til din database for å bygge koden din, som kan være litt irriterende.
- [RepoDb](https://github.com/mikependon/RepoDB) - Gir et EF lignende API.
- [Dapper.FSharp](https://github.com/Dzoukr/Dapper.FSharp) - En lettvekts wrapper på Dapper som gjør det enkelt å bruke Dapper med Giraffe
Og det finnes mange flere også så om man er interessert er det bare å google litt så kommer det masse forslag.

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

Så kan vi inserte dem i databasen slik:
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

## Gjør om alt til tasks

## Hjelpefunksjoner
Det vi egentlig trenger å gjøre er å opprette en databasekobling til en database hvor vi har opprettet tabeller for oppskrifter og ingredienser.
For å gjøre det så enkelt som mulig er det opprettet 2 tabeller, 1 med oppskrifter og 1 med ingredienser. Hver ingrediens har en fremmednøkkel til oppskrifter, så vi vet hvilke ingredienser en oppskrift faktisk består av.

Det vi også trenger er noen databasemodeller. Disse vil ikke være lik Domene-modellene da de ikke er helt like.

Oppskriftstypen vår ser slik ut:
```fsharp
[<CLIMutable>]
type RecipeDbModel =
    { Id: string
      Title: string
      Description: string
      Meal: string
      Time: float
      Steps: string array
      Portions: int }
``

Og ingrediensene slik:
```fsharp
[<CLIMutable>]
type IngredientDbModel =
    { Volume: float
      Measurement: string
      Name: string
      Recipe: string }
```

Vi trenger også å koble oss til databasen og lage en referanse til tabellene i koden vår.
```fsharp
let connection = new NpgsqlConnection(connectionString)
let recipeTable = table'<RecipeDbModel> "Recipe"
let ingredientTable = table'<IngredientDbModel> "Ingredient"
```

Noe vi også trenger er funksjoner for å konvertere domenemodellene våre til databasemodeller og tilbake.
```fsharp
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
```

Dette er noen linjer med kode, men er veldig rett frem. Det er funksjoner som tar inn en type, databasemodell f.eks og gjør den om til en DB-modell.

Vi kan også lage en liten hjelpefunksjon som tar inn en oppskrift of spytter ut databasemodeller til både oppskrifter og ingredienser.
```fsharp
let private recipeToDbModels recipe =
    let recipeToInsert = recipeToDb recipe

    let ingredientsToInsert =
        recipe.Ingredients
        |> List.map (fun i -> ingredientToDb i recipeToInsert.Id)

    recipeToInsert, ingredientsToInsert
```

## Slafs: Database time
Vi kan starte med å hente ut alle oppskriftene våre.
For å gjøre det så endrer vi `getAllRecipes` funksjonen.
Det vi trenger å gjøre der er å hente alle oppskriftene, så hente alle ingrediensene.
Så må vi mappe over alle oppskriftene og knytte dem sammen med ingrediensene som tilhører dem.

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
