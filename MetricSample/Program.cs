// cf. https://github.com/Azure/azure-sdk-for-net/tree/main/sdk/monitor/Azure.Monitor.Query

using Azure;
using Azure.Identity;
using Azure.Monitor.Query;
using Azure.Monitor.Query.Models;

string resourceId =
    "/subscriptions/subid/resourceGroups/IoTDemos/providers/Microsoft.Devices/ProvisioningServices/dps";
var client = new MetricsQueryClient(new DeviceCodeCredential());

Response<MetricsQueryResult> results = await client.QueryResourceAsync(
    resourceId,
    new[] { "DeviceAssignments" }
);

foreach (MetricResult metric in results.Value.Metrics)
{
    Console.WriteLine(metric.Name);
    foreach (MetricTimeSeriesElement element in metric.TimeSeries)
    {
        Console.WriteLine("Dimensions: " + string.Join(",", element.Metadata));

        foreach (MetricValue value in element.Values)
        {
            Console.WriteLine(value);
        }
    }
}
