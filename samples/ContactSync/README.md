# Contact Sync

The purpose of this sample is to demonstrate the usage of _NTangle_ in a typical real world scenario.

<br/>

## Requirements

Our fictitious company has an existing legacy application, named `OldApp`, that contains contact information that is required to be synchronized to a new application, named `NewApp`, as part of a holistic app modernization initiative. The `OldApp` is a monolithic application that is not easily modified. The `NewApp` is a modern application that is going to be built as cloud-native, leveraging the likes of domains and microservices, etc. 

Whilst going through the digital decoupling journey, the `OldApp` will continue to be the source of truth for the contact information in the short term. However, the `NewApp` needs to have this information available within it, near real-time, to provide a better user experience for the consumers of the `NewApp`. This new application will in time replace the old.

In reviewing the requirements, traditional database synchronization capabilties such as Debezium and alike have been dismissed. The primary reasoning is the desire to produce business-based domain-oriented events that will be consumed by the `NewApp`, but are equally available for consumption by the underlying data and analytics platform, in a completely decoupled manner. It is believed that _NTangle_ can achieve these outcomes more effectively; time and cost.

<br/>

## Architecture

To support the requirements, the following event-driven architecture (EDA) is proposed, and will be demonstrated within this sample.

!!diagram-to-be-added!!

The old and new applications will be completely decoupled from each other, and as such will not be aware of each other (i.e. there are no solution dependencies). The only commonality between the two applications will be the underlying event stream (Azure Service Bus queue/topic) with a standardized message contract.

<br/>

### Old App

The following will be introduced within the `OldApp`.

- _NTangle_ will be used to introduce change-data-capture (CDC) to the underlying SQL Server database.
- Azure Function (timer-based trigger) will be used to host the _NTangle_ CDC publishing:
  - Leverage a transactional outbox wihtin the `OldApp` database for the interim storage of events to improve resilency; 
  - Publish the events to an Azure Service Bus queue/topic.

<br>

### New App

The foundations of the `NewApp` will be introduced.

- New SQL Server database with updated schema leveraging [_DbEx_](https://github.com/Avanade/dbex) to enable end-to-end management.
- Azure Function (Azure Service Bus trigger) that subscribes to the events and updates the new database accordingly.

<br/>

## Implementation steps

This sample will walk through the implementation in a numner of logical steps.

<br/>

### Old App

1. _NTangle_ - solution setup, configuration and code-generation.
2. Database - apply CDC and schema changes.
3. Publish - publish events to Azure Service Bus leveraging Azure Functions.

<br/>

### New App

4. Database - create new database and schema.
5. Subscribe -  subscribe to events from Azure Service Bus leveraging Azure Function.

<br/>

## Conclusion

The basis of the functional capabilities have been created for our fictitious solution. In the end, the developer should have a reasonable understanding of how to build a data syncrhonization solution leveraging _NTangle_ (and [_CoreEx_](https://github.com/Avanade/CoreEx)) capabilities.
