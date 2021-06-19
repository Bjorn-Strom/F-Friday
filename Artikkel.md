# F# Friday 4

Hei og velkommen til den fjerde posten i en serie om programmeringsspråket F#!

[Forrige gang]() laget vi en backend som kunne serve oppskriftene våre. Med den kan vi hente alle, lage nye, oppdatere eller slette oppskrifter. Denne gangen skal vi lage en enkel frontend som kan kommunisere med denne backenden og vise frem de fine oppskriftene våre. 

Istedenfor å gå igjennom hele oppsettet for hvordan man kan legge til en server, client og hvordan man deler kode mellom dem så skal vi bruke en template.
Det finnes ganske mange av disse faktisk:[SAFE Stack](https://safe-stack.github.io), [SAFEr.Template](https://github.com/Dzoukr/SAFEr.Template), [SAFE.Simplified](https://github.com/Zaid-Ajaj/SAFE.Simplified).

Siden jeg vil holde dette så enkelt som mulig så skal jeg bruke min [Elmish-Fss-Stack](https://github.com/Bjorn-Strom/elmish-fss-stack).

Dersom det er interesse i å vite hvordan man kan sette opp noe slikt, eller du vil vite hvordan dette henger sammen er det bare å gi meg beskjed, så kan jeg skrive noe eget om akkurat dette, men i denne artikkelen vil jeg fokusere på frontend kode.

## Dagens plan
- Vi skal lære oss hvordan MVU-modellen funker.
- 