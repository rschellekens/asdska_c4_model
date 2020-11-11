using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using DemoAppModel.Architecture;
using DemoAppModel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace DemoAppModel
{
    class Program
    {

        public static IConfiguration Configuration { get; set; }

        static void Main(string[] args)
        {
            var serilog = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger(); ;
            // Middleware koppelen
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder=> builder.AddSerilog(logger: serilog, dispose: true)); 

            // UserSecrets t.b.v. gegevens Structurizr Account (zie readme.md)
            var builder = new ConfigurationBuilder().AddUserSecrets<Program>();
            Configuration = builder.Build();

            var secrets = new Secrets
            {
                WorkspaceId = Convert.ToInt64(Configuration["DemoAppModel:WorkspaceId"]),
                ApiKey = Configuration["DemoAppModel:ApiKey"],
                ApiSecret = Configuration["DemoAppModel:ApiSecret"]
            };

            // Autofac IoC
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Populate(serviceCollection);
            containerBuilder.RegisterInstance<Secrets>(secrets);
            containerBuilder.RegisterType<SecretService>().As<ISecretService>();
            containerBuilder.RegisterType<StructurizrService>().As<IStructurizrService>();
            containerBuilder.RegisterType<SoftwareArchitecture>().As<ISoftwareArchitecture>();

            var container = containerBuilder.Build();
            var serviceProvider = new AutofacServiceProvider(container);

            // Debug om secrets te tonen
            var revealer = serviceProvider.GetService<ISecretService>();
            revealer.ShowSecrets();

            var c4 = serviceProvider.GetService<ISoftwareArchitecture>();
            c4.SetWorkspace("DemoApp C4 Model (Structurizr for .NET)", "C4 Architectuur View Model van DemoApp gebruikt tijdens HAN ASD-SKA (20-21)");
            c4.PublishC4Model();
        }
    }
}
