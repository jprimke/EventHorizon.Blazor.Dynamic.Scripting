using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Loader;
using System.Threading.Tasks;
using CSScripting;
using EventHorizon.Blazor.Dynamic.Scripting.Services;
using MediatR;
using Microsoft.AspNetCore.Components;
using PSN.TestInterfaces;

namespace EventHorizon.Blazor.Dynamic.Scripting.Pages;

public class IndexModel : ComponentBase
{
    [Inject]
    public ScriptDllClient Http { get; set; }
    [Inject]
    public IClientInterop ClientInterop { get; set; }
    [Inject]
    public IMediator Mediator { get; set; }
    
    public int result = 0;

    public async Task HandleRunScript()
    {
        var dllResult = await Http.GetContentAsync();
        var bytes = Convert.FromBase64String(
            dllResult.dllContent
        );
        // https://docs.microsoft.com/en-us/dotnet/core/dependency-loading/understanding-assemblyloadcontext
        var assembly = AssemblyLoadContext.Default.LoadFromStream(
            new MemoryStream(
                bytes
            )
        );
        var cssRoot = "css_root"; // Root class name in CSScripting library 
        dynamic script = assembly.CreateObject(
                                           $"*.Scripts_Assets_Tree_Create"
                                          );
        var data = new Dictionary<string, object>
        {
            { "arg1", "Argument from Client" },
        };

        dynamic result = await script.Run(
            Mediator,
            ClientInterop,
            data
        );
        Console.WriteLine(
            $"Script.Run(): {result.Value}"
        );
    }

    public async Task RunLocalScript()
    {
        var dllResult = await Http.GetContent2Async();
        var bytes = Convert.FromBase64String(
                                             dllResult.dllContent
                                            );
        // https://docs.microsoft.com/en-us/dotnet/core/dependency-loading/understanding-assemblyloadcontext
        var assembly = AssemblyLoadContext.Default.LoadFromStream(
                                                                  new MemoryStream(
                                                                                   bytes
                                                                                  )
                                                                 );
        var cssRoot = "css_root"; // Root class name in CSScripting library 
        if (assembly.CreateObject(
                                  $"*.Calculator"
                                 ) is ICalc script)
        {
            result = script.Add(2, 3);
        }
    }
}
