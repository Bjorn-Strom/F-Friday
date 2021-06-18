# F# Friday 2

Hei og velkommen til den andre posten i en serie om programmeringsspråket F#!

[Forrige gang](https://medium.com/r/?url=https%3A%2F%2Fblogg.bekk.no%2Ff-friday-1-39f63618d2e4) startet vi med en kort og lett introduksjon til hva F# er og hva du kan bruke det til. Denne gangen skal vi bruke litt mer tid til å se på hva F# har å by på samtidig som vi skal skrive litt kode. Vi skal nemlig starte å implementere et system for å organisere matoppskrifter!

## Hva står på menyen?
Systemet er nokså enkelt og består av:
- **Measurements** er målenheter. Her kommer vi til å implementere noen av de enhetene som stadig vekk dukker opp. Disse kan se slik ut: 10g, 1ss eller lignende.
- **Ingredients**. Som er ingrediensene en oppskrift kan bestå av. En ingrediens består av en målenhet og et navn.
- **Recipe**. Selve oppskriften blir den største typen vi kommer til å lage i dag. Den kommer til å ha en Id så vi kan unikt identifisere oppskrifter. Vi vil vite slags måltid det er, frokost, middag, lunsj eller dessert. Hvor lang tid det vil ta å lage dette om man følger oppskriften. Steg som beskriver hvordan man lager maten og alle ingrediensene som er nødvendig. Vi skal også ha et felt som sier hvor mange porsjoner denne retten består. Helt til slutt trenger vi under-oppskrifter, så en oppskrift kan lenke til andre oppskrifter.

Alt dette skal vi nå implementere i F#. Dette blir gøy!

## Let's get cooking!
Om du ønsker å skrive kode as we go kan du skrive følgende i din terminal for å opprette et enkelt F# prosjekt: `dotnet new console --lang f# -o recipeTracker` eller du kan bruke din IDE til å opprette noe lignende.

La oss starte med å implementere målenheter! Vi kommer til å bruke en [*discriminated union*](https://medium.com/r/?url=https%3A%2F%2Ffsharpforfunandprofit.com%2Fposts%2Fdiscriminated-unions%2F) (DU) for å representere disse. Jeg liker å tenke på denne data strukturen som en **eller** type. For eksempel om vi ønsker å definere en type for å håndtere login resultat kan det se slik ut:
```fsharp
type LoginResult =
     | Success of User
     | Error of string
```
Da vil en LoginResult kunne være suksess **eller** en feilmelding. Hver av casene her har en egen type knyttet til seg. Dersom innloggingen gikk greit kan vi hente ut brukeren, dersom den feilet har vi feilmeldingen i string format. Hver av disse casene kan brukes som konstruktører av LoginResult typen. Det kan se slik ut: `Error "Feil passord!"`.

Armert med denne stekespaden kan vi nå modelere målenheter i systemet vårt. Vi skulle jo bruke en DU så la oss bare definere den.

```fsharp
type Measurement = 
    | Kg of float
    | G of float
    | Mg of float
    | L of float
    | Dl of float
    | Ml of float
    | Ms of float
    | Ss of float
    | Ts of float
    | Stk of float
```

Her har vi en utvalg vanlige enheter og vi kan alltids utvide senere om vi vil. Vi får dessverre ikke utrettet så mye kun med målenheter. Så la oss lage ingredienser også.

I oppskrifter finner man gjerne målenheter på dette formatet: *200g smør* eller *1ss sukker*, så la oss prøve å modellere noe som ser slik ut. Til dette passer en record bra. En record er en egentlig bare en datastruktur som holder på data. En ingrediens kan da se slik ut:

```fsharp
type Ingredient =
    { Measurement: Measurement
      Name: string }
```

Denne typen tar inn målenheten vi allerede har definert og en string som sier hva slags ingrediens dette er. Nå kan vi lage ingredienser slik:

```fsharp
{ Measurement = G 200
  Name = "Smør" }
```

Problemet med dette er at det ikke ligner veldig på det man finner i oppskrifter. Vi ville jo skrive noe som ligner på *200g smør*. Vi kan derfor opprette en hjelpefunksjon som lager ingredienser for oss.

```fsharp
let ingredient volume measurement name = 
    { Measurement = measurement volume
      Name = name }
```

Dette definerer *ingredient* funksjonen.
Den tar inn:
- **Volume** som er hvor mye av gitt enhet vi vil ha.
- **Measurement**, som er selve målenhet som vi kan bruke som en konstruktør for denne typen
- **Name** som er navnet vi ønsker å gi ingrediensen vår.

Denne funksjonen oppretter type for oss og vi kan bruke den slik: `ingredient 200. G "Smør"` og ser mye mer ut som det vi ville.

## Oi, se her kommer hovedretten
Når det kommer til oppskrifter ønsker vi å vite hva slags måltid denne retten tilhører. Er det en rett man lager til frokost, lunsj, middag eller dessert? Hvis du som meg tenker **eller** her er nok atter en *DU* løsningen.

```fsharp
type Recipe =
        { Id: System.Guid
          Title: string
          Meal: Meal
          Time: float
          Steps: string list
          Ingredients: Ingredient list
          Portions: int
          SubRecipes: System.Guid list }
```
Her også ønsker vi en hjelpefunksjon så vi enklere kan lage oppskrifter. Da kan vi også slippe å manuelt lage ny GUIDer for hånd.
```fsharp
let createRecipe title meal time steps ingredients portions subRecipes =
    { Id = System.Guid.NewGuid()
      Title = title
      Meal = meal
      Time = time
      Steps = steps
      Ingredients = ingredients
      Portions = portions
      SubRecipes = List.map (fun r -> r.Id) subRecipes }
```

## For en saftig biff! 
Nå som vi kan lage oppskrifter burde vi starte med en klassiker. Noe vi sikkert ofte trenger for å lage norsk husmannskost er kokte poteter. Så la oss lage en oppskrift for det:

```fsharp
let koktPotet = 
    createRecipe 
       "Kokt potet"
        Dinner
        20.
        [ "Skrubb og skyll potetene"
          "Del potetene i 2"
          "Kok dem i 10-15 minutter til de er gjennomkokte"
        ]
        [ ingredient 800. G "Potet"; ingredient 1. L "Vann"; ingredient 1. Ts "Salt" ]
        4
        []
```
Nå som vi kan koke poteter kan vi lage en rett som trenger kokte poteter og da kan vi henvise til den allerede eksisterende oppskriften for det.

```fsharp
let koteletterOgPotet = 
    createRecipe
        "Koteletter med kokt potet"
        Dinner
        5.
        [  "Grill kotelettene i 3-4 minutt på hver side, dryss med salt og pepper"
        ] 
        [ ingredient 1. Ts "Salt"; ingredient 0.5 Ts "Pepper"; ingredient 4. Stk "Kotelett" ]
        4
        [ koktPotet ]
```

Her har vi en kotelelett oppskrift som har kokte poteter som en underoppskrift.

## Og til dessert
Så hva har vi egentlig fått til?

Vi har:
- Brukt Discriminated Unions for å modellere målenheter og måltider.
- Definert målenhetene med en record så vi kan lage ingredienser og laget en hjelpefunksjon så de blir enklere å lage.
- Deretter laget vi typen til selve oppskriften. Her også brukte vi en hjelpefunksjon så vi slipper å lage GUIDer for hånd hver gang.

Om du vil se koden i sin hellhet kan du se koden på GitHub(LINK HER).

Dette er et veldig enkelt og simpelt system, men det tillater oss å lage oppskrifter som kan ha under-oppskrifter. Dette kan vi jobbe videre med og det er akkurat det vi skal! Neste gang skal vi flytte denne koden backend slik at vi kan serve våre egne og potensielt andres oppskrifter ut til verden!