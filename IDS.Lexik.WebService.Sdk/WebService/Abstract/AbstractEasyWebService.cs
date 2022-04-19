using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Bcs.IO;
using IDS.Lexik.WebService.Sdk.WaitBehaviour;
using IDS.Lexik.WebService.Sdk.WaitBehaviour.Abstract;
using IDS.Lexik.WebService.Sdk.WebService.Configuration;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Tfres;

namespace IDS.Lexik.WebService.Sdk.WebService.Abstract
{
  public abstract class AbstractEasyWebService<T> : IDisposable where T : EasyWebServiceConfiguration, new()
  {
    protected string _ip;
    protected int _port;
    private Server _server;
    private string _documentation;

    protected AbstractEasyWebService()
    {
    }

    /// <summary>
    /// Starten den WebService
    /// </summary>
    /// <param name="WaitBehaviour">Gibt an, ob und wie der Service warten soll.</param>
    public void Start(AbstractWaitBehaviour WaitBehaviour = null)
    {
      LoadAdditionalConfiguration(LoadConfiguration());
      Console.Write($"Run {GetType().Namespace} on {_ip}:{_port}...");
      LoadData();
      _server = RunServer();
      _server.AddEndpoint(HttpVerb.GET, "/ping", (arg) => arg.Response.Send(HttpStatusCode.OK));
      ConfigureEndpoints(_server);
      Console.WriteLine("ok!");

      PerformTasks();

      if (WaitBehaviour == null)
        WaitBehaviour = new WaitBehaviourLinux();

      WaitBehaviour.Wait();
      _server.Dispose();
    }

    /// <summary>
    /// Virtuelle Methode, um Aufgaben nach dem Start des Webservice auszuführen.
    /// </summary>
    protected virtual void PerformTasks()
    {
    }

    /// <summary>
    /// Nutzen Sie diese Funktion, um die Endpoints zu konfigurieren.
    /// </summary>
    /// <param name="server">Server-Objekt</param>
    /// <example>
    /// server.AddEndpoint(HttpVerb.GET, "/search", RequestSearch);
    /// </example>
    protected abstract void ConfigureEndpoints(Server server);

    private Server RunServer()
    {
      _documentation = AppendDefaultDocumentation(GetDocumentation()).ConvertToJson();
      return new Server(_ip, _port, OpenApiRoute);
    }

    private void OpenApiRoute(HttpContext req) 
      => req.Response.Send(_documentation);

    protected string WebServiceUrlBase { get; set; } = null;

    protected string ProjectName { get; set; } = "OWIDplus";
    protected string ProjectUrl { get; set; } = "https://www.owid.de/plus";
    protected string ProjectVersion { get; set; } = "1.0.0";

    private OpenApiDocument AppendDefaultDocumentation(OpenApiDocument document)
    {
      document.Info = new OpenApiInfo
      {
        License = new OpenApiLicense { Name = "GNU Affero General Public License 3.0", Url = new Uri("https://www.gnu.org/licenses/agpl-3.0.de.html") },
        Contact = new OpenApiContact { Name = "Leibniz-Institut für Deutsche Sprache - Entwickler: Jan Oliver Rüdiger", Url = new Uri(ProjectUrl) },
        TermsOfService = new Uri("https://www.gnu.org/licenses/agpl-3.0.de.html"),
        Title = ProjectName,
        Version = ProjectVersion
      };

      document.Servers = new List<OpenApiServer> {
        new OpenApiServer{ Url = WebServiceUrlBase ?? $"http://{_ip}:{_port}" }
      };

      document.Paths?.Add("/ping",
                          new OpenApiPathItem
                          {
                            Operations = new Dictionary<OperationType, OpenApiOperation>
                            {
                              {
                                OperationType.Get, new OpenApiOperation
                                {
                                  Description = "Liefert den Statuscode 200, wenn der WebServcie verfügbar ist.",
                                  Responses = new OpenApiResponses
                                  {
                                    {"200", new OpenApiResponse {Description = "Service ist verfügbar"}}
                                  }
                                }
                              }
                            }
                          });

      return document;
    }

    /// <summary>
    /// Hier sollte die ergänzende Dokumentation aufgeführt werden. Dokumentiert ist bereits /execute/actions/. /execute/ und weitere/zusätzliche Funktionen müssen hier aufgeführt werden.
    /// </summary>
    /// <returns>Zusätzliche Dokumentation</returns>
    protected abstract OpenApiDocument GetDocumentation();

    protected T LoadConfiguration()
    {
      try
      {
        var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "config.cnf");

        if (!File.Exists(path))
        {
          Console.WriteLine("Please configure this service in 'config.cnf' correctly.");
          FileIO.Write(path, JsonConvert.SerializeObject(new T()));
          throw new Exception();
        }

        var config = JsonConvert.DeserializeObject<T>(FileIO.ReadText(path));
        _ip = config.Ip;
        _port = config.Port;
        return config;
      }
      catch
      {
        _ip = "127.0.0.1";
        _port = 1111;
        return default(T);
      }
    }

    /// <summary>
    /// Lädt spezielle Konfigurationsoptionen.
    /// Die Standardoptionen (siehe EasyWebServiceConfiguration) werden automatisch geladen und konfiguriert.
    /// </summary>
    /// <param name="config">Deserialisierte Einstellungen.</param>
    protected abstract void LoadAdditionalConfiguration(T config);

    /// <summary>
    /// Lade Daten falls notwendig. Wird nach LoadAdditionalConfiguration ausgeführt.
    /// </summary>
    protected abstract void LoadData();

    public void Dispose()
    {
      _server?.Dispose();
    }
  }
}
