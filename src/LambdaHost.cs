using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Test.LambdaHarness
{
    public class LambdaHost : IDisposable
    {
        private readonly IWebHost _handle;

        public LambdaHost(object instance, Dictionary<string, string> environmentVariables)
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


                        /*
                        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
                        public async Task<APIGatewayProxyResponse> Handler(APIGatewayProxyRequest request, ILambdaContext context)
                        */
                        var request = new APIGatewayProxyRequest
                        {
                            Body = body,
                            Path = context.Request.Path.Value,
                            HttpMethod = context.Request.Method,
                            Headers = headers,

                        };
                        var lambdaContext = new ConsoleILambdaContext();
                        var result = instance.GetType().GetMethod("Handler").Invoke(instance, new object[] { request, lambdaContext });
                        var response = ((Task<APIGatewayProxyResponse>)result).Result;
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