# CasusFda
De applicatie haalt de **top 10 makelaars** op met de meeste objecten (huizen) binnen een opgegeven zoekopdracht.  
De architectuur volgt het **Clean Architecture** patroon en bevat tests voor alle lagen.

## Architectuur
Het project is opgezet volgens **Clean Architecture**, waarbij elke laag een duidelijke verantwoordelijkheid heeft:

### Domain
Bevat de domeinmodellen.  
In deze casus is deze laag aanwezig maar nog leeg, omdat er geen specifieke domeinlogica nodig is.

### Application
Bevat de interfaces en logica voor het ophalen en verwerken van data.

### Infrastructure
Bevat de implementaties van externe afhankelijkheden zoals HTTP-clients en API-communicatie.

### Console
De entrypoint-applicatie die de use case uitvoert en de resultaten toont.

# Applicatie starten

Start het Console project om de applicatie uit te voeren.

# Gebruik van AI tools

- Copilot voor code suggesties.
- ChatGPT voor uitleg en hulp bij complexe problemen.
  - Zoeken van documentatie, voorbeelden en verificatie van clean architecture principes.
  - Polly pipeline voor foutafhandeling en retry logica.
  - Unit tests laten genereren op basis van enkele tests.
  - Verder kleine code snippets en uitleg.