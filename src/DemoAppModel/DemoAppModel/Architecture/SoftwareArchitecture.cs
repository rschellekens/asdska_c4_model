using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DemoAppModel.Core;
using Microsoft.Extensions.Logging;
using Structurizr;
using Structurizr.Api;
using Structurizr.Core.Util;
using Structurizr.Documentation;

namespace DemoAppModel.Architecture
{
    public class SoftwareArchitecture : ISoftwareArchitecture
    {
        private const string BedrijfsNaam = "Demo Softwarebedrijf";
        private const string Gebruikt = "gebruikt";
        private const string InternalSystemTag = "Intern Systeem";
        private const string ExternalSystemTag = "Extern Systeem";
        private const string WebBrowserTag = "Web Browser";
        private const string DatabaseTag = "Database";


        private readonly IStructurizrService structurizrService;
        private readonly ILogger<SoftwareArchitecture> logger;
        private readonly Secrets secrets;

        private Workspace workspace;
        private Model model;
        private ViewSet views;

        public SoftwareArchitecture(IStructurizrService structurizrService, ILogger<SoftwareArchitecture> logger, Secrets secrets)
        {
            this.structurizrService = structurizrService;
            this.logger = logger;
            this.secrets = secrets;
        }

        public void SetWorkspace(string workspaceName, string workspaceDescription)
        {
            workspace = new Workspace(workspaceName, workspaceDescription);
            views = workspace.Views;
            model = workspace.Model;
            model.Enterprise = new Enterprise($"{BedrijfsNaam}");
        }

        private void BuildC4Model()
        {
            // Add stakeholders en Systemen
            SoftwareSystem boekhoudSysteem = model.AddSoftwareSystem(Location.Internal, "Boekhoudsysteem", "Boekhoudsysteem voor midden en kleinbedrijf en ZZP'er");
            
            SoftwareSystem itsmSysteem = model.AddSoftwareSystem(Location.Internal, "ITSM Systeem", "Interne CRM applicatie");
            itsmSysteem.AddTags(InternalSystemTag);
            boekhoudSysteem.Uses(itsmSysteem, "Haalt abonnement gegevens op");

            SoftwareSystem website = model.AddSoftwareSystem(Location.Internal, "Website", $"publieke website van {BedrijfsNaam}");
            website.AddTags(WebBrowserTag);
            website.Uses(itsmSysteem, "Registreert een abonnement");

            SoftwareSystem webshop = model.AddSoftwareSystem(Location.External, "Webshop", $"Webshopkoppeling names klant met Boekhoudsysteem");
            webshop.AddTags(ExternalSystemTag);
            boekhoudSysteem.Uses(webshop, "krijgt gegevens van");

            SoftwareSystem bank = model.AddSoftwareSystem(Location.External, "Bank", $"Bankkoppeling names klant met Boekhoudsysteem");
            bank.AddTags(ExternalSystemTag);
            boekhoudSysteem.Uses(bank, "krijgt gegevens van");

            Person hoofdgebruiker = model.AddPerson(Location.External, "Hoofdgebruiker", "De gebruiker van de klant die het abonnenement heeft afgesloten");
            hoofdgebruiker.Uses(boekhoudSysteem, Gebruikt);
            hoofdgebruiker.Uses(website, Gebruikt);

            Person gebruiker = model.AddPerson(Location.External, "Gebruiker", "De medewerker van de klant");
            gebruiker.Uses(boekhoudSysteem, Gebruikt);

            Person accountant = model.AddPerson(Location.External, "Accountant", "De accountant van de klant");
            accountant.Uses(boekhoudSysteem, Gebruikt);
            hoofdgebruiker.InteractsWith(accountant, "Vraagt controle aan bij");

            Person helpdesk = model.AddPerson(Location.Internal, "Helpdeskmedewerker", $"Helpdeskmedewerker van {BedrijfsNaam}");
            helpdesk.Uses(boekhoudSysteem, Gebruikt);
            helpdesk.Uses(itsmSysteem, Gebruikt);
            hoofdgebruiker.InteractsWith(helpdesk, "Vraagt hulp van");

            SystemLandscapeView systemLandscapeView = views.CreateSystemLandscapeView("SystemLandscape", "System Context diagram Boekhoudsysteem.");
            systemLandscapeView.AddAllElements();
            systemLandscapeView.EnableAutomaticLayout();


            // containers
            Container coreModule = boekhoudSysteem.AddContainer("Core Module", "Basis module voor inrichting, gebruikers", "C#");
            coreModule.AddTags(Tags.Container);
            coreModule.Uses(itsmSysteem, "krijgt gegevens van");

            Container bankModule = boekhoudSysteem.AddContainer("Bank Module", "Importeren van betaalgegevens", "C#");
            bankModule.AddTags(Tags.Container);
            bankModule.Uses(bank, "krijgt gegevens van");
            
            Container koppelingModule = boekhoudSysteem.AddContainer("Koppelingen Module", "Module voor inrichten diverse externe koppelingen", "C#");
            koppelingModule.AddTags(Tags.Container);
            koppelingModule.Uses(webshop, "krijgt gegevens van");

            Container boekhoudModule = boekhoudSysteem.AddContainer("Boekhoud Module", "Basis onderdelen van een Boekhoudsysteem zoals dagboeken en rapportages", "C#");
            boekhoudModule.AddTags(Tags.Container);
            boekhoudModule.Uses(webshop, "krijgt gegevens van");

            Container facturatieModule = boekhoudSysteem.AddContainer("Facturatie Module", "Facturatie en Offerte module", "C#");
            facturatieModule.AddTags(Tags.Container);
            facturatieModule.Uses(boekhoudModule, "geeft verkooporders aan");

            Container importModule = boekhoudSysteem.AddContainer("Import Module", "Module voor import en export gegevens", "C#");
            importModule.AddTags(Tags.Container);
            importModule.Uses(boekhoudModule, "geeft relaties aan");
            importModule.Uses(facturatieModule, "geeft artikelen aan");

            hoofdgebruiker.Uses(coreModule, Gebruikt);
            hoofdgebruiker.Uses(boekhoudModule, Gebruikt);
            hoofdgebruiker.Uses(facturatieModule, Gebruikt);
            hoofdgebruiker.Uses(bankModule, Gebruikt);
            hoofdgebruiker.Uses(koppelingModule, Gebruikt);
            hoofdgebruiker.Uses(importModule, Gebruikt);

            gebruiker.Uses(boekhoudModule, Gebruikt);
            gebruiker.Uses(facturatieModule, Gebruikt);
            gebruiker.Uses(bankModule, Gebruikt);

            accountant.Uses(boekhoudModule, Gebruikt);
            helpdesk.Uses(boekhoudModule, Gebruikt);

            Container databaseSystem = boekhoudSysteem.AddContainer("Systeem Database", "Opslag gebruikers, abonnement, administratie gegevens, hashed authentication credentials, access logs, etc.", "Relational Database Schema");
            databaseSystem.AddTags(DatabaseTag);
            coreModule.Uses(databaseSystem, Gebruikt);

            Container databaseAccount = boekhoudSysteem.AddContainer("Boekhouding Database", "Opslag boekhouding per abonnement per administratie", "Relational Database Schema");
            databaseAccount.AddTags(DatabaseTag);
            boekhoudModule.Uses(databaseAccount, Gebruikt);
            facturatieModule.Uses(databaseAccount, Gebruikt);
            bankModule.Uses(databaseAccount, Gebruikt);

            // Components
            Component bankImportView = bankModule.AddComponent("Bank statement Import", "Importscherm bankafschriften", "ASP.Net Webform");
            bankImportView.AddTags(WebBrowserTag);
            Component bankPaymentLogic = bankModule.AddComponent("Payment logic service", "Businesslaag bankafschriften", "C#");
            bankPaymentLogic.AddTags(InternalSystemTag);
            Component bankPaymentData = bankModule.AddComponent("Payment data service", "Datalaag bankafschriften", "C#");
            bankPaymentData.AddTags(InternalSystemTag);

            bankImportView.Uses(bankPaymentLogic, Gebruikt);
            bankPaymentLogic.Uses(bankPaymentData, Gebruikt);
            bankPaymentLogic.Uses(bank, Gebruikt);

            Component bankPaymentView = bankModule.AddComponent("Bank payments", "Betaalopdrachten", "ASP.Net Webform");
            bankPaymentView.AddTags(WebBrowserTag);
            bankPaymentView.Uses(bankPaymentLogic, Gebruikt);

            Component bankInstellingView = bankModule.AddComponent("Instellingen Bankstatements", "Instellingen bankafschriften", "ASP.Net Webform");
            bankInstellingView.AddTags(WebBrowserTag);
            Component bankInstellingLogic = bankModule.AddComponent("Bankinstellingen logic service", "Businesslaag bankinstellingen", "C#");
            bankInstellingLogic.AddTags(InternalSystemTag);
            Component bankInstellingData = bankModule.AddComponent("Bankinstellingen data service", "Datalaag bankinstellingen", "C#");
            bankInstellingData.AddTags(InternalSystemTag);

            bankInstellingView.Uses(bankInstellingLogic, "Leest en schrijft naar");
            bankInstellingLogic.Uses(bankInstellingData, "Leest en schrijft naar");

            bankPaymentData.Uses(databaseAccount, Gebruikt, "Linq2Sql");
            bankInstellingData.Uses(databaseAccount, Gebruikt, "Linq2Sql");

            Component importExportView = importModule.AddComponent("Artikel Import", "Importscherm artikelen", "ASP.Net Webform");
            importExportView.AddTags(WebBrowserTag);
            Component importExportLogic = importModule.AddComponent("Import Export logic service", "Businesslaag import export functionaliteit", "C#");
            importExportLogic.AddTags(InternalSystemTag);
            Component importExportData = importModule.AddComponent("Import Export data service", "Datalaag import export functionaliteit", "C#");
            importExportData.AddTags(InternalSystemTag);

            importExportView.Uses(importExportLogic, Gebruikt);
            importExportLogic.Uses(importExportData, Gebruikt);
            importExportData.Uses(databaseAccount, Gebruikt, "Linq2Sql");

            // Add Views
            SystemContextView contextView = views.CreateSystemContextView(boekhoudSysteem, "SystemContext", "System Context diagram Boekhoudsysteem.");
            contextView.AddNearestNeighbours(boekhoudSysteem);
            contextView.EnableAutomaticLayout();

            ContainerView containerView = views.CreateContainerView(boekhoudSysteem, "Containers", "Het container diagram voor het boekhoudsysteem.");
            containerView.EnableAutomaticLayout();
            containerView.Add(hoofdgebruiker);
            containerView.Add(gebruiker);
            containerView.Add(accountant);
            containerView.Add(helpdesk);
            containerView.AddAllContainers();
            containerView.Add(webshop);
            containerView.Add(bank);
            containerView.Add(itsmSysteem);

            ComponentView bankComponentView = views.CreateComponentView(bankModule, "Bank Components", "Component diagram van de Bank module");
            bankComponentView.EnableAutomaticLayout();
            bankComponentView.Add(databaseAccount);
            bankComponentView.AddAllComponents();
            bankComponentView.Add(bank);

            ComponentView importExportComponentView = views.CreateComponentView(importModule, "Import-Export Components", "Component diagram van de Import Export module.");
            importExportComponentView.EnableAutomaticLayout();
            importExportComponentView.Add(databaseAccount);
            importExportComponentView.AddAllComponents();

            // Set Styling
            Styles styles = views.Configuration.Styles;
            styles.Add(new ElementStyle(Tags.SoftwareSystem) { Background = "#1168bd", Color = "#ffffff" });
            styles.Add(new ElementStyle(Tags.Container) { Background = "#438dd5", Color = "#ffffff" });
            styles.Add(new ElementStyle(Tags.Component) { Background = "#85bbf0", Color = "#000000" });
            styles.Add(new ElementStyle(Tags.Person) { Background = "#08427b", Color = "#ffffff", Shape = Shape.Person });
            styles.Add(new ElementStyle(InternalSystemTag) { Background = "#999999", Color = "#ffffff" });
            styles.Add(new ElementStyle(ExternalSystemTag) { Background = "#999999", Color = "#ffffff" });
            styles.Add(new ElementStyle(WebBrowserTag) { Shape = Shape.WebBrowser });
            styles.Add(new ElementStyle(DatabaseTag) { Shape = Shape.Cylinder });
        }

        public void PublishC4Model()
        {
            BuildC4Model();
            structurizrService.PublishModel(workspace);
        }
    }
}
