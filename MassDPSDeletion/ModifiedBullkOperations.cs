// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.Devices.Provisioning.Service;

namespace BazStuff {
    public class ModifiedBulkOperations {
        private readonly ProvisioningServiceClient _provisioningServiceClient;
        private static int s_individualEnrollmentsDeleted;
        
        private readonly List<string> _individualEnrollmentsToBeRetained = new() {
            "cert1"
        };

        // Maximum number of elements per query.
        private const int QueryPageSize = 10;

        public ModifiedBulkOperations(ProvisioningServiceClient provisioningServiceClient) {
            _provisioningServiceClient = provisioningServiceClient;
        }

        public async Task RunSampleAsync() {
            await QueryAndDeleteIndividualEnrollmentsAsync();
        }

        private async Task QueryAndDeleteIndividualEnrollmentsAsync() {
            Console.WriteLine("Creating a query for enrollments...");
            var querySpecification = new QuerySpecification("SELECT * FROM enrollments");
            using Query query = _provisioningServiceClient.CreateIndividualEnrollmentQuery(querySpecification, QueryPageSize);
            while (query.HasNext()) {
                Console.WriteLine("Querying the next enrollments...");
                QueryResult queryResult = await query.NextAsync();
                IEnumerable<object> items = queryResult.Items;
                var individualEnrollments = new List<IndividualEnrollment>();
                foreach (IndividualEnrollment enrollment in items.Cast<IndividualEnrollment>()) {
                    if (!_individualEnrollmentsToBeRetained.Contains(enrollment.RegistrationId, StringComparer.OrdinalIgnoreCase)) {
                        individualEnrollments.Add(enrollment);
                        Console.WriteLine($"Individual enrollment to be deleted: {enrollment.RegistrationId}");
                        s_individualEnrollmentsDeleted++;
                    }
                }
                if (individualEnrollments.Count > 0) {
                    await DeleteBulkIndividualEnrollmentsAsync(individualEnrollments);
                }

                await Task.Delay(1000);
            }
        }

        private async Task DeleteBulkIndividualEnrollmentsAsync(List<IndividualEnrollment> individualEnrollments) {
            Console.WriteLine("Deleting the set of individualEnrollments...");
            BulkEnrollmentOperationResult bulkEnrollmentOperationResult = await _provisioningServiceClient
                .RunBulkEnrollmentOperationAsync(BulkOperationMode.Delete, individualEnrollments);
            Console.WriteLine(bulkEnrollmentOperationResult);
        }

    }
}
