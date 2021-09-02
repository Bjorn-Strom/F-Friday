# F# Friday 4

Hei og velkommen til den fjerde posten i en serie om programmeringsspråket F#!

[Forrige gang]() laget vi en backend som kunne serve oppskriftene våre. Med den kan vi hente alle, lage nye, oppdatere eller slette oppskrifter. Denne gangen skal vi lage en enkel frontend som kan kommunisere med denne backenden og vise frem de fine oppskriftene våre.

Istedenfor å gå gjennom oppsettet til en server, client og hvordan man deler kode mellom dem så kan man bruke en template.
Det finnes ganske mange av disse faktisk: [SAFE Stack](https://safe-stack.github.io), [SAFEr.Template](https://github.com/Dzoukr/SAFEr.Template), [SAFE.Simplified](https://github.com/Zaid-Ajaj/SAFE.Simplified) med flere, men SAFE Stack er den jeg har brukt mest.

Siden jeg vil holde oppsettet i denne artikkelserien så enkelt som mulig har jeg lagd en minimalistisk stack på egen hånd.

## Dagens plan
Målet med denne artikkelen er å vise frem hvor enkelt og morsomt det er å lage frontend-applikasjoner med F#.
Den endelige appen vi lager her er ikke produksjonsklar og har noen problemer, men jeg håper det er mulig å fokusere på
hvordan det er å skrive React i F# heller enn forbedringspotensialet.

Det viktigste verktøyet i denne stacken er [Fable](https://fable.io). Fable tillater oss å transpilere F# koden vår over til JavaScript.
Dette trenger vi da vi ønsker å bruke F# som en TypeScript erstatning.
Vi kommer også til å ha typesikker styling ved hjelp av mitt bibliotek [Fss](https://github.com/Bjorn-Strom/FSS).
Til slutt skal vi se hvor enkelt det er å dele kode mellom frontend og backend, se litt ekstra på promises og avslutte med
en liten tankerekke på hvorfor dette er en interessant stack.

Frontend er et veeeldig stort tema og det er umulig å dekke alt i én artikkel. Se på denne som en introduksjon til Frontend i F#, så kan det komme flere mer spissede temaer i fremtiden.

I et forsøk på holde denne artikkelen relativt kort dekker jeg ikke all koden som denne frontenden består av. Jeg har prøvd å trekke frem de delene jeg mener er av størst interesse.
Om du vil ha tilgang til hele kildekoden er den å finne på GitHub.


## How to React?
Som nevnt er [Fable](https://fable.io) et bibliotek som lar oss skrive JavaScript i F#.
Hvis du kombinerer dette med [Feliz](https://github.com/Zaid-Ajaj/Feliz) så kan du skrive React komponenter som vanlig,
du kan bruke hooks, context, portals, error boundaries, alt som du er kjent med fra "vanlig" React - bare på en immutabel og typesikker måte.

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

Systemet er nokså enkelt: `Html.` etterfulgt av hvilket Html element du ønsker deg.
Dette er bare funksjoner så du får automatisk IDE hjelp samtidig som du slipper å rote med start og slutt tags.

`[<ReactComponent>]` er en attributt som forteller Feliz og Fable at dette skal være en react komponent. Dette gjør at du kan definere funksjonen som du vil og bibliotekene vil ta seg av evt optimaliseringer.

### Hook, line, and sinker

Noe annet vi ofte gjør i React er å skrive egne hooks.
Hvordan gjør man så dette i F#?

Det er egentlig ganske enkelt. Vi bruker `[<Hook>]` attributten.
Som et eksempel skal vi implementere en hook en kollega introduserte meg for, og
som du kan lese mer om [her](https://blogg.bekk.no/kaptein-krok-%EF%B8%8F-useeffectonce-ea28aacd6919).
`useEffectOnce` er en hook som kun skal kjøres én gang.

```fsharp
[<Hook>]
let useEffectOnce callback =
    let (hasRun, setHasRun) = React.useState(false)

    React.useEffect((fun () ->
       if not hasRun then
           callback()
           setHasRun(true)
    ), [| hasRun :> obj; callback :> obj |])
```

Dette ligner jo også på TypeScript.
De største forskjellene er:
- At vi trenger en attributt så Feliz vet at dette skal bli til en hook.
- Vi bruker parantes for tupler.
- Alle typer i F# er en implementasjon av `obj` typen. Men det finnes ingen implisitt konvertering til dette, da må vi ekspisitt kaste verdiene i dependency lista vår til en `obj` med `:>` operatoren.


## Suit up!
Stilig, men hva med styling?
For å være litt shameless kommer jeg til å bruke [mitt eget](https://github.com/Bjorn-Strom/FSS) styling bibliotek, men du kan også bruke inline styles, SASS, LESS, type providers eller gode gammel CSS om du ønsker det.
Fordelen med Fss er typesikkerhet. Programmet ditt kompilerer ikke om du har skrevet stylingen din feil (at det ser bra ut er desverre ingen garanti).

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
- Det ser likt ut om CSS, tanken bak Fss er at om du kan skrive CSS kan du også skrive Fss.
- Flere ting?

## Tilbake til oppskrifter
Nå som vi har litt bakgrunn for hva vi skal gjøre, og hvordan - er det igjen oppskriftene som står for tur.
La oss planlegge litt hvordan denne siden skal se ut og hvordan den skal fungere.

Vi delte oppskriftene inn i gruppene *frokost*, *lunsj*, *middag* og *dessert*. Så hver av disse burde ha sin egen side hvor man kan se alle oppskriftene innenfor en enkelt gruppe.
Fra den siden kan man klikke seg inn på en enkelt oppskrift for å se ingrediensene og steg.
Vi trenger også en side for å legge til nye oppskrifter.
Til slutt burde vi også legge inn en velkomstside, så brukerne våre føler seg velkommen.

Teknisk tenker jeg noe slik:
- På page load lastes alle oppskrifter og vi lagrer dem i en context.
- Noe annet vi lagrer i contexten er hvilken side vi ser på.
- Basert på hvilken side vi ser på endrer vi hva som rendres. Her ville vi nok helst brukt routing, men det skipper vi i dag.
- Vi har en egen side for man kan lage nye oppskrifter og sende dem til backend.
- Når vi oppretter en ny oppskrift blir vi videresendt til dens side.
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
Som vi husker fra før kan en slik datatype bli sett på som en *eller* type.
Så denne typen definerer ett view i appen vår.
Vi ser også at `RecipeDetails` er det eneste viewet som har en verdi knyttet til seg.
I dette tilfellet er den verdien en oppskrift som vi har definert før.

En annen type vi skal definere gjør det lettvint å holde på med ekstern data. Denne kaller vi for `RemoteData` og den sier noe om statusen på dataen vi har i applikasjonen vår. Denne dataen kan holde på å bli lastet, være hentet eller det har kunne forekommet en feil.

Vi kan definere denne slik:
```fsharp
type RemoteData<'t> =
    | Fetching
    | Data of 't
    | Failure of string
```
Atter en *eller* type. Her ser vi at dataen vår kan enten:
være i ferd med å lastes, være lastet, eller det kan ha forekommet en feil.
Dette er noe vi enkelt kan skjekke i applikajonskoden vår.

Legg merke til at vi tar inn en generisk type `t` her som definerer hvilken type dataen vi henter har.
Den kan være hvilken som helst datatype, alt fra string, ints, objekter eller lister av disse.

## Lagring av data
Før vi går videre kan vi også sette opp *storen* vår.

Vi ønsker å holde styr på 2 ting:
1. Oppskriftene våre
2. Hvilket view vi er inne på nå

Vi trenger også en måte å endre disse på.
Så vi trenger *actions*, en *reducer* og en initial *store*.

La oss starte med å definere datatypen til storen:
```fsharp
type Store =
    { Recipes: Recipe list RemoteData
      View: View }
```
`Recipe list RemoteData` leses forøvrig slik: `RemoteData<list<Recipe>>`. Du kan også skrive typene på denne måten om du vil, da F# godtar begge variantene.
Som nevnt tidligere lagrer vi oppskriftene og hvilket view vi er inne i.
Dette bruker oppskrift typen og definerte for lenge siden, sammen med `RemoteData` og `View` som vi definerte i stad.

Vår store kommer til å ha 3 actions:
```fsharp
type StoreAction =
    | SetRecipes of Recipe list RemoteData
    | AddRecipe of Recipe
    | SetCurrentView of View
```
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
[<Hook>]
let useStore() =
    React.useContext(storeContext)
```

Veldig mye greier...
Men nå har vi en store vi kan bruke slik:
```fsharp
let (store, dispatch) = useStore()
```
Hvor `store` er et objekt som er vår nåværende store og `dispatch` en funksjon som lar oss utføre actions.

La oss koble dette sammen til en app!

## Slafs!
La oss starte å implementere `Slafs!`, **den** nye store nettsiden for matoppskrifter!

Okei, nå vet vi hvordan vi lager React-komponenter og vi vet hvordan vi bruker staten vår.
La oss lage en komponent som bruker `useEffect` til å hente alle oppskriftene våre og lagre dem i staten vår.

```fsharp
let Container() =
    // Hent ut state og dispatch
    let (state, dispatch) = useStore()

    // Bruk useEffectOnce hooken vår
    Hooks.useEffectOnce((fun () ->
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
        |> Promise.start) )
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
dataen vi mottok med `_`. Vi trenger ikke å prop drille denne her når den ligger i store.
Til slutt dersom vi mottar en feil lagrer vi den i `e` og viser den.
Enkelt og greit, ikke sant?

### Nye oppskrifter
Vi trenger en egen side for å legge til nye oppskrifter.
Her kommer jeg ikke til å gå igjennom hvordan dette rendres, men mer logikken som tillater oss å endre, poste oppsrifter og oppdatere storen med nye oppskrifter.

Vi lager en state hook for hver del av oppskriften vi vil lagre.
Hvis vi tenker tilbake til oppskrift typen så vet vi at vi trenger:
- En tittel
- En beskrivelse
- Hvilket måltid det er
- Hvor lang tid måltidet tar å tilberede
- En liste med steg
- En liste med ingredienser
- Antall porsjoner måltidet består av
```fsharp
let NewRecipeView () =
    let (_, dispatch) = useStore()

    let (title, setTitle) = React.useState ""
    let (description, setDescription) = React.useState ""
    let (meal, setMeal) = React.useState Breakfast
    let (time, setTime) = React.useState 0.
    let (steps, setSteps) = React.useState<string list> List.empty
    let (ingredients, setIngredients) = React.useState<Ingredient list> List.empty
    let (portions, setPortions) = React.useState 0
```

Når vi så ønsker å oppdatere et av disse feltene bruker vi hooken. Det kan se slik ut:
```fsharp
Html.input [
    prop.onChange setTitle
    prop.value title
]
```

Nå trenger vi en funksjon for å lagre oppskriften.
Om vi skulle ha gjort dette skikkelig ville vi nok trukket ut all nettverkskode ut i en egen fil og laget en fin abstraksjon rundt dette.
For å illustrere hvordan man kan utføre nettverkskall er det nok enklere å bare lage en closure `saveRecipe` funksjon rett i denne komponenten.

```FSharp
let saveRecipe () =
    // Denne funksjonen skrev vi i den andre artikkelen i denne serien. Her har vi flyttet den til Shared.fs
    let recipe = createRecipe title description meal time steps ingredients portions

    // Tilsier hvilke egenskaper nettverkskallet vårt skal ha
    // Vi vil at det skal et POST kall som bruker JSON
    // Vi spesifiserer også hvordan JSON skal encodes så vi er sikre at backend får decoda dette.
    let properties =
        [ RequestProperties.Method HttpMethod.POST
          requestHeaders [ ContentType "application/json" ]
          RequestProperties.Body (unbox(Encode.Auto.toString(4, recipe, caseStrategy = CamelCase))) ]

    // Her definerer vi hvordan kallet faktisk skal oppføre seg
    fetch "http://localhost:5000/api/recipe" properties
    // Vi vil også lagre resultatet i storen vår og bytte view til den nye oppskriften
    |> Promise.map(fun _ ->
        dispatch (AddRecipe recipe)
        dispatch (SetCurrentView (RecipeDetails recipe)))
    // Eksplisitt starte promiset
    |> Promise.start
```

## Kode-deling
Noe jeg liker med denne stacken er å bruke F# både frontend og backend.
Heldigvis er dette også veldig enkelt å få til.
Man har et eget prosjekt i solution hvor man plasserer all kode som man vil skal dele mellom frontend og backend.
I vårt prosjekt plasserte jeg all oppskriftkode og alle hjelpefunksjoner som trengs i hele stacken.
Når man så importerer koden blir det transpilert til JavaScript med fable i frontenden og kompilert i backenden.

- Da slipper man å context switche når man bytter mellom de to
- Trenger ikke gjenta samme kode flere plasser
- Enklere validering
- Dele hjelpefunksjoner

## Promises
Jeg føler det kan være en god ide å ta en ekstra runde på promises og hvordan de funker i F# kontra JS.

I JavaScript har vi 2 måter å skrive promises på,
`then` eller `async` notasjon.

For å bruke det siste POST eksempelet vår vil man kunne skrevet det i JavaScript på disse to måtene.

Med `.then`
```javascript
const properties = { method: 'POST',
    headers: {
        'Content-Type': 'application/json',
    },
    body: JSON.stringify(data),
}
fetch('http://localhost:5000/api/recipe', properties).then(() => {
    // Lagre ting i store
})
```

Med `async`

```javascript
const saveRecipe = async () => {
    const recipe = createRecipe(...)
    const properties =
        { method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(recipe),
        }
    await fetch('http://localhost:5000/api/recipe', properties)
    // lagre ting i store
}
```

Tro det eller ei så har F# tilsvarende måter å utføre promises.
I vår kode har vi brukt
```FSharp
let properties =
    [ ..... ]
fetch "http://localhost:5000/api/recipe" properties
|> Promise.map(fun _ ->
// lagre ting i store)
|> Promise.start
```
Hvor `Promise.map` kan sees på som F# sin verson av `.then` (dette er ikke strengt tatt riktig, men jeg føler det er en fin måte å tenke på dette når man først starter med F#)

I tillegg finnes det en annen syntax for dette i F# som heter (computation expressions)[https://fsharpforfunandprofit.com/series/computation-expressions/] som man kan tenke på som en async funksjon i JavaScript (Igjen, ikke helt riktig, men nyttig sted å starte).

I F# vil man kunne skrevet det tilsvarende slik:
```fsharp
let saveRecipe () =
    let recipe = createRecipe ...
    let properties = [...]
    promise {
        do fetch "http://localhost:5000/api/recipe" properties
        do dispatch (AddRecipe recipe)
        do dispatch (SetCurrentView (RecipeDetails recipe)))
    }
    |> Promise.start
```

Den største forskjellen, sett bort fra syntax, er at du i F# eksplisitt må starte promiset.

## Why tho?
Oookei, men hvorfor kan jeg ikke bare bruke TypeScript?
Vell, det kan du jo selvfølgelig gjøre!

Men, jeg vil påstå at F# gjør den samme jobben - bare bedre.
Med F# får du:
- Immutabilitet uten å bruke tredjeparts biblioteker
- Slipper å tenke like mye på JS, det er gjerne i det grenselandet TS feiler
- Du slipper implicit any.
- Veldig sikker refaktorering
- Sterkt statisk typesystem

Da har du endelig nådd slutten!
Så det er en del forbedringspotensial i dette prosjektet:
- Nettverkskall kan fort bli gjort litt smoothere.
- Routing er alltid en forbedring
- Hadde ikke skadet å gått over stylingen en tur, siden er ikke akkurat pen!
- Vi kan ikke slette eller oppdatere oppskrifter.

Om du har lyst til å se appen kan du se den her, selve koden finner du her.

Sees neste gang, da blir det databaser!