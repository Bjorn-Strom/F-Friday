# F# Friday 5

Hei og velkommen til den femte posten i en serie om programmeringsspråket F#!

Lenker til tidligere artikler:
- Del 1: [Introduksjon](https://blogg.bekk.no/f-friday-1-39f63618d2e4)
- Del 2: [Typesystemet](https://blogg.bekk.no/f-friday-2-typesystemet-3e7ee0554f0e)
- Del 3: [Backenden](https://blogg.bekk.no/f-friday-3-backend-7463edf0f94a)
- Del 4: [Frontent og React](https://blogg.bekk.no/f-friday-4-frontend-og-react-c356d34a6095)
- **Del 5: JSON-Dekodere**


[Forrige gang](https://blogg.bekk.no/f-friday-4-frontend-og-react-c356d34a6095) lagde vi en enkel frontend i Feliz som kunne snakke med backenden vår. Når vi først lagde backenden vår for en stund siden valgte vi å brukte et dictionary til å holde på alle oppskriftene våre.
Denne skal vi bytte ut, men først må vi ta en runde å snakke om JSON-dekodere!

## Dagens mål
Utviklere elsker å mappe data fra en struktur til en annen. I frontend har vi har kanskje en `viewModel` som blir mappet
over til en `writeModel` før den sendes backend. I backend blir denne mappet om igjen til en `domainModel` som mappes om til en `dbModel`
før den skrives til databasen og så til en `viewModel` igjen før det returneres til frontend. Kanskje har vi `create` og `edit` modeller også.

All denne mappingen er stort sett bortkastet og bidrar kun til komplisert kode. Særlig i en applikasjon som SLAFS hvor det stort sett gjøres CRUD operasjoner.
Vi skal bruke JSON-dekodere og enkodere til å skrive noen enkle funksjoner som erstatter all denne mappingen.
Målet vårt er at serveren vår skal ta imot JSON, dekode og validere den, gjøre litt logikk på dataen, skrive den til en database før vi tilslutt enkoder den til et nyttig JSON format og sender det ut igjen.

## Dekodere? Enkodere?
Du har kanskje hørt om JSON-dekodere fra [Elm](https://package.elm-lang.org/packages/elm/json/latest/Json.Decode), men det finnes også [alternativ](https://github.com/tskj/typescript-json-decoder) som tillater noe lignende i TypeScipt.
Det dekodere tillater oss er å validere at vår JSON har riktig form, riktig innhold, og at den konverteres til riktig data-struktur. 

F# sine dekodere kan man bruke i backend og det er akkurat det vi skal gjøre. Vi skal ta imot en oppskrift som JSON, dekode den om til to databasemodeller som skal lagres.
Når vi til slutt sender denne dataen ut fra backenden igjen bruker vi JSON-enkodere til å produsere den endelige JSONen.

## La oss dekode
I F# bruker vi [thoth-json](https://thoth-org.github.io/) til denne jobben.
La oss si vi har følgende JSON blob å dekode:
```json
{
    "id": "995d1ad3-123f-499a-95f0-4220027cecc0",
    "bank": "Banknavn",
    "betalingsMetode": "kort",
    "adresse": "gate 32"
}
```

Gitt at vi har en tilsvarende F# type, så kan vi lage en dekoder slik:
```fsharp
Decode.object (fun get ->
{
    Id = get.Required.Field "id" Decode.guid
    Bank = get.Required.Field "bank" Decode.string
    BetalingsMetode = get.Required.Field "betalingsMetode" Decode.string
    Adresse = get.Optional.Field "adresse" Decode.string
})
```
Her sier vi at vi ønsker å dekode til et objekt. Vi trenger å si hva feltene heter, om de er påkrevde og hvilken dekoder vi ønsker å bruke på det feltet.
Vi ser også at adressefeltet bruker `get.Optional.Field` som betyr at feltet ikke trenger å være definert og blir til en `option` type.

Vi kan også bruke dekodere til ekstra validering.
La oss si vi har følgende type for betalingsmetode:
```fsharp
type BetalingsMetode =
    | Bank
    | Kontant
```
Vi ønsker at feltet vi dekoder for betalingsmetode ikke skal være en string, men skal bli til en `BetalingsMetode` type.
Da kan vi lage en valideringsfunksjon som ser slik ut:
```fsharp
let validateBetalingsMetode metode =
    match metode with
    | "bank" ->
        Decode.succeed Bank
    | "kontant" ->
        Decode.succeed Kontant
    | _ ->
        Decode.fail $"""Forventet "bank" eller "kontant" ikke {metode}"""
```
Så kan vi bruke den i dekoderen vår:
```fsharp
    ...
    Bank = get.Required.Field "bank" (Decode.string |> Decode.andThen validateBetalingsMetode)
    ...
```

## Oppskrifter står for tur

Nå som vi vet alt om dekoding kan vi ta det i bruk og fikse opp i backenden vår og la oss starte med ingrediensene.
Fra før av så hadde vi alle typene våre i den delte filen som både frontend og backend brukte. Disse modellene kommer ikke backend lenger til å bruke.
Derfor har jeg opprettet en egen `Types.fs` fil i frontend og flyttet disse typene inn dit. 
Vi trenger også noen backend typer, så jeg har laget en ny fil `Types.fs` som lever i backenden og der definerer jeg en `IngredientDbModel`.

```fsharp
type IngredientDbModel =
    { Volume: float
      Measurement: Measurement
      Name: string
      RecipeId: System.Guid }
```

Når jeg ser denne modellen er det 2 felter jeg har lyst til å validere under dekodingen.
Det er feltene `Measurement` og `Name`.
`Measurement` skal bli gjort om fra en string til en tilsvarende `Measurement` type og `Name` skal ha noen lengde-begrensninger.
La oss først skrive funksjonen `validateMeasurement`, den blir veldig lik eksempelet over:
```fsharp
let validateMeasurement (measurement: string) =
    match stringToMeasurement measurement with
    | Some measurement -> Decode.succeed measurement
    | None -> Decode.fail $"{measurement} er ikke en gyldig målenhet."
```
Her bruker vi en funksjon vi allerede har definert `stringToMeasurement`, bare at jeg har endret den til å returnere en option.

Vi lager også en funksjon `validateStringLength` som validerer riktig lengde på en string.
Vi sier at strings i våre objekter må ha minst 1 karakter og har en maks lengde.
En slik funksjon kan se slik ut:
```fsharp
let validateStringLength (name: string) maxLength (fieldToValidate: string)  =
    if fieldToValidate.Length = 0 then
        Decode.fail $"\"{name}\" må være lenger enn 0 tegn"
    else if fieldToValidate.Length > maxLength then
        Decode.fail $"{name} kan ikke være lenger enn {maxLength} tegn"
    else
        Decode.succeed fieldToValidate
```

Med dette på plass kan vi lage ingredient dekoderen vår:
```fsharp
let ingredientDecoder: Decoder<IngredientDbModel> =
    Decode.object (fun get ->
    {
        Volume = get.Required.Field "volume" Decode.float
        Measurement = get.Required.Field "measurement"
                          (Decode.string
                           |> Decode.andThen validateMeasurement)
        Name = get.Required.Field "name"
                   (Decode.string
                    |> Decode.andThen(validateStringLength "name" 32))
        RecipeId = get.Required.Field "recipeId" Decode.guid
    })
```
I samme slengen kan vi lage en liten dekoder som dekoder en liste med ingredienser.
```fsharp
let ingredientListDecoder: Decoder<IngredientDbModel list> =
    Decode.object (fun get -> get.Required.Field "ingredients" (Decode.list ingredientDecoder))
```

Oppskriftsmodellen vår ser slik ut:
```fsharp
type RecipeDbModel =
    { Id: System.Guid
      Title: string
      Description: string
      Meal: Meal
      Time: float
      Steps: string array
      Portions: int }
```

Her ønsker vi å validere `Meal` og noen av stringene våre.
I tillegg så vil vi ikke at en oppskrift uten steg skal være gyldig, og den må ha minst 1 porsjon.

Valideringsfunksjonene kan se slik ut:
```fsharp
let validateStep steps =
    if Array.isEmpty steps then
        Decode.fail "Oppskrifter trenger minst ett steg"
    else
        Decode.succeed steps
        
let validatePortion portion =
    if portion = 0 then
        Decode.fail "Oppskrifter må ha minst 1 porsjon"
    else
        Decode.succeed portion
        
let validateMeal (meal: string) =
    match stringToMeal meal with
    | Some meal -> Decode.succeed meal
    | None -> Decode.fail $"{meal} er ikke et gyldig måltid."
```

Oppskrift dekoder:
```fsharp
let recipeDecoder: Decoder<RecipeDbModel> =
    Decode.object (fun get ->
        {
            Id = get.Required.Field "id" Decode.guid
            Title = get.Required.Field "title"
                        (Decode.string
                         |> Decode.andThen (validateStringLength "title" 64))
            Description = get.Required.Field "description" Decode.string
            Meal = get.Required.Field "meal"
                       (Decode.string
                        |> Decode.andThen validateMeal)
            Time = get.Required.Field "time" Decode.float
            Steps = get.Required.Field "steps"
                        (Decode.array Decode.string
                         |> Decode.andThen validateStep)
            Portions = get.Required.Field "portions"
                           (Decode.int
                            |> Decode.andThen validatePortion)
        })
```

Det vi også trenger for begge modellene våre er enkodere.
For ingredienser vil de se slik ut:
```fsharp
let encoder ingredient = 
   Encode.object [
        "volume", Encode.float ingredient.Volume
        "measurement", Encode.string (ingredient.Measurement.ToString())
        "name", Encode.string ingredient.Name
        "recipeId", Encode.guid ingredient.RecipeId
    ]
```

For oppskrifter bruker vi den forrige enkoderen vår så vi får ingredients inn i oppskriften.
Her genererer vi en JSON-versjon av den samme domenemodellen vi hadde før, den som frontend forventer.
For mer typesikkerhet kan vi bruke den her for å forsikre oss om at den blir riktig.
```fsharp
let encodeRecipeAndIngredient (recipe, ingredients) =
    Encode.object [
        "id", Encode.guid recipe.Id
        "title", Encode.string recipe.Title
        "description", Encode.string recipe.Description
        "meal", Encode.string (recipe.Meal.ToString())
        "time", Encode.float recipe.Time
        "steps", recipe.Steps |> Array.map Encode.string |> Encode.array
        "portions", Encode.int recipe.Portions
        "ingredients",
            ingredients
            |> List.map IngredientDbModel.encoder
            |> Encode.list
    ]
```

Ettersom vi validerer all vår JSON i det vi mottar den kan vi anta at data-strukturene våre er gode så lenge valideringen ikke feiler.
Derfor lagrer vi den bare rett inn i databasen.

## Giraffe
Nå som vi har dekodere og enkodere trenger vi å bruke dem.
Det kan vi gjøre når vi mottar/sender data. I vår backend gjør vi kun det i `HttpHandlers` fila vår.

Først tenker jeg å lage to hjelpefunksjoner.
Den første gjør det litt enklere å si at vi har mottatt en dårlig spørring:
```fsharp
let private badRequest errorMessage next (context: HttpContext) =
    let errorMessage = {| Error = errorMessage |}
    context.SetStatusCode(400)
    json errorMessage next context
```

Denne tar imot feilmeldingen som skal returneres, setter retur-verdier og svarer på spørringen.

Den andre hjelpefunksjonen gjør selve dekodingen av både ingredienser og oppskrifter og returnerer en resultatstype.
Dersom dekoding av både oppskrift og ingredienser går fint så returnerer vi de decodede verdiene.
Dersom det er feil i en eller begge av de så returnerer vi en feilmelding.

```fsharp
let decodeRecipeAndIngredientHelper (context: HttpContext) =
    task {
        let! body = context.ReadBodyFromRequestAsync()
        let decodeRecipe = Decode.fromString recipeDecoder body
        let decodeIngredient = Decode.fromString ingredientListDecoder body
        
        match decodeRecipe, decodeIngredient with
        | Ok recipe, Ok ingredients ->
            return Ok (recipe, ingredients)
        |  Error e1, Error e2 ->
            return Error $"Feil under decoding av oppskrifter {e1} og ingredienser {e2}."
        | Error e, _ ->
            return Error $"Feil under decoding av oppskrifter {e}."
        |  _, Error e ->
            return Error $"Feil under decoding av ingredienser {e}."
    }
```

La oss starte i `getRecipes` - funksjonen som henter alle oppskriftene.
Her trenger vi ingen dekoding, men vi skal encode. Den kan se slik ut:
```fsharp
let getRecipes: HttpHandler =
    fun (next: HttpFunc) (context: HttpContext) ->
        let encodedRecipes =
            Database.getAllRecipes ()
            |> List.map Types.encodeRecipeAndIngredient
            
        json encodedRecipes next context
```

For `postRecipe` trenger vi faktisk dekoding også.
Da kan vi bruke de nye hjelpefunksjonene våre.
Dersom dekodingen går fint så kan vi lagre oppskriften og ingrediensene i databasen.
Hvis ikke så kan vi bruke `badRequest` til å si ifra til brukeren vår at noe er feil.
```fsharp
let postRecipe: HttpHandler =
    fun (next: HttpFunc) (context: HttpContext) ->
        task {
            let! decodedRecipeAndIngredients = decodeRecipeAndIngredientHelper context
            match decodedRecipeAndIngredients with
            | Ok (recipe, ingredients) ->
                let newRecipes =
                    Database.addRecipe recipe ingredients
                    |> List.map Types.encodeRecipeAndIngredient
                return! json newRecipes next context
            | Error e ->
                return! badRequest e next context
        }
```

`putRecipe` følger samme mønster som funksjonen over, så det kan være en øvelse til leseren.

## Fin

Nå har vi en ganske robust backend, det eneste vi mangler er faktisk persistens.
Som vanlig kan du finne hele prosjektet på [github](https://github.com/Bjorn-Strom/F-Friday/tree/5-decoders)
Så bra at neste oppgave blir å lagre alt dette i en database. Det blir gøy!
Da skal vi se på Dapper og Postgresql.

Vi sees da!

