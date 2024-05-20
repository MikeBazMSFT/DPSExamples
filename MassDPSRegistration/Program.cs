// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;
using Microsoft.Azure.Devices.Provisioning.Service;
using ModifiedBulkOperations;

namespace BazStuff {
    internal class Program {
        public static int Main(string[] args) {
            // Parse application parameters
            Parameters parameters = new();
            ParserResult<Parameters> result = Parser.Default.ParseArguments<Parameters>(args)
               .WithParsed(parsedParams => {
                   parameters = parsedParams;
               })
               .WithNotParsed(errors => {
                   Environment.Exit(1);
               });

            if (string.IsNullOrWhiteSpace(parameters.ProvisioningConnectionString)) {
                Console.WriteLine(CommandLine.Text.HelpText.AutoBuild(result, null, null));
                Environment.Exit(1);
            }

            using var provisioningServiceClient = ProvisioningServiceClient.CreateFromConnectionString(parameters.ProvisioningConnectionString);
            var sample = new ModifiedBulkOperations(provisioningServiceClient);
            sample.RunSampleAsync().GetAwaiter().GetResult();

            Console.WriteLine("Done.\n");
            return 0;
        }
    }
}
