﻿global using CoreEx;
global using CoreEx.Azure.ServiceBus;
global using CoreEx.Configuration;
global using CoreEx.Database;
global using CoreEx.Database.Mapping;
global using CoreEx.Database.SqlServer;
global using CoreEx.Database.SqlServer.Outbox;
global using CoreEx.Entities;
global using CoreEx.Events;
global using CoreEx.Hosting;
global using CoreEx.Json;
global using CoreEx.Mapping;
global using Microsoft.Data.SqlClient;
#if (implement_publisher_function)
global using Microsoft.Azure.Functions.Worker;
#endif
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using NTangle;
global using NTangle.Cdc;
global using NTangle.Data;
global using NTangle.Data.SqlServer;
global using NTangle.Events;
global using NTangle.Services;
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text.Json.Serialization;
global using System.Threading;
global using System.Threading.Tasks;
global using AppName.Publisher.Data;
global using AppName.Publisher.Entities;
global using AppName.Publisher.Services;
global using Az = Azure.Messaging.ServiceBus;