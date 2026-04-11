using NoteApp.Services;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Vault file path
string vaultConnStrPath = "/vault/secrets/mongodb__connectionstring";


string connectionString = File.Exists(vaultConnStrPath)
    ? File.ReadAllText(vaultConnStrPath).Trim()
    : throw new FileNotFoundException($"❌ ERROR: Vault secret file not found at {vaultConnStrPath}");

var client = new MongoClient(connectionString);
var database = client.GetDatabase("NoteDb");

builder.Services.AddSingleton<IMongoDatabase>(database);
builder.Services.AddSingleton<NoteService>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Notes}/{action=Index}/{id?}");

Console.WriteLine("✅ NoteApp running on http://0.0.0.0:5050");

app.Run("http://0.0.0.0:5050");

