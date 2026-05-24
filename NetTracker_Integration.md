# NetTracker.Core Integration Guide

This guide provides detailed instructions on how to integrate the `NetTracker.Core` library into an ASP.NET Core project, based on its implementation in the ClinicHub project.

## 1. Install the NuGet Package

Add the `NetTracker.Core` package to your web API project.

```xml
<PackageReference Include="NetTracker.Core" Version="2.0.1" />
```

## 2. Register Services

In your `Program.cs` or `Startup.cs`, register the NetTracker services with the dependency injection container. You will need to import the `NET_Tracker.Extensions` namespace.

```csharp
using NET_Tracker.Extensions;

// Add this before builder.Build();
builder.Services.AddNetTracker(builder.Configuration);
```

## 3. Database Initialization

NetTracker requires its own database tables (e.g., `HttpTransactions`) to store tracking data. You should ensure these tables are created when the application starts using Entity Framework Core's relational database creator.

```csharp
var app = builder.Build();

// --- NetTracker Table Creation Workaround ---
using (var scope = app.Services.CreateScope())
{
    var trackerDb = scope.ServiceProvider.GetRequiredService<NET_Tracker.Data.ApplicationDbContext>();
    var dbCreator = trackerDb.Database.GetService<Microsoft.EntityFrameworkCore.Storage.IRelationalDatabaseCreator>();
    try
    {
        await dbCreator.CreateTablesAsync();
        // Log success
    }
    catch (Exception)
    {
        // Ignore exception if the table already exists
    }
}
```

## 4. Add the Middleware

Add the NetTracker middleware to your request pipeline. It is recommended to place it early in the pipeline so it can intercept and track all incoming HTTP requests.

```csharp
// Add this early in the pipeline
app.UseNetTracker(app.Configuration);
```

## 5. API Versioning Compatibility (Optional)

If your project uses API Versioning (e.g., `Asp.Versioning`), you need to mark NetTracker controllers as API version neutral so they can be accessed without providing a specific version parameter.

```csharp
builder.Services.AddApiVersioning(...)
    .AddMvc(options =>
    {
        options.Conventions.Controller(typeof(NET_Tracker.Controllers.TrackerController)).IsApiVersionNeutral();
        options.Conventions.Controller(typeof(NET_Tracker.Controllers.HealthController)).IsApiVersionNeutral();
        options.Conventions.Controller(typeof(NET_Tracker.Controllers.HomeController)).IsApiVersionNeutral();
        options.Conventions.Controller(typeof(NET_Tracker.Controllers.HttpTransactionsController)).IsApiVersionNeutral();
        options.Conventions.Controller(typeof(NET_Tracker.Controllers.StatisticsController)).IsApiVersionNeutral();
    });
```

## 6. Hiding NetTracker from Swagger / OpenAPI (Optional)

To prevent NetTracker endpoints from polluting your Swagger UI or OpenAPI documents, you can hide them using a custom `IControllerModelConvention`.

```csharp
// 1. Add convention to controllers
builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new HideNetTrackerControllersConvention());
});

// 2. Implementation of the convention
public class HideNetTrackerControllersConvention : Microsoft.AspNetCore.Mvc.ApplicationModels.IControllerModelConvention
{
    public void Apply(Microsoft.AspNetCore.Mvc.ApplicationModels.ControllerModel controller)
    {
        if (controller.ControllerType.Assembly.FullName != null && controller.ControllerType.Assembly.FullName.Contains("NetTracker", StringComparison.OrdinalIgnoreCase) ||
            (controller.ControllerType.Namespace != null && controller.ControllerType.Namespace.Contains("Tracker", StringComparison.OrdinalIgnoreCase)))
        {
            controller.ApiExplorer.IsVisible = false;
            foreach (var action in controller.Actions)
            {
                action.ApiExplorer.IsVisible = false;
            }
        }
    }
}
```

## 7. Routing Configuration

Ensure you have a default controller route mapped so that the NetTracker dashboard/UI can be accessed easily (e.g., via the `/Tracker` route).

```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

## 8. Configuration Settings

Make sure to add any required NetTracker configuration values into your `appsettings.json` file. `AddNetTracker` and `UseNetTracker` read from the standard configuration sources (e.g., checking for specific connection strings if NetTracker uses a separate database).
