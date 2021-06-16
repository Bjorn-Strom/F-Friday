module Server

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe

open HttpHandlers

let routes =
    choose [ GET    >=> route  "/recipes"    >=> getRecipes 
             POST   >=> route  "/recipe"     >=> postRecipe
             PUT    >=> route  "/recipe"     >=> putRecipe
             DELETE >=> routef "/recipe/%O" deleteRecipe
             RequestErrors.NOT_FOUND "Not found"
           ]

let configureApp (app : IApplicationBuilder) =
    app.UseGiraffe routes

let configureServices (services : IServiceCollection) =
    services.AddGiraffe() |> ignore

[<EntryPoint>]
let main args =
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .Configure(Action<IApplicationBuilder> configureApp)
                    .ConfigureServices(configureServices)
                    |> ignore)
        .Build()
        .Run()
    0