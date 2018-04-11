using System;
using System.Collections.Generic;
using System.IO;
using Amazon.Lambda.APIGatewayEvents;
using AuditTrailApi;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace LambdaHarnessApi
{
    public class LambdaHost : IDisposable
    {
        private readonly IWebHost _handle;

        public LambdaHost(Function instance, Dictionary<string, string> environmentVariables)
        {
            foreach (var environmentVariable in environmentVariables)
                Environment.SetEnvironmentVariable(environmentVariable.Key, environmentVariable.Value);
            
            _handle = WebHost.CreateDefaultBuilder()
                .UseKestrel()
                .Configure(app =>
                {
                    app.Run(context =>
                    {
                        if (instance == null)
                            throw new Exception("The instance of the function you passed, doesn't look right");

                        string body;
                        using (var sr = new StreamReader(context.Request.Body))
                        {
                            body = sr.ReadToEnd();
                        }

                        Dictionary<string, string> headers = new Dictionary<string, string>();
                        foreach (var requestHeader in context.Request.Headers)
                        {
                            headers.Add(requestHeader.Key, String.Join(';', requestHeader.Value));
                        }

                        var response = instance.Handler(new APIGatewayProxyRequest
                        {
                            Body = body,
                            Path = context.Request.Path.Value,
                            HttpMethod = context.Request.Method,
                            Headers = headers,

                        }, new ConsoleILambdaContext()).Result;
                        Console.WriteLine(response);
                        return null;
                    });
                }).Build();
            _handle.Run();
        }

        public void Wait()
        {
            Console.WriteLine("Any key to exit...");
            Console.ReadKey();
        }

        public void Dispose()
        {
            _handle.Dispose();
        }
    }
}