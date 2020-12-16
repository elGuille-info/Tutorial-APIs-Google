'------------------------------------------------------------------------------
' Tutorial Google APIs cs 01                                        (15/Dic/20)
'
' Instalar los siguientes paquetes de NuGet
'
'    Instalo desde el NuGet para la solución
'    Google.Apis -Version 1.49.0
'    Google.Apis.Auth -Version 1.49.0
'    Google.Apis.Core -Version 1.49.0
'    Google.Apis.People.v1 -Version 1.25.0.830
'
'    Aparte de los que he instalado usando la consola
'    Install-Package Google.Apis.Docs.v1 -Version 1.49.0.2170
'    Install-Package Google.Apis.Drive.v3 -Version 1.49.0.2166
'
'
' (c) Guillermo (elGuille) Som, 2020
'------------------------------------------------------------------------------
Option Strict On
Option Infer On

'// Genéricas
Imports Google.Apis.Auth.OAuth2
Imports Google.Apis.Services
Imports Google.Apis.Util.Store
'// People API
Imports Google.Apis.People.v1
Imports Google.Apis.People.v1.Data
'// Docs API
Imports Google.Apis.Docs.v1
Imports Google.Apis.Docs.v1.Data
'// Drive API
Imports Google.Apis.Drive.v3
Imports Google.Apis.Drive.v3.Data '// Para el tipo File

Imports System
Imports System.Collections.Generic
Imports System.Threading
Imports System.Text
Imports System.Linq


Class Program

    Shared Scopes As String() =
        {
            DocsService.Scope.Documents,
            DocsService.Scope.DriveFile,
            PeopleService.Scope.ContactsReadonly
        }

    Shared ApplicationName As String = "Tutorial Google APIs VB"

    Shared secrets As ClientSecrets = New ClientSecrets() With
        {
            .ClientId = "430211665266-t68vl99t2q40v3lbctgbph23j2644bpj.apps.googleusercontent.com",
            .ClientSecret = "Xqexl0FMPedNc1KYs0iJt22A"
        }

    Shared docService As DocsService
    Shared driveService As DriveService
    Shared peopleService As PeopleService

    Shared Credential As UserCredential

    Public Shared Sub Main(args As String())

        Console.WriteLine("Tutorial Google APIs con Visual Basic")

        Dim credPath As String = System.Environment.GetFolderPath(
            Environment.SpecialFolder.Personal)

        '// Directorio donde se guardarán las credenciales
        credPath = System.IO.Path.Combine(credPath, ".credentials/Tutorial-Google-APIs-VB")

        Credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
            secrets,
            Scopes,
            "user",
            CancellationToken.None,
            New FileDataStore(credPath, True)).Result

        '// Mostrar los contactos
        MostrarContactos()

        Console.WriteLine()
        Console.WriteLine("Pulsa una tecla.")
        Console.Read()
    End Sub

    Private Shared Sub MostrarContactos()
        peopleService = New PeopleService(New BaseClientService.Initializer() With
        {
            .HttpClientInitializer = Credential,
            .ApplicationName = ApplicationName
        })

        Console.WriteLine("Contactos:")

        GetPeople(peopleService, Nothing)

        Console.WriteLine()
        Console.WriteLine($"Hay {total} contactos / People")
        Console.WriteLine()
    End Sub

    Private Shared total As Integer = 0

    Private Shared Sub GetPeople(service As PeopleService, pageToken As String)

        Dim peopleRequest As PeopleResource.ConnectionsResource.ListRequest =
            service.People.Connections.List("people/me")

        peopleRequest.RequestMaskIncludeField = New List(Of String)() From {
                "person.names",
                "person.phoneNumbers",
                "person.emailAddresses",
                "person.birthdays",
                "person.Addresses"
            }

        If pageToken IsNot Nothing Then
            peopleRequest.PageToken = pageToken
        End If

        Dim people As ListConnectionsResponse = peopleRequest.Execute()

        If people IsNot Nothing AndAlso
                people.Connections IsNot Nothing AndAlso
                people.Connections.Count > 0 Then
            total += people.Connections.Count

            For Each person In people.Connections
                Console.Write(If(person.Names IsNot Nothing,
                              ($"{person.Names.FirstOrDefault().DisplayName} - "), ""))
                Console.Write(If(person.PhoneNumbers IsNot Nothing,
                              ($"{person.PhoneNumbers.FirstOrDefault().Value} - "), ""))
                Console.Write(If(person.EmailAddresses IsNot Nothing,
                              ($"{person.EmailAddresses.FirstOrDefault().Value} - "), ""))
                Console.Write(If(person.Addresses IsNot Nothing,
                              ($"{person.Addresses.FirstOrDefault()?.City} - "), ""))

                If person.Birthdays IsNot Nothing Then
                    Dim fecha = ""
                    Dim b = person.Birthdays.FirstOrDefault()?.Date
                    If b IsNot Nothing Then fecha = $"{b.Day}/{b.Month}/{b.Year}"
                    Console.Write($"{fecha}")
                End If

                Console.WriteLine()
            Next

            If people.NextPageToken IsNot Nothing Then
                Console.WriteLine()
                Console.WriteLine($"{total} contactos mostrados hasta ahora. " &
                                  "Pulsa una tecla para seguir mostrando contactos.")
                Console.WriteLine()
                Console.ReadKey()
                GetPeople(service, people.NextPageToken)
            End If
        Else
            Console.WriteLine("No se han encontrado contactos.")
            Return
        End If
    End Sub

End Class

