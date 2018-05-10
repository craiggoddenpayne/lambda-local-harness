using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Test.LambdaHarness;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var host = new LambdaHost(new Function(), new Dictionary<string, string>()))
            {
                host.Wait();
            }
        }

        class Function
        {
            [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
            public async Task<APIGatewayProxyResponse> Handler(APIGatewayProxyRequest request, ILambdaContext context)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = 400,
                    Body = "Test",
                    Headers = new Dictionary<string, string>
                    {
                        { "Test", "Test"}
                    }
                };
            }
        }
    }
}
