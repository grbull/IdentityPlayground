// discover endpoints from metadata

using System.Text.Json;
using IdentityModel.Client;

var handler = new HttpClientHandler();
handler.ClientCertificateOptions = ClientCertificateOption.Manual;
handler.ServerCertificateCustomValidationCallback = 
    (httpRequestMessage, cert, cetChain, policyErrors) =>
    {
        return true;
    };

var client = new HttpClient(handler);
var disco = await client.GetDiscoveryDocumentAsync("http://localhost:5000");
if (disco.IsError)
{
    Console.WriteLine(disco.Error);
    return;
}

// request token
var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
{
    Address = disco.TokenEndpoint,

    ClientId = "client",
    ClientSecret = "secret",
    Scope = "api"
});

if (tokenResponse.IsError)
{
    Console.WriteLine(tokenResponse.Error);
    return;
}

Console.WriteLine(tokenResponse.AccessToken);

// call api
var apiHandler = new HttpClientHandler();
apiHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
apiHandler.ServerCertificateCustomValidationCallback = 
    (httpRequestMessage, cert, cetChain, policyErrors) =>
    {
        return true;
    };
var apiClient = new HttpClient(apiHandler);
apiClient.SetBearerToken(tokenResponse.AccessToken);

var response = await apiClient.GetAsync("https://localhost:6001/WeatherForecast/identity");
if (!response.IsSuccessStatusCode)
{
    Console.WriteLine(response.StatusCode);
}
else
{
    var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
    Console.WriteLine(JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true }));
}