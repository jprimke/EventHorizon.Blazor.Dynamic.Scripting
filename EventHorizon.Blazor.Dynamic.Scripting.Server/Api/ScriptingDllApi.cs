namespace EventHorizon.Blazor.Dynamic.Scripting.Server.Api
{
    using System;
    using System.IO;
    using CSScriptLib;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/scripting")]
    [ApiController]
    public class ScriptingDllApi : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ScriptingDllApi(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        [Route("dll")]
        public IActionResult Dll()
        {
            var path = Path.Combine(_webHostEnvironment.WebRootPath, "scripts", "Scripts.dll");

            string asm = CSScript.RoslynEvaluator.CompileAssemblyFromCode(
                                                                          @"
                        using System;
                        using System.Collections.Generic;
                        using System.Threading.Tasks;
                        using System.Threading;
                        using MediatR;
                        using EventHorizon.Blazor.Dynamic.Scripting.Model;
                        using EventHorizon.Blazor.Dynamic.Scripting.Events;
                        using EventHorizon.Blazor.Dynamic.Scripting.Services;
                        
                        public class Scripts_Assets_Tree_Create
                        {
                            public async Task<GenericModel> Run(
                                IMediator mediator,
                                IClientInterop interopService, 
                                IDictionary<string, object> data
                            )
                            {
                                interopService.Alert();
                                await mediator.Send(new GenericCommand { Echo = ""Script Command Echo Arg"" });
                                var @event = new GenericEvent { Echo = ""Script Event Echo Arg"" };
                                await mediator.Publish(@event, CancellationToken.None);
                                
                                var queryResults = await mediator.Send(new GenericQuery());
                                foreach(var queryResult in queryResults)
                                {
                                    Console.WriteLine(""From Script: "" + queryResult.Value);
                                }
                                var fromData = data[""arg1""];
                                return new GenericModel { Value = $""From Script: {fromData}"" };
                            }
                        }
                    ",
                                                                          path
                                                                         );

            var bytes = System.IO.File.ReadAllBytes(path);
            var file = Convert.ToBase64String(bytes);

            return Ok(new ScriptingDllResult { DllContent = file, });
        }

        [HttpGet]
        [Route("dll2")]
        public IActionResult Dll2()
        {
            var path = Path.Combine(_webHostEnvironment.WebRootPath, "scripts", "Scripts2.dll");
            string asm = CSScript.RoslynEvaluator.CompileAssemblyFromCode(
                                                                          @"
                        using System;
                        using PSN.TestInterfaces;
                        
                        public class Calculator : ICalc
                        {
                            public int Add(int a, int b)
                            {
                                return a + b;
                            }
                            
                            public int Subtract(int a, int b)
                            {
                                return a - b;
                            }
                            
                            public int Multiply(int a, int b)
                            {
                                return a * b;
                            }
                            
                            public int Divide(int a, int b)
                            {
                                return a / b;
                            }
                        }
                    ",
                                                                          path
                                                                         );

            var bytes = System.IO.File.ReadAllBytes(path);
            var file = Convert.ToBase64String(bytes);

            return Ok(new ScriptingDllResult { DllContent = file, });
        }
    }

    public class ScriptingDllResult
    {
        public string DllContent { get; internal set; }
    }
}
