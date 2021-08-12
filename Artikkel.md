# F# Friday 4

Hei og velkommen til den fjerde posten i en serie om programmeringsspråket F#!

[Forrige gang]() laget vi en backend som kunne serve oppskriftene våre. Med den kan vi hente alle, lage nye, oppdatere eller slette oppskrifter. Denne gangen skal vi lage en enkel frontend som kan kommunisere med denne backenden og vise frem de fine oppskriftene våre.

Istedenfor å gå gjennom oppsettet til en server, client og hvordan man deler kode mellom dem så kan man bruke en template.
Det finnes ganske mange av disse faktisk: [SAFE Stack](https://safe-stack.github.io), [SAFEr.Template](https://github.com/Dzoukr/SAFEr.Template), [SAFE.Simplified](https://github.com/Zaid-Ajaj/SAFE.Simplified) med flere, men SAFE Stack er den jeg har brukt mest.

Siden jeg vil holde oppsettet i denne artikkelserien så enkelt som mulig har jeg lagd en minimalistisk stack på egen hånd.

## Dagens plan
Målet med denne artikkelen er å vise frem hvor enkelt, morsomt og bra det er å lage frontend applikasjoner med F#.

For å oppnå dette kommer vi til å se på [Fable](https://fable.io), et verktøy som tillater oss å transpilere F# koden vår over til JavaScript.
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

Vi trenger også en måte å endre disse 2 på, så da trenger vi 2 actions.
Da må vi også ha en *reducer* som håndterer disse og en initial *store*.

```fsharp
type Store =
    { Recipes: Recipe list RemoteData
      View: View }
```
Her lagrer vi dataen vår, vi ser at det er datatyper vi allerede har definert.

```fsharp
type StoreAction =
    | SetRecipes of Recipe list RemoteData
    | SetCurrentView of View
```
Actions for å endre disse, de kommer til å ta de samme typene som *parameter*.

For å deale med actions trenger vi reduceren.
Det kommer til å være en funksjon som matcher på de actionene vi har definert
```fsharp
let StoreReducer state action =
    match action with
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

## Why tho?
- Immutabilitet uten å bruke tredjeparts biblioteker
- Slipper å tenke like mye på JS, der i de områdene TS feiler
- Slipper implicit any