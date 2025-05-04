using Common.Utils.Logging;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

ConsoleLogging.RunApp(app);

