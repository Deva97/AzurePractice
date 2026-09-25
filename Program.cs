using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure;

var builder = WebApplication.CreateBuilder(args);
var keyVaultUrl = builder.Configuration["AZURE_KEY_VAULT_URL"];

if (string.IsNullOrWhiteSpace(keyVaultUrl))
{
	throw new InvalidOperationException("Set AZURE_KEY_VAULT_URL to your Key Vault URL.");
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(options =>
{
	options.Title = "AzurePractice API";
	options.Version = "v1";
});
builder.Services.AddSingleton(new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential()));

var app = builder.Build();

app.UseOpenApi();
app.UseSwaggerUi();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
	.WithName("GetHealth")
	.WithSummary("Check API health");

app.MapGet("/api/secrets/DBserver", async (SecretClient secretClient) =>
{
	try
	{
		var secret = await secretClient.GetSecretAsync("DBserver");
		return Results.Ok(new { name = secret.Value.Name, value = secret.Value.Value });
	}
	catch (RequestFailedException exception)
	{
		return Results.Problem(
			statusCode: exception.Status,
			title: "Unable to retrieve the Key Vault secret.");
	}
})
	.WithName("GetDbServerSecret")
	.WithSummary("Retrieve the DBserver secret from Azure Key Vault");

app.Run();
