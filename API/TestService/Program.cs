
var builder = WebApplication.CreateBuilder(args);

if (builder.Configuration["Vault:Role"] != null)
{
    builder.Configuration.AddVault(options =>
      {
          var vaultOptions = builder.Configuration.GetSection("Vault");
          options.Address = vaultOptions["Address"];
          options.Role = vaultOptions["VAULT_ROLE_ID"];
          options.SecretPath = vaultOptions["SecretPath"];
          options.Secret = vaultOptions["VAULT_SECRET_ID"];
      });
}

var app = builder.Build();


app.Run();
