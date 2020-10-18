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


            // Add Views
            SystemContextView contextView = views.CreateSystemContextView(boekhoudSysteem, "SystemContext", "System Context diagram Boekhoudsysteem.");
            contextView.AddNearestNeighbours(boekhoudSysteem);
            contextView.EnableAutomaticLayout();
            










            // Set Styling
            Styles styles = views.Configuration.Styles;
            styles.Add(new ElementStyle(Tags.SoftwareSystem) { Background = "#1168bd", Color = "#ffffff" });
            styles.Add(new ElementStyle(InternalSystemTag) { Background = "#999999", Color = "#ffffff" });
            styles.Add(new ElementStyle(ExternalSystemTag) { Background = "#999999", Color = "#ffffff" });
            styles.Add(new ElementStyle(Tags.Person) { Background = "#08427b", Color = "#ffffff", Shape = Shape.Person });
            styles.Add(new ElementStyle(WebBrowserTag) { Shape = Shape.WebBrowser });
        }

        public void PublishC4Model()
        {
            BuildC4Model();
            structurizrService.PublishModel(workspace);
        }
    }
}
