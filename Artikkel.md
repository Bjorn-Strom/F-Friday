### F# Friday 2

Hei og velkommen til den andre posten i en serie om programmeringsspråket F#!

[Forrige gang](https://blogg.bekk.no/f-friday-1-39f63618d2e4) startet vi med en
kort og lett introduksjon til hva F# er og hva du kan bruke det til. Denne
gangen skal vi bruke litt mer tid til å se på hva F# har å by på samtidig som vi
skal skrive litt kode. Vi skal nemlig starte å implementere et system for å
organisere matoppskrifter!

#### Hva står på menyen?

Systemet er nokså enkelt og består av: 

* **Measurements **er målenheter. Her kommer vi til å implementere noen av de
enhetene som stadig vekk dukker opp. Disse kan se slik ut: *g, ss *eller
lignende. 
* **Ingredients**. Som er ingrediensene en oppskrift kan bestå av. En ingrediens
består av en målenhet, volum og et navn.
* **Recipe***. *Selve oppskriften blir den største typen vi kommer til å lage i
dag. I tillegg til en tittel og beskrivelse trenger den en *Id *så vi kan unikt
identifisere oppskrifter. Vi vil vite slags måltid det er, frokost, middag,
lunsj eller dessert. Hvor lang tid det vil ta å lage dette om man følger
oppskriften. Steg som beskriver hvordan man lager maten og alle ingrediensene
som er nødvendig. Og til slutt et felt som sier hvor mange porsjoner denne
retten kommer til å ha. 

Alt dette skal vi nå implementere i F#. Dette blir gøy!

#### Let’s get cooking!

Om du ønsker å skrive kode as we go kan du skrive følgende i din terminal for å
opprette et enkelt F# prosjekt: `dotnet new console -lang "F#" -o
recipeTracker`eller du kan bruke din IDE til å opprette noe lignende.

La oss starte med å implementere målenheter! Vi kommer til å bruke en
[discriminated
union](https://fsharpforfunandprofit.com/posts/discriminated-unions/)(DU) for å
representere disse. Jeg liker å tenke på denne data strukturen som en **eller***
*type. For eksempel om vi ønsker å definere en type for å håndtere login
resultat kunne den se slik ut:
```fsharp
type LoginResult =
     | Success of User
     | Error of string
```
Da vil en LoginResult kunne være suksess **eller** en feilmelding. Hver av
casene her har en egen type knyttet til seg. Dersom innloggingen gikk greit kan
vi hente ut brukeren, dersom den feilet har vi feilmeldingen i string format.
Hver av disse casene kan brukes som konstruktører av *LoginResult *typen. Det
kan se slik ut: `Error "Feil passord!"`. Vi kommer til å se mye mer på DUs
senere.

Armert med denne stekespaden kan vi nå modelere målenheter i systemet vårt. Vi
skulle jo bruke en DU så la oss bare definere den.

```fsharp
type Measurement = 
    | Kg
    | G 
    | Mg 
    | L 
    | Dl 
    | Ml
    | Ms 
    | Ss 
    | Ts 
    | Stk 
```

Vi har ikke knyttet noen typer til denne, som også går greit. Da kan den tenkes
mer på som en enum (**merk: **det er faktisk ikke en enum). Her har vi en utvalg
vanlige enheter og vi kan alltids utvide senere om vi vil. Vi får dessverre ikke
utrettet så mye kun med målenheter. Så la oss lage ingredienser også.

I oppskrifter finner man gjerne målenheter på dette formatet: *200g smør *eller*
1ss sukker, så la *oss prøve å modellere noe som ser slik ut. Til dette passer
en record bra. En record er en egentlig bare en datastruktur som holder på data.
En ingrediens kan da se slik ut:

```fsharp
type Ingredient =
    { Volume: float
      Measurement: Measurement
      Name: string }
```

Denne typen tar inn målenheten vi allerede har definert, et volum og en string
som sier hva slags ingrediens dette er. Nå kan vi lage ingredienser slik:

```fsharp
{ Volume = 200.
  Measurement = G
  Name = "Smør" }
```

Problemet med dette er at det ikke ligner veldig på det man finner i
oppskrifter. Vi ville jo skrive noe som ligner på *200g smør. *Vi kan derfor
opprette en hjelpefunksjon som lager ingredienser for oss.

Dette definerer `ingredient` funksjonen. 
```fsharp
let ingredient volume measurement name = 
    { Volume = volume
      Measurement = measurement 
      Name = name }
```

Den tar inn:

* **Volume** som er hvor mye av gitt enhet vi vil ha.
* **Measurement**, som er selve målenhet som vi kan bruke som en konstruktør for
denne typen
* **Name** som er navnet vi ønsker å gi ingrediensen vår.

Denne funksjonen oppretter type for oss og vi kan bruke den slik: `ingredient
200. G "Smør"`og ser mye mer ut som det vi ville.

## Oi, se her kommer hovedretten
Når det kommer til oppskrifter ønsker vi å vite hva slags måltid denne retten
tilhører. Er det en rett man lager til frokost, lunsj, middag eller dessert?
Hvis du som meg tenker **eller** her er nok atter en *DU* løsningen. 
```fsharp
type Meal =
    | Breakfast
    | Lunch
    | Dinner
    | Desert
```

Nå skal vi legge inn selve oppskriftstypen. Dette blir den største typen vi har
skrevet så langt og kommer til å bruke alle typene vi har skrevet over. Som vi
allerede vet trenger den å ha:

* **Id** så vi kan unikt identifisere oppskrifter
* **Tittel **og **beskrivelse**.
* **Måltidstype**
* **Tilberedningstid**, 
* **Stegene **som inngår i å lage matretten
* **Ingrediensene **man trenger.
* **Porsjoner** 

Det blir en. del, så la oss bare skrive det inn!

```fsharp
type Recipe =
    { Id: System.Guid
      Title: string
      Description: string
      Meal: Meal
      Time: float
      Steps: string list
      Ingredients: Ingredient list
      Portions: int }
```
Her også ønsker vi en hjelpefunksjon så vi enklere kan lage oppskrifter. Da kan
vi også slippe å manuelt lage nye GUIDer for hånd hele tiden.

```fsharp
let createRecipe title description meal time steps ingredients portions =
    { Id = System.Guid.NewGuid()
      Title = title
      Description = description
      Meal = meal
      Time = time
      Steps = steps
      Ingredients = ingredients
      Portions = portions }
```

#### For en saftig biff!

Nå som vi kan lage oppskrifter burde vi starte med en klassiker. Noe vi sikkert
ofte trenger for å lage norsk husmannskost er kokte poteter. Så la oss lage en
oppskrift for det:

```fsharp
let koktPotet = 
  createRecipe 
     "Kokt potet"
     "En skikkelig, potensielt smakløs, klassiker som du som inngår i ganske mange andre retter."
     Dinner
     20.
     [ "Skrubb og skyll potetene"
       "Del potetene i 2"
       "Kok dem i 10-15 minutter til de er gjennomkokte" ]
     [ ingredient 800. G "Potet"
       ingredient 1. L "Vann"
       ingredient 1. Ts "Salt" ]
     4
```
Nå som vi kan koke poteter kan vi lage en rett som trenger kokte poteter og et
fint sted å bruke gamle middagsrester.

```fsharp
let pyttIPanne =
  createRecipe
    "Pytt i panne"
    "Det evige hvilestedet til gamle middager."
    Dinner
    20.
    [ "Stek baconet og del det inn i biter"
      "Del potetene og inn i terninger og hakk løk."
      "Stek poteten og løken sammen i bacon-fettet"
      "Bland inn baconet"
      "Del paprikaen i biter og dryss over"]
    [ ingredient 2. Stk "Bacon"
      ingredient 4. Stk "Kokte poteter"
      ingredient 1. Stk "Løk"
      ingredient 0.25 Stk "Paprika"]
      2
```

Her har vi en kotelelett oppskrift som har kokte poteter som en underoppskrift.

#### Og til dessert

Så hva har vi egentlig fått til? 

Vi har:

* Brukt *Discriminated Unions *for å modellere målenheter og måltider.
* Definert målenhetene med en *record* så vi kan lage ingredienser og laget en
hjelpefunksjon så de blir enklere å lage.
* Deretter laget vi typen til selve oppskriften. Her også brukte vi en
hjelpefunksjon så vi slipper å lage GUIDer for hånd hver gang.
* Til slutt lagde vi to flotte oppskrifter.

Om du vil se koden i sin hellhet kan du se koden på GitHub(LINK HER).

Dette er et veldig enkelt og simpelt system, men det tillater oss å lage enkle
oppskrifter. Dette kan vi jobbe videre med og det er akkurat det vi skal! Neste
gang skal vi flytte denne koden backend slik at vi kan serve våre egne og
potensielt andres oppskrifter ut til verden!
