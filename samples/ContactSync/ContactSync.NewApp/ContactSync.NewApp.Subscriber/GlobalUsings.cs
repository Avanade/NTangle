﻿global using CoreEx;
global using CoreEx.Abstractions;
global using CoreEx.Azure.ServiceBus;
global using CoreEx.Configuration;
global using CoreEx.Database;
global using CoreEx.Database.SqlServer;
global using CoreEx.Entities;
global using CoreEx.Events;
global using CoreEx.Events.Subscribing;
global using CoreEx.Hosting;
global using CoreEx.Json;
global using CoreEx.Mapping;
global using CoreEx.Results;
global using CoreEx.Validation;
global using Microsoft.Data.SqlClient;
global using Microsoft.Azure.Functions.Extensions.DependencyInjection;
global using Microsoft.Azure.WebJobs;
global using Microsoft.Azure.WebJobs.ServiceBus;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text.Json.Serialization;
global using System.Threading;
global using System.Threading.Tasks;
global using ContactSync.NewApp.Subscriber.Subscribers.Entities;
global using Az = Azure.Messaging.ServiceBus;