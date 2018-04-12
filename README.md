# lambda-local-harness

Wraps up an AWS lambda function, inside an API, to allow local testing.


Supports Lambda functions, that are created to be run with an api gateway
e.g. have the main function, similar to

```
[LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
public async Task<APIGatewayProxyResponse> Handler(APIGatewayProxyRequest request, ILambdaContext context)
```


## Usage

Create a new console project, and install using nuget


Instantiate your lambda function, such as 

```
var function = new Function(
        new SnsPublisher(
                new Settings()),
                new TelemetryClient());
```

Then instantiate the lambda host api harness, passing in any environment variables as a dictionary such as

```
using (var host = new LambdaHost(function, new Dictionary<string, string>
{
        { "Environment", "local"},
        { "SnsTopic", "arn:aws:sns:eu-west-2:00000000:topic"},
        { "AppInsightsInstrumentationKey", "NOT SET"}
}))
{
        host.Wait();
}        
```

When the application starts, the console will advise of the port to call the lambda on, and you should be able to call the lambda, as if it were attached to from api gateway.


### Full example:

```
class Program
{
        static void Main(string[] args)
        {
                var function = new Function(
                        new SnsPublisher(
                                new Settings()),
                                new TelemetryClient());
                        
                using (var host = new LambdaHost(function, new Dictionary<string, string>
                {
                        { "Environment", "local"},
                        { "SnsTopic", "arn:aws:sns:eu-west-2:00000000:marketing-opt-out-messages"},
                        { "AppInsightsInstrumentationKey", "NOT SET"}
                }))
                {
                        host.Wait();
                }
        }
}
```
