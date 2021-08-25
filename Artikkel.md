# F# Friday 4

Hei og velkommen til den fjerde posten i en serie om programmeringsspråket F#!

[Forrige gang]() laget vi en backend som kunne serve oppskriftene våre. Med den kan vi hente alle, lage nye, oppdatere eller slette oppskrifter. Denne gangen skal vi lage en enkel frontend som kan kommunisere med denne backenden og vise frem de fine oppskriftene våre.

Istedenfor å gå gjennom oppsettet til en server, client og hvordan man deler kode mellom dem så kan man bruke en template.
Det finnes ganske mange av disse faktisk: [SAFE Stack](https://safe-stack.github.io), [SAFEr.Template](https://github.com/Dzoukr/SAFEr.Template), [SAFE.Simplified](https://github.com/Zaid-Ajaj/SAFE.Simplified) med flere, men SAFE Stack er den jeg har brukt mest.

Siden jeg vil holde oppsettet i denne artikkelserien så enkelt som mulig har jeg lagd en minimalistisk stack på egen hånd.

## Dagens plan
Målet med denne artikkelen er å vise frem hvor enkelt, morsomt og bra det er å lage frontend-applikasjoner med F#.
Denne appen er ikke produksjonsklar og har noen problemer, men jeg håper det er mulig å fokusere på
hvordan det er å skrive React i F# heller enn noen av feilene med appen. :pray:

Verktøyet som tillater oss å gjøre dette er [Fable](https://fable.io). Fable tillater oss å transpilere F# koden vår over til JavaScript.
Siden vi ønsker å bruke F# som en TypeScript erstatning skal vi skrive React med hjelp av [Fable](https://fable.io).
Vi kommer til å ha typesikker styling ved hjelp av mitt bibliotek [Fss](https://github.com/Bjorn-Strom/FSS).
Og tilslutt se hvor enkelt det er å dele kode mellom frontend og backend.

Frontend er et veeeldig stort tema og er umulig å dekke i en artikkel. Se på denne som en introduksjon til Frontend i F#, så kan det komme flere temaer i fremtiden.
En annen ting som er umulig her er å gå gjennom hver kodelinje. Så jeg kommer til å forklare hvordan ting funker, så gå over koden i grove trekk. Så kan du alltids se hele koden inne på github!

## Fable
[Fable](https://fable.io) er en måte å transpilere vanlig F# til JavaScript og er et utrolig bra verktøy. Ikke bare tillater det oss å skrive *single page applications* i F#, men vi kan i praksis kjøre F# alle plasser der JavaScript kan kjøres.
Så om vil lage nettsider, node eller elektron applikasjoner så står du fritt til å gjøre det.

I tillegg til dette kan Fable også transpilere over til Python og PHP.

## How to React?
Som nevnt er [Fable](https://fable.io) et bibliotek som lar oss skrive React i F#. Så ved å bruke dette kan du skrive React komponenter som vanlig,
du kan bruke hooks, context, portals, alt som du er kjent med fra "vanlig" React - bare i en immutabel og typesikker måte.

Med Feliz kan du importere JSX om du vil, men biblioteket tilbyr et eget DSL som er mye mer egnet til F# og som fungerer veldig bra.
Et hello world eksempel:
```fsharp
[<ReactComponent>]
let Hello() =
    let (name, setName) = React.useState("World!")
    Html.div [
        Html.input [
            prop.value name
            prop.onChange (fun e -> setName(e.value))
        ]

        Html.p $"Hello {name}"
    ]
```

Dette ser sånn ut:

Systemet er nokså enkelt: `Html.` etterfulgt av hvilket Html element du ønsker deg. Så en liste med de propertiene du vil at elementet ditt skal ha.
Aldri igjen trenger du å rote med innholdet i start og slutt tags!

## Suit up!
Stilig, men hva med styling?
For å være litt shameless kommer jeg til å bruke [mitt eget](https://github.com/Bjorn-Strom/FSS) styling bibliotek, men du kan også bruke inline styles, SASS, LESS, type providers eller gode gamle CSS om du ønsker det.
Fordelen med Fss er typesikkerhet, programmet ditt kompilerer ikke om du har skrevet stylingen din feil (at det ser bra ut er desverre ingen garanti).

Fss fungerer uavhengig av Feliz, men har også en Feliz plugin for å kunne skrive stylingen direkte i komponenter. Så la oss style eksempelet over litt.
```fsharp
[<ReactComponent>]
let Hello() =
    let (name, setName) = React.useState("World!")
    Html.div [
        Html.input [
            prop.value name
            prop.onChange (fun e -> setName(e.value))
            prop.fss [
                BackgroundColor.gainsboro
                Width' (px 150)
                Height' (px 20)
                FontSize' (px 17)
            ]
        ]

        Html.p [
            prop.text $"Hello {name}"
            prop.fss [
                FontSize' (px 17)
                if name = "world" then
                    Color.green
                else
                    Color.blue
                Hover [
                    FontWeight.bold
                ]
            ]
        ]
    ]
```

Ting å merke seg her:
- Vi styler conditionally basert på en if setning rett i listen.
- Flere ting?

## Tilbake til oppskrifter
Nå som vi har litt bakgrunn for hva vi skal gjøre er det igjen oppskriftene som står for tur.
La oss planlegge litt hvordan denne siden skal se ut og hvordan den skal fungere.

Vi delte oppskriftene inn i gruppene *frokost*, *lunsj*, *middag* og *dessert*. Så hver av disse burde ha sin egen side hvor man kan se alle oppskriftene innenfor en enkelt gruppe.
Fra den siden kan man klikke seg inn på en enkelt oppskrift for å se ingrediensene og steg.
Vi trenger også en side for å legge til nye oppskrifter.
Til slutt burde vi også legge inn en velkomstside, så brukerne våre føler seg velkommen.

Teknisk tenker jeg noe slik:
- På page load lastes alle oppskrifter og vi lagrer dem i en context.
- Noe annet vi lagrer i contexten er hvilken side vi ser på.
- Basert på hvilken side vi ser på endrer vi hva som rendres. Her ville vi nok brukt routing, men det skipper vi i dag.
- Vi har en egen side for å lage nye oppskrifter og en knapp som sender oppskriften vår til backend. Da endrer vi side til den nye oppskriften.

## Typer
Som alltid starter vi med å lage noen typer. Vi vet allerede at vi trenger typer for *viewet* vårt.
De kan se slik ut:
```fsharp
type View =
    | Home
    | RecipeDetails of Recipe
    | Breakfasts
    | Lunches
    | Dinners
    | Desserts
    | NewRecipe
```
Da kan vi være på en av disse sidene. Vi ser også at `RecipeDetails` har en oppskrift knyttet til seg.
En annen ting vi trenger som vil gjøre det lettvint å holde på med ekstern data er en `RemoteData` type.
Denne typen sier noe om statusen på dataen vi har i applikasjonen vår. Denne dataen kan holde på å bli lastet, være hentet eller det har kunne forekommet en feil.

Vi kan definere denne slik:
```fsharp
type RemoteData<'t> =
    | Fetching
    | Data of 't
    | Failure of string
```
Her tar vi inn en type t som er den typen dataen vår kommer til å være, og dersom det har skjedd en feil får vi denne dataen som en string.


## Lagring av data
Før vi går videre kan vi også sette opp *storen* vår.

Vi ønsker å holde styr på 2 ting:
1. Oppskriftene våre
2: Hvilket view vi er inne på nå

Vi trenger også en måte å endre disse på.
Så vi trenger *actions*, en *reducer* og en initial *store*.

```fsharp
type Store =
    { Recipes: Recipe list RemoteData
      View: View }
```
Datatypen til storen vår. Det er her vi kommer til å lagre all data i appen. Denne bruker oppskrifter vi definerte for lenge siden, men også `RemoteData` og `View` som vi definerte i stad.

```fsharp
type StoreAction =
    | SetRecipes of Recipe list RemoteData
    | AddRecipe of Recipe
    | SetCurrentView of View
```
Vår store kommer til å ha 3 actions:
1. `SetRecipes` som setter oppskrift staten vår. Denne tar inn en liste med `Recipes` inne i en `RemoteData` og lagrer den.
2. `AddRecipe` tar inn en oppskrift og lagrer den sammen med de andre oppskriftene våre.
3. `SetCurrentView` som endrer viewet vi ser på.

For å deale med actions trenger vi reduceren.
Det kommer til å være en funksjon som matcher på de actionene vi har definert
```fsharp
let StoreReducer state action =
    match action with
    | AddRecipe recipe ->
        let newRecipes =
            match state.Recipes with
            | Data recipes -> Data (recipe :: recipes)
            | _ -> Data [recipe]
        { state with Recipes = newRecipes }
    | SetRecipes recipes -> { state with Recipes = recipes }
    | SetCurrentView view -> { state with View = view }
```

Før vi setter opp selve contexten trenger vi vår inital store:
```fsharp
let initialStore =
    { Recipes = Fetching
      View = Home }
```
Siden det første vi ønsker å gjøre er å hente inn data kan vi sette oppskriftene til fetching med en gang.
Vi ønsker at brukeren blir vist forsiden til å starte med og den kalte vi `Home`.

Det å lage contexter er veldig enkelt, vi bruker funksjonen `createContext`:
```fsharp
let storeContext = React.createContext()
```

Vi trenger også en provider vi kan koble sammen med `app` komponenten vår så hele appen vår får tilgang til storen.
```fsharp
[<ReactComponent>]
let StoreProvider children =
    let (state, dispatch) = React.useReducer(StoreReducer, initialStore)
    React.contextProvider(storeContext, (state, dispatch), React.fragment [children])
```
Denne komponenten tar inn et barn, som for oss kommer til å være hele appen, og wrapper den med en provider.

Det aller siste vi trenger er en hook så vi får hente ut storen og en funksjon så vi kan dispatche til den.
```fsharp
Hooks lager vi med å bruke `[<Hook>]` attributten:
[<Hook>]
let useStore() =
    React.useContext(storeContext)
```

Veldig mye greier...
Men nå har vi en store vi kan bruke slik:
```fsharp
let (store, context) = useStore()
```

La oss koble dette sammen i app så vi kan starte med å skrive litt React!

## Slafs!
La oss starte å implementere `Slafs!`, **den** nye store nettsiden for matoppskrifter!

Okei, nå vet vi hvordan vi lager React-komponenter og vi vet hvordan vi bruker staten vår.
La oss lage en komponent som bruker `useEffect` til å hente alle oppskriftene våre, lagre den i staten og basert på om dataen kommer inn eller ikke viser enten at det laster, velkomstsiden, eller en feilmelding.

```fsharp
let Container() =
    // Hent ut state og dispatch
    let (state, dispatch) = useStore()

    // Lag useEffect hooken vår
    React.useEffect((fun () ->
        // Bruker en F# implementasjon av Fetch APIet
        fetch "http://localhost:5000/api/recipes" []
        // Sender den inn i bind så vi får hentet ut response-stringen består av
        |> Promise.bind (fun result -> result.text())
        // Sender den stringen videre til en decoder som forsøker å gjøre den om til en liste med oppskrifter
        |> Promise.map (fun result -> Decode.Auto.fromString<Recipe list>(result, caseStrategy=CamelCase))
        // Dersom decodingen fungerte så vil vi gjøre den om til en RemoteData
        |> Promise.map (fun result ->
            match result with
            | Ok recipes -> Data recipes
            | Error e -> Failure e)
        // Uansett hvilken type RemoteData vi får tilbake så vil vi lagre den i state
        |> Promise.map (fun r -> dispatch (SetRecipes r))
        // Så må vi starte promiset
        |> Promise.start)
        , [| |])
```

Der har vi halve komponenten. Denne delen kjører et nettverkskall mot
localhost og lagrer resultatet i en `RemoteData`.
Det neste vi trenger er å rendre basert på hva denne dataen er.
Vi vet at vi kan være i:
- `Fetching` modus som betyr at vi henter data.
- `Data` og da har vi data lagret i state
- `Failure` det skjedde en feil under henting av oppskriftene våre.

Siden vi modellerte dette med en discriminated union kan vi nå matche på alle disse 3 tilfellene.
Vi legger til dette under promiset vårt:
```fsharp
    match state.Recipes with
    | Fetching -> Html.div [ prop.text "Laster..." ]
    | Data _ -> PageView()
    | Failure e -> Html.div [
        prop.fss [ textFont ]
        prop.text $"En feil skjedde under henting av oppskrifter: {e}"
    ]
```

Her viser vi, en fattig manns spinner, teksten "Laster..." om vi venter på data.
Dersom vi har data viser vi `PageView` komponenten. Den trenger ingen props og vi forkaster
dataen vi mottok med `_`. Vi trenger ikke å prop drille denne her når den ligger i state.
Til slutt dersom vi mottar en feil lagrer vi den i `e` og viser den.
Enkelt og greit, ikke sant?

## Why tho?
- Immutabilitet uten å bruke tredjeparts biblioteker
- Slipper å tenke like mye på JS, der i de områdene TS feiler
- Slipper implicit any
- Veldig sikkert refaktorering

Mye forbedringspotensial her.
- Nettverkskall kan fort bli gjort litt smoothere.
- Routing
- Hadde ikke skadet å gått over stylingen en tur, it's not excactly pretty.