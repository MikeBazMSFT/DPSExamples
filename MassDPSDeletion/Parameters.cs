using CommandLine;

namespace ModifiedBulkOperations {
    /// <summary>
    /// Parameters for the application
    /// </summary>
    internal class Parameters {
        [Option(
            'c',
            "ProvisioningConnectionString",
            Required = false,
            HelpText = "The connection string of device provisioning service. Not required when the PROVISIONING_CONNECTION_STRING environment variable is set.")]
        public string? ProvisioningConnectionString { get; set; } = Environment.GetEnvironmentVariable("PROVISIONING_CONNECTION_STRING");
    }
}
