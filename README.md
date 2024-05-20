# DPS Examples

This repository has some simple .NET sample code for interacting with Azure IoT Hub Device Provisioning Services (DPS).

All are based on the sample code in the .NET SDK at https://github.com/Azure/azure-iot-sdk-csharp.

The following examples are here:

## DPSMetricsDotNet

A sample somewhat inspired by the example at https://github.com/amticianelli/dpsMetrics which is a more built-out
example, but is in Python, and I wanted something in C#.

## MassDPSDeletion

Deletes all registrations except a specific device (hardcoded in the code).  This is primarily intended to
clean up the registrations from MassDPSRegistration.

## MassDPSRegistration

Creates a large number of registrations, all associated with a sample authentication setup.  These are not
really directly usable registrations as created; this exists mainly to allow testing the other code.

## MetricSample

A very simple example to read a metric from DPS, in this case the number of `Device Assignments` over the metric
time period.  The resource path is hard-coded in the code and needs to be changed to work in your environment

