# DemoApp C4 Model

- C4 Architectuur View Model - Simon Brown https://c4model.com/
- Gebruik van Structurizr - Diagrams as code https://structurizr.com/help/code

## Voorbeeld
Het C4 model is te vinden via deze link: https://structurizr.com/share/59853

## Instellen Structurizr

- Registreer met je HAN email adres voor een Academic licentie (3 workspaces) 
- Maak een Workspace aan
- Kopieer Workspace ID, API key en API secret

## Instellen Visual Studio solution

De gegevens van de Workspace worden bewaard in een secret. Voer de volgende stappen uit in de Developer Powershell:
- dotnet user-secrets init --project DemoAppModel
- dotnet user-secrets set "DemoAppModel:WorkspaceId" "**[WORKSPACE_ID]**" --project DemoAppModel
- user-secrets set "DemoAppModel:ApiKey" "**[APIKEY]**" --project DemoAppModel
- dotnet user-secrets set "DemoAppModel:ApiSecret" "**[APISECRET]**" --project DemoAppModel
- dotnet user-secrets list --project DemoAppModel


