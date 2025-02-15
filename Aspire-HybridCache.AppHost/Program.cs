var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var apiService = builder.AddProject<Projects.Aspire_HybridCache_ApiService>("apiservice")
                        .WithReference(cache)
                        .WaitFor(cache);

builder.AddProject<Projects.Aspire_HybridCache_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
