using Microsoft.Azure.Devices.Provisioning.Service;

namespace BazStuff {
    public class MetricCalculator(ProvisioningServiceClient provisioningServiceClient) {
        private readonly ProvisioningServiceClient _provisioningServiceClient = provisioningServiceClient;

        // Maximum number of elements per query.
        private const int QueryPageSize = 10000;

        public async Task<(int, int)> CalculateTotal() {
            int totalCount = 0;
            int activeCount = 0;

            Console.WriteLine("Calculating group totals...");
            var (groupTotalCount, groupActiveCount) = await CalculateGroupTotal();
            totalCount += groupTotalCount;
            activeCount += groupActiveCount;

            Console.WriteLine("Calculating indvidual totals...");
            var (individualTotalCount, IndividualActiveCount) = await CalculateIndividualTotal();
            totalCount += individualTotalCount;
            activeCount += IndividualActiveCount;

            return (totalCount, activeCount);
        }

        public async Task<(int, int)> CalculateGroupTotal() {
            int totalCount = 0;
            int activeCount = 0;
            using (var groupQuery = _provisioningServiceClient.CreateEnrollmentGroupQuery(new QuerySpecification("SELECT * FROM EnrollmentGroups"), pageSize: QueryPageSize)) {
                while (groupQuery.HasNext()) {
                    Console.Write(".");
                    var currentGroupResults = await groupQuery.NextAsync();
                    foreach (EnrollmentGroup groupItem in currentGroupResults.Items.Cast<EnrollmentGroup>()) {
                        using (var deviceQuery = _provisioningServiceClient.CreateEnrollmentGroupRegistrationStateQuery(
                            new QuerySpecification("SELECT registrationId,assignedHub,status FROM deviceregistration"),
                            pageSize: QueryPageSize,
                            enrollmentGroupId: groupItem.EnrollmentGroupId
                        )) {
                            var currentRegistrationResults = await deviceQuery.NextAsync();
                            foreach (DeviceRegistrationState registration in currentRegistrationResults.Items.Cast<DeviceRegistrationState>()) {
                                totalCount++;
                                if (registration.Status == EnrollmentStatus.Assigned) {
                                    activeCount++;
                                }
                            }
                        }
                    }
                }
                Console.WriteLine();
            }
            return (totalCount, activeCount);
        }

        public async Task<(int, int)> CalculateIndividualTotal() {
            int totalCount = 0;
            int activeCount = 0;
            using (var individualQuery = _provisioningServiceClient.CreateIndividualEnrollmentQuery(new QuerySpecification("SELECT * FROM Enrollments"), pageSize: QueryPageSize)) {
                int itemCounter = 0;
                while (individualQuery.HasNext()) {
                    Console.Write(".");
                    var currentIndividualResults = await individualQuery.NextAsync();
                    foreach (IndividualEnrollment individualEnrollment in currentIndividualResults.Items.Cast<IndividualEnrollment>()) {
                        totalCount++;
                        // if (individualEnrollment.RegistrationId == "cert1") {
                        //     System.Diagnostics.Debugger.Launch();
                        // }
                        try {
                            var registrationState = await _provisioningServiceClient.GetDeviceRegistrationStateAsync(individualEnrollment.RegistrationId);
                            if (registrationState.Status == EnrollmentStatus.Assigned) {
                                activeCount++;
                            }
                        } catch (ProvisioningServiceClientHttpException) {
                            // an unused registration will throw a 404
                        }
                    }
                    itemCounter++;
                    if (itemCounter == 80) {
                        Console.WriteLine(totalCount);
                        itemCounter = 0;
                    }
                }
                Console.WriteLine();
            }
            return (totalCount, activeCount);
        }
    }
}
