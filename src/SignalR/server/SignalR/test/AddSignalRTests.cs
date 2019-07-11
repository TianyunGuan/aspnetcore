// Copyright(c) .NET Foundation.All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Internal;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.AspNetCore.SignalR.Tests
{
    public class AddSignalRTests
    {
        [Fact]
        public void ServicesAddedBeforeAddSignalRAreUsed()
        {
            var serviceCollection = new ServiceCollection();

            var markerService = new SignalRCoreMarkerService();
            serviceCollection.AddSingleton(markerService);
            serviceCollection.AddSingleton<IUserIdProvider, CustomIdProvider>();
            serviceCollection.AddSingleton(typeof(HubLifetimeManager<>), typeof(CustomHubLifetimeManager<>));
            serviceCollection.AddSingleton<IHubProtocolResolver, CustomHubProtocolResolver>();
            serviceCollection.AddSingleton(typeof(IHubActivator<>), typeof(CustomHubActivator<>));
            serviceCollection.AddSingleton(typeof(IHubContext<>), typeof(CustomHubContext<>));
            serviceCollection.AddSingleton(typeof(IHubContext<,>), typeof(CustomHubContext<,>));
            var hubOptions = new HubOptionsSetup(new List<IHubProtocol>());
            serviceCollection.AddSingleton<IConfigureOptions<HubOptions>>(hubOptions);
            serviceCollection.AddSignalR();

            var serviceProvider = serviceCollection.BuildServiceProvider();
            Assert.IsType<CustomIdProvider>(serviceProvider.GetRequiredService<IUserIdProvider>());
            Assert.IsType<CustomHubLifetimeManager<CustomHub>>(serviceProvider.GetRequiredService<HubLifetimeManager<CustomHub>>());
            Assert.IsType<CustomHubProtocolResolver>(serviceProvider.GetRequiredService<IHubProtocolResolver>());
            Assert.IsType<CustomHubActivator<CustomHub>>(serviceProvider.GetRequiredService<IHubActivator<CustomHub>>());
            Assert.IsType<CustomHubContext<CustomHub>>(serviceProvider.GetRequiredService<IHubContext<CustomHub>>());
            Assert.IsType<CustomHubContext<CustomTHub, string>>(serviceProvider.GetRequiredService<IHubContext<CustomTHub, string>>());
            Assert.IsType<CustomHubContext<CustomDynamicHub>>(serviceProvider.GetRequiredService<IHubContext<CustomDynamicHub>>());
            Assert.Equal(hubOptions, serviceProvider.GetRequiredService<IConfigureOptions<HubOptions>>());
            Assert.Equal(markerService, serviceProvider.GetRequiredService<SignalRCoreMarkerService>());
        }

        [Fact]
        public void ServicesAddedAfterAddSignalRAreUsed()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSignalR();
            serviceCollection.AddSingleton<IUserIdProvider, CustomIdProvider>();
            serviceCollection.AddSingleton(typeof(HubLifetimeManager<>), typeof(CustomHubLifetimeManager<>));
            serviceCollection.AddSingleton<IHubProtocolResolver, CustomHubProtocolResolver>();
            serviceCollection.AddSingleton(typeof(IHubActivator<>), typeof(CustomHubActivator<>));
            serviceCollection.AddSingleton(typeof(IHubContext<>), typeof(CustomHubContext<>));
            serviceCollection.AddSingleton(typeof(IHubContext<,>), typeof(CustomHubContext<,>));

            var serviceProvider = serviceCollection.BuildServiceProvider();
            Assert.IsType<CustomIdProvider>(serviceProvider.GetRequiredService<IUserIdProvider>());
            Assert.IsType<CustomHubLifetimeManager<CustomHub>>(serviceProvider.GetRequiredService<HubLifetimeManager<CustomHub>>());
            Assert.IsType<CustomHubProtocolResolver>(serviceProvider.GetRequiredService<IHubProtocolResolver>());
            Assert.IsType<CustomHubActivator<CustomHub>>(serviceProvider.GetRequiredService<IHubActivator<CustomHub>>());
            Assert.IsType<CustomHubContext<CustomHub>>(serviceProvider.GetRequiredService<IHubContext<CustomHub>>());
            Assert.IsType<CustomHubContext<CustomTHub, string>>(serviceProvider.GetRequiredService<IHubContext<CustomTHub, string>>());
            Assert.IsType<CustomHubContext<CustomDynamicHub>>(serviceProvider.GetRequiredService<IHubContext<CustomDynamicHub>>());
        }

        [Fact]
        public void HubSpecificOptionsDoNotAffectGlobalHubOptions()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSignalR().AddHubOptions<CustomHub>(options =>
            {
                options.SupportedProtocols.Clear();
            });

            var serviceProvider = serviceCollection.BuildServiceProvider();
            Assert.Equal(1, serviceProvider.GetRequiredService<IOptions<HubOptions>>().Value.SupportedProtocols.Count);
            Assert.Equal(0, serviceProvider.GetRequiredService<IOptions<HubOptions<CustomHub>>>().Value.SupportedProtocols.Count);
        }

        [Fact]
        public void StreamBufferCapacityGetSet()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSignalR().AddHubOptions<CustomHub>(options =>
            {
                options.StreamBufferCapacity = 42;
            });

            var serviceProvider = serviceCollection.BuildServiceProvider();
            Assert.Equal(42, serviceProvider.GetRequiredService<IOptions<HubOptions<CustomHub>>>().Value.StreamBufferCapacity);
        }
    }

    public class CustomHub : Hub
    {
    }

    public class CustomTHub : Hub<string>
    {
    }

    public class CustomDynamicHub : DynamicHub
    {
    }

    public class CustomIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            throw new System.NotImplementedException();
        }
    }

    public class CustomHubProtocolResolver : IHubProtocolResolver
    {
        public IReadOnlyList<IHubProtocol> AllProtocols => throw new System.NotImplementedException();

        public IHubProtocol GetProtocol(string protocolName, IReadOnlyList<string> supportedProtocols)
        {
            throw new System.NotImplementedException();
        }
    }

    public class CustomHubActivator<THub> : IHubActivator<THub> where THub : Hub
    {
        public HubHandle<THub> Create(IServiceProvider serviceProvider)
        {
            throw new System.NotImplementedException();
        }

        public void Release(in HubHandle<THub> hub)
        {
            throw new System.NotImplementedException();
        }
    }

    public class CustomHubContext<THub> : IHubContext<THub> where THub : Hub
    {
        public IHubClients Clients => throw new System.NotImplementedException();

        public IGroupManager Groups => throw new System.NotImplementedException();
    }

    public class CustomHubContext<THub, T> : IHubContext<THub, T>
        where THub : Hub<T>
        where T : class
    {
        public IHubClients<T> Clients => throw new System.NotImplementedException();

        public IGroupManager Groups => throw new System.NotImplementedException();
    }

    public class CustomHubLifetimeManager<THub> : HubLifetimeManager<THub> where THub : Hub
    {
        public override Task AddToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public override Task OnConnectedAsync(HubConnectionContext connection)
        {
            throw new System.NotImplementedException();
        }

        public override Task OnDisconnectedAsync(HubConnectionContext connection)
        {
            throw new System.NotImplementedException();
        }

        public override Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public override Task SendAllAsync(string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public override Task SendAllExceptAsync(string methodName, object[] args, IReadOnlyList<string> excludedConnectionIds, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public override Task SendConnectionAsync(string connectionId, string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public override Task SendConnectionsAsync(IReadOnlyList<string> connectionIds, string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public override Task SendGroupAsync(string groupName, string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public override Task SendGroupExceptAsync(string groupName, string methodName, object[] args, IReadOnlyList<string> excludedConnectionIds, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public override Task SendGroupsAsync(IReadOnlyList<string> groupNames, string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public override Task SendUserAsync(string userId, string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public override Task SendUsersAsync(IReadOnlyList<string> userIds, string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}
