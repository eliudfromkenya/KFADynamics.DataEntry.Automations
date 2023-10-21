using System;
using Microsoft.Extensions.Configuration;

namespace KFADynamics.DataEntry.Automations;

public class SecretAppsettingReader
{
  public static string ReadConfig(string sectionName)
  {
    //var environment = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
    //var builder = new ConfigurationBuilder()
    //    .AddJsonFile("appsettings.json")
    //    .AddJsonFile($"appsettings.{environment}.json", optional: true);
    ////.AddEnvironmentVariables();
    //var configurationRoot = builder.Build();

    var config = new ConfigurationBuilder()
    .AddUserSecrets(typeof(Program).Assembly)
    .Build();

    return config.GetSection(sectionName)?.Value;
   

    //return configurationRoot.GetSection(sectionName)?.Value;
  }
}
