// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.Devices.Provisioning.Service;

namespace BazStuff {
    public class ModifiedBulkOperations {
        private readonly ProvisioningServiceClient _provisioningServiceClient;
        private const string SampleTpmEndorsementKey =
            "AToAAQALAAMAsgAgg3GXZ0SEs/gakMyNRqXXJP1S124GUgtk8qHaGzMUaaoABgCAAEMAEAgAAAAAAAEAxsj2gUS" +
            "cTk1UjuioeTlfGYZrrimExB+bScH75adUMRIi2UOMxG1kw4y+9RW/IVoMl4e620VxZad0ARX2gUqVjYO7KPVt3d" +
            "yKhZS3dkcvfBisBhP1XH9B33VqHG9SHnbnQXdBUaCgKAfxome8UmBKfe+naTsE5fkvjb/do3/dD6l4sGBwFCnKR" +
            "dln4XpM03zLpoHFao8zOwt8l/uP3qUIxmCYv9A7m69Ms+5/pCkTu/rK4mRDsfhZ0QLfbzVI6zQFOKF/rwsfBtFe" +
            "WlWtcuJMKlXdD8TXWElTzgh7JS4qhFzreL0c1mI0GCj+Aws0usZh7dLIVPnlgZcBhgy1SSDQMQ==";

        // Maximum number of elements per query.
        private const int QueryPageSize = 10;

        private const int NumberOfBatches = 90000;

        public ModifiedBulkOperations(ProvisioningServiceClient provisioningServiceClient) {
            _provisioningServiceClient = provisioningServiceClient;
        }

        public async Task RunSampleAsync() {
            //await QueryIndividualEnrollmentsAsync().ConfigureAwait(false);

            List<IndividualEnrollment> enrollments = await CreateBulkIndividualEnrollmentsAsync();
            //await GetIndividualEnrollmentInfoAsync(enrollments);
            //await DeleteIndividualEnrollmentsAsync(enrollments);
        }

        public async Task<List<IndividualEnrollment>> CreateBulkIndividualEnrollmentsAsync() {
            var allEnrollments = new List<IndividualEnrollment>(QueryPageSize * NumberOfBatches);
            Attestation attestation = new TpmAttestation(SampleTpmEndorsementKey);
            for (int batchCounter = 1; batchCounter <= NumberOfBatches; batchCounter++) {
                Console.WriteLine($"Working on batch {batchCounter} of {NumberOfBatches}...");
                var individualEnrollments = new List<IndividualEnrollment>(QueryPageSize);
                for (int deviceCounter = 1; deviceCounter <= QueryPageSize; deviceCounter++) {
                    var thisDeviceId = "bulk-" + string.Format("{0:D6}", (batchCounter - 1) * QueryPageSize + deviceCounter - 1);
                    var thisEnrollment = new IndividualEnrollment(thisDeviceId, attestation);
                    //Console.WriteLine($"... {thisDeviceId}");
                    individualEnrollments.Add(thisEnrollment);
                    allEnrollments.Add(thisEnrollment);
                }
                Console.WriteLine("... Running the bulk operation to create the individualEnrollments...\n");
                BulkEnrollmentOperationResult bulkEnrollmentOperationResult =
                    await _provisioningServiceClient.RunBulkEnrollmentOperationAsync(BulkOperationMode.Create, individualEnrollments);
                //Console.WriteLine("... Result of the Create bulk enrollment: ", bulkEnrollmentOperationResult);
            }
            return allEnrollments;
        }

        public async Task GetIndividualEnrollmentInfoAsync(List<IndividualEnrollment> individualEnrollments) {
            foreach (IndividualEnrollment individualEnrollment in individualEnrollments) {
                string registrationId = individualEnrollment.RegistrationId;
                Console.WriteLine($"\nGetting the {nameof(individualEnrollment)} information for {registrationId}...");
                IndividualEnrollment getResult = await _provisioningServiceClient
                    .GetIndividualEnrollmentAsync(registrationId);
                Console.WriteLine(getResult);
            }
        }

        public async Task DeleteIndividualEnrollmentsAsync(List<IndividualEnrollment> individualEnrollments) {
            Console.WriteLine("\nDeleting the set of individualEnrollments...");
            BulkEnrollmentOperationResult bulkEnrollmentOperationResult = await _provisioningServiceClient
                .RunBulkEnrollmentOperationAsync(BulkOperationMode.Delete, individualEnrollments);
            Console.WriteLine(bulkEnrollmentOperationResult);
        }

        public async Task QueryIndividualEnrollmentsAsync() {
            Console.WriteLine("\nCreating a query for enrollments...");
            var querySpecification = new QuerySpecification("SELECT * FROM enrollments");

            using Query query = _provisioningServiceClient.CreateIndividualEnrollmentQuery(querySpecification, QueryPageSize);
            while (query.HasNext()) {
                Console.WriteLine("\nQuerying the next enrollments...");
                QueryResult queryResult = await query.NextAsync();
                Console.WriteLine(queryResult);
            }
        }
    }
}
