using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Test.LambdaHarness
{
    /// <summary>
    /// Allows an AWS lambda to be hosted locally for local testing
    /// </summary>
    public class LambdaHost : IDisposable
    {
        private readonly IWebHost _handle;


        /// <summary>
        /// Creates a self hosted api, which will call an AWS lambda function
        /// </summary>
        /// <param name="instance">An instance of the AWS lambda</param>
        /// <param name="environmentVariables">A dictionary, containing any environment variables that need to be set</param>
        public LambdaHost(object instance, Dictionary<string, string> environmentVariables)
            : this(instance, environmentVariables, new ConsoleILambdaContext())
        {

        }

        /// <summary>
        /// Creates a self hosted api, which will call an AWS lambda function
        /// </summary>
        /// <param name="instance">An instance of the AWS lambda</param>
        /// <param name="environmentVariables">A dictionary, containing any environment variables that need to be set</param>
        /// <param name="context">A custom lambda context</param>
        public LambdaHost(object instance, Dictionary<string, string> environmentVariables, ILambdaContext lambdaContext)
        {
            foreach (var environmentVariable in environmentVariables)
                Environment.SetEnvironmentVariable(environmentVariable.Key, environmentVariable.Value);

            _handle = WebHost.CreateDefaultBuilder()
                .UseKestrel()
                .Configure(app =>
                {
                    app.Run(context =>
                    {
                        try
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
                            var result = instance.GetType().GetMethod("Handler").Invoke(instance, new object[] { request, lambdaContext });
                            var task = (Task<APIGatewayProxyResponse>)result;
                            var response = task.Result;                            
                            Console.WriteLine(request.Path + "|" + response?.StatusCode + "|" + response?.Body);

                            foreach (var header in response?.Headers ?? new Dictionary<string,string>())
                                context.Response.Headers.TryAdd(header.Key, header.Value);
                            
                            context.Response.StatusCode = response?.StatusCode ?? 0;                            
                            if (response?.Body != null)
                                using (var streamWriter = new StreamWriter(context.Response.Body))
                                    streamWriter.Write(response.Body);

                            return Task.FromResult(response);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            throw;
                        }
                    });
                }).Build();
            _handle.Run();
        }


        /// <summary>
        /// Prevents the application from closing
        /// </summary>
        public void Wait()
        {
            Console.WriteLine("Any key to exit...");
            Console.ReadKey();
        }

        /// <summary>
        /// Disposes the api
        /// </summary>
        public void Dispose()
        {
            _handle.Dispose();
        }
    }
}