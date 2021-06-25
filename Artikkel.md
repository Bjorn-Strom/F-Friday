# F# Friday 4

Hei og velkommen til den fjerde posten i en serie om programmeringsspråket F#!

[Forrige gang]() laget vi en backend som kunne serve oppskriftene våre. Med den kan vi hente alle, lage nye, oppdatere eller slette oppskrifter. Denne gangen skal vi lage en enkel frontend som kan kommunisere med denne backenden og vise frem de fine oppskriftene våre. 

Istedenfor å gå igjennom hele oppsettet for hvordan man kan legge til en server, client og hvordan man deler kode mellom dem så skal vi bruke en template.
Det finnes ganske mange av disse faktisk: [SAFE Stack](https://safe-stack.github.io), [SAFEr.Template](https://github.com/Dzoukr/SAFEr.Template), [SAFE.Simplified](https://github.com/Zaid-Ajaj/SAFE.Simplified) med flere.

Siden jeg vil holde dette så enkelt som mulig har jeg lagd en minimalistisk en til denne artikkelen.

Dersom det finne interesse for å vite hvordan man kan sette opp noe slikt, eller du vil vite hvordan dette henger sammen er det bare å gi meg beskjed, så kan jeg skrive noe eget om akkurat dette, men i denne artikkelen vil jeg fokusere på frontend.

## Dagens plan
I dag skal vi lære om:
- Fable, som er det viktigste verktøyet vi kommer til å se på idag.
- [Feliz](https://github.com/Zaid-Ajaj/Feliz) som er en måte å skrive React på i F#.
- Typesikker styling ved hjelp av mitt bibliotek [Fss](https://github.com/Bjorn-Strom/FSS).
- Hvordan vi kan bruke kode samme F# kode delt mellom frontend og backend.

Da dette er et veldig stort tema er det ikke alt det er mulig å dekke i en artikkel. Ting som Recoil, Remoting, Elm-modellen og slike ting kommer i en senere artikkel.

## Fable
[Fable](https://fable.io) er en måte å transpilere vanlig F# til JavaScript og er et utrolig bra verktøy. Ikke bare tillater det oss å skrive *single page applications* i F#, men vi kan i praksis kjøre F# alle plasser der JavaScript kan kjøres.
Så om vil lage nettsider, node eller elektron applikasjoner så står du fritt til å gjøre det. 