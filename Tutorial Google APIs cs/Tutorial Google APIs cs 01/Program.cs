//-----------------------------------------------------------------------------
// Tutorial Google APIs cs 01                                       (15/Dic/20)
//
// Instalar los siguientes paquetes de NuGet
/*
    Instalo desde el NuGet para la solución:
    Google.Apis -Version 1.49.0
    Google.Apis.Auth -Version 1.49.0
    Google.Apis.Core -Version 1.49.0
    Google.Apis.People.v1 -Version 1.25.0.830

    Aparte de los que he instalado usando la consola:
    Install-Package Google.Apis.Docs.v1 -Version 1.49.0.2170
    Install-Package Google.Apis.Drive.v3 -Version 1.49.0.2166
*/
// 
//
// (c) Guillermo (elGuille) Som, 2020
//-----------------------------------------------------------------------------

// Genéricas
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
// People API
using Google.Apis.People.v1;
using Google.Apis.People.v1.Data;
// Docs API
using Google.Apis.Docs.v1;
using Google.Apis.Docs.v1.Data;
// Drive API
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data; // Para el tipo File

using System;
using System.Collections.Generic;
//using System.IO;
using System.Threading;
using System.Text;
using System.Linq;

namespace Tutorial_Google_APIs_cs_01
{
    class Program
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/docs.googleapis.com-dotnet-quickstart.json
        static string[] Scopes = { DocsService.Scope.Documents, 
                                   DocsService.Scope.DriveFile, 
                                   PeopleService.Scope.ContactsReadonly };

        static string ApplicationName = "Tutorial Google APIs VB";

        // Los datos del proyecto creado para VB
        static ClientSecrets secrets = new ClientSecrets()
        {
            ClientId = "430211665266-t68vl99t2q40v3lbctgbph23j2644bpj.apps.googleusercontent.com",
            ClientSecret = "Xqexl0FMPedNc1KYs0iJt22A"
        };

        static DocsService docService;
        static DriveService driveService;
        static PeopleService peopleService;

        static UserCredential Credential;

        static void Main(string[] args)
        {
            Console.WriteLine("Tutorial Google APIs con C#");

            string credPath = System.Environment.GetFolderPath(
                Environment.SpecialFolder.Personal);
            
            // Directorio donde se guardarán las credenciales
            credPath = System.IO.Path.Combine(credPath, ".credentials/Tutorial-Google-APIs-VB");

            Credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true)).Result;
            //Console.WriteLine("Credential file saved to: " + credPath);

            // Mostrar los contactos
            MostrarContactos();

            Console.WriteLine();
            Console.WriteLine("Pulsa una tecla.");
            Console.Read();
        }

        private static void MostrarContactos()
        {
            // Create Drive API service.
            peopleService = new PeopleService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = Credential,
                ApplicationName = ApplicationName,
            });

            // Lista de los contactos (People)
            Console.WriteLine("Contactos:");

            // Este muestra todos los contactos
            GetPeople(peopleService, null);

            Console.WriteLine();
            Console.WriteLine($"Hay {total} contactos / People");
            Console.WriteLine();
        }

        private static int total = 0;

        static void GetPeople(PeopleService service, string pageToken)
        {
            // Define parameters of request.
            PeopleResource.ConnectionsResource.ListRequest peopleRequest =
                    service.People.Connections.List("people/me");

            //
            // Lista de campos a usar en RequestMaskIncludeField:
            // https://developers.google.com/people/api/rest/v1/people/get
            //

            peopleRequest.RequestMaskIncludeField = new List<string>()
                    {"person.names", "person.phoneNumbers", "person.emailAddresses",
                      "person.birthdays", "person.Addresses"
                    };


            if (pageToken != null)
            {
                peopleRequest.PageToken = pageToken;
            }

            ListConnectionsResponse people = peopleRequest.Execute();

            if (people != null && people.Connections != null && people.Connections.Count > 0)
            {
                total += people.Connections.Count;
                foreach (var person in people.Connections)
                {
                    Console.Write(person.Names != null ? ($"{person.Names.FirstOrDefault().DisplayName} - ") : "");
                    Console.Write(person.PhoneNumbers != null ? ($"{person.PhoneNumbers.FirstOrDefault().Value} - ") : "");
                    Console.Write(person.EmailAddresses != null ? ($"{person.EmailAddresses.FirstOrDefault().Value} - ") : "");
                    Console.Write(person.Addresses != null ? ($"{person.Addresses.FirstOrDefault()?.City} - ") : "");
                    if (person.Birthdays != null)
                    {
                        var fecha = "";
                        var b = person.Birthdays.FirstOrDefault()?.Date;
                        if (b != null)
                            fecha = $"{b.Day}/{b.Month}/{b.Year}";
                        Console.Write($"{fecha}");
                    }
                    Console.WriteLine();
                }

                if (people.NextPageToken != null)
                {
                    Console.WriteLine();
                    Console.WriteLine($"{total} contactos mostrados hasta ahora. Pulsa una tecla para seguir mostrando contactos.");
                    Console.WriteLine();
                    Console.ReadKey();

                    GetPeople(service, people.NextPageToken);
                }
            }
            else
            {
                Console.WriteLine("No se han encontrado contactos.");
                return;
            }
        }
    }
}
