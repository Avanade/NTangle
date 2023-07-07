## Step 3 - Old App Publish

This will walk through the `ContactSync.OldApp.Publisher` project that has been created to leverage Azure Functions. Finally, the publisher functions will be executed to validate.

<br/>

## Project structure

The following project structure will have been generated as part of the solution set up, and the execution of the _NTangle_ code generation. Any sub-folders named `Generated` contain the _NTangle_ generated artefacts; these should not be updated directly as future code generation will overwrite any changes.

```
└── ContactSync.OldApp.Subscriber
  └── Data                       // Components that interact directly with the database
    └── Generated
  └── Entities                   // Entity definition(s)/contract(s) used for publishing
    └── Generated
  └── Functions                  // Azure Function implementations
    └── ContactFunction.cs
    └── RelayFunction.cs
  └── Generated                  // Other ad-hoc component(s)
  └── Services                   // Self-orchestrated entity service(s) used by the Azure Functions
    └── Generated
```

<br/>

## Contact function

The [`ContactFunction`](../ContactSync.OldApp/ContactSync.OldApp.Publisher/Functions/ContactFunction.cs) class is the Azure Function implementation that will invoke the _NTangle_ generated [`ContactService`](../ContactSync.OldApp/ContactSync.OldApp.Publisher/Services/Generated/ContactService.cs) to perform the CDC-related orchestration and publishing.

Luckily, the _NTangle_ solution templating capability provides the `ContactFunction` class implementation as an example, so there is no need to create this class specifially for this sample. However, where specifying a different entity then this class would need to be renamed and the internal references updated accordingly. Additionally, where multiple entities are being published then additional Azure Functions would need to be created, using the same pattern.

To improve the resiliency of the overall publishing solution the [transactional outbox pattern](https://microservices.io/patterns/data/transactional-outbox.html) is being leveraged for the event publishing. The `ContactFunction` behind the scenes, as part of the [startup](../ContactSync.OldApp/ContactSync.OldApp.Publisher/Startup.cs) has set the [`IEventPublisher`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/IEventSender.cs) to [`EventOutboxEnqueue`](../ContactSync.OldApp/ContactSync.OldApp.Publisher/Data/Generated/EventOutboxEnqueue.cs) that will transactionally commit the events to be published with the underlying CDC orchestration. This will ensure that the events are not lost in the event of an application or database failure.

The code is provided for reference. Note that the `TimerTrigger` is set for every five (5) seconds, this should be reviewed and updated to a value that is applicable to the environment and acceptable latency. The corresponding `ContactService` instance is injected into the constructor via dependency injection.

``` csharp
namespace ContactSync.OldApp.Publisher.Functions;

public class ContactFunction
{
    private readonly ContactService _contactService;

    public ContactFunction(ContactService contactService) => _contactService = contactService.ThrowIfNull();

    [FunctionName(nameof(ContactFunction))]
    public Task RunAsync([TimerTrigger("*/5 * * * * *")] TimerInfo timer, CancellationToken cancellationToken) => _contactService.ExecuteAsync(cancellationToken);
}
```

<br/>

## Relay function

The intent of a _message relay_ process is that it will result in at least once publishing semantics; i.e. where there was an error and retry the same events/messages may be sent again. It is the responsibility of the end subscriber to handle multiple events/messages; being the requirement for [duplicate](https://learn.microsoft.com/en-us/azure/service-bus-messaging/duplicate-detection) checking. The [`EventData.Id`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/EventDataBase.cs) by default is unique and should be used for this purpose.

To achieve in-order publishing the _message relay_ process should execute as a [singleton](https://en.wikipedia.org/wiki/Singleton_pattern); i.e. only a single (synchronized) process can execute to guarantee in-order sequencing. Within an event-driven architecture the order in which the events/messages are generated is critical, and as such this order _must_ be maintained (at least from a publishing perspective).

The [`RelayFunction`](../ContactSync.OldApp/ContactSync.OldApp.Publisher/Functions/RelayFunction.cs) class is the Azure Function implementation that will invoke the [`EventOutboxDequeue`](../ContactSync.OldApp/ContactSync.OldApp.Publisher/Data/Generated/EventOutboxDequeue.cs) and publish the events/messages to the configured Azure Service Bus topic (leveraging the configured [`ServiceBusSender`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Azure/ServiceBus/ServiceBusSender.cs).

The code is provided for reference. Note that the `TimerTrigger` is set for every five (5) seconds, this should be reviewed and updated to a value that is applicable to the environment and acceptable latency. The corresponding [`EventOutboxService`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Database.SqlServer/Outbox/EventOutboxService.cs) instance is injected into the constructor via dependency injection.

``` csharp
namespace ContactSync.OldApp.Publisher.Functions;

public class RelayFunction
{
    private readonly EventOutboxService _eventOutboxService;

    public RelayFunction(EventOutboxService eventOutboxService) => _eventOutboxService = eventOutboxService.ThrowIfNull();

    [FunctionName(nameof(RelayFunction))]
    public Task RunAsync([TimerTrigger("*/5 * * * * *")] TimerInfo timer, CancellationToken cancellationToken) => _eventOutboxService.ExecuteAsync(cancellationToken);
}
```

<br/>

## Testing

The Azure Functions can be tested locally using the [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=windows%2Ccsharp%2Cbash#v2). One of the requirements for the Azure Functions is that the [Azure Storage Emulator](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator) is installed and running.

The following steps will walk through the execution of the Azure Functions locally.

- Log into the Azure Portal and create a Service Bus queue (or topic) and copy the secret into the `appsettings.json` file. 
- Start the Azure Functions via Visual Studio.
- Manipulate (add, update and delete) the data in the `OldApp` database.
- Periodically check the Service Bus queue (or topic) to see the events/messages being published.