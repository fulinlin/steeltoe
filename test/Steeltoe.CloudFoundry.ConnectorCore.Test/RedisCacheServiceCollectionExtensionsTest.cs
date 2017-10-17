﻿// Copyright 2015 the original author or authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Steeltoe.Extensions.Configuration;
using Xunit;

namespace Steeltoe.CloudFoundry.Connector.Redis.Test
{
    public class RedisCacheServiceCollectionExtensionsTest
    {
        public RedisCacheServiceCollectionExtensionsTest()
        {
            Environment.SetEnvironmentVariable("VCAP_APPLICATION", null);
            Environment.SetEnvironmentVariable("VCAP_SERVICES", null);
        }

        [Fact]
        public void AddDistributedRedisCache_ThrowsIfServiceCollectionNull()
        {
            // Arrange
            IServiceCollection services = null;
            IConfigurationRoot config = new ConfigurationBuilder().Build();

            // Act and Assert
            var ex = Assert.Throws<ArgumentNullException>(() => RedisCacheServiceCollectionExtensions.AddDistributedRedisCache(services, config));
            Assert.Contains(nameof(services), ex.Message);

            var ex2 = Assert.Throws<ArgumentNullException>(() => RedisCacheServiceCollectionExtensions.AddDistributedRedisCache(services, config, "foobar"));
            Assert.Contains(nameof(services), ex2.Message);

            var ex3 = Assert.Throws<ArgumentNullException>(() => RedisCacheServiceCollectionExtensions.AddDistributedRedisCache(services, config, config, "foobar"));
            Assert.Contains(nameof(services), ex3.Message);
        }

        [Fact]
        public void AddDistributedRedisCache_ThrowsIfConfigurationNull()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            IConfigurationRoot config = null;
            IConfigurationRoot connectionConfig = new ConfigurationBuilder().Build();

            // Act and Assert
            var ex = Assert.Throws<ArgumentNullException>(() => RedisCacheServiceCollectionExtensions.AddDistributedRedisCache(services, config));
            Assert.Contains(nameof(config), ex.Message);

            var ex2 = Assert.Throws<ArgumentNullException>(() => RedisCacheServiceCollectionExtensions.AddDistributedRedisCache(services, config, "foobar"));
            Assert.Contains(nameof(config), ex2.Message);

            var ex3 = Assert.Throws<ArgumentNullException>(() => RedisCacheServiceCollectionExtensions.AddDistributedRedisCache(services, config, connectionConfig, "foobar"));
            Assert.Contains("applicationConfiguration", ex3.Message);
        }

        [Fact]
        public void AddDistributedRedisCache_ThrowsIfServiceNameNull()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            IConfigurationRoot config = new ConfigurationBuilder().Build();
            string serviceName = null;

            // Act and Assert
            var ex = Assert.Throws<ArgumentNullException>(() => RedisCacheServiceCollectionExtensions.AddDistributedRedisCache(services, config, serviceName));
            Assert.Contains(nameof(serviceName), ex.Message);
        }

        [Fact]
        public void AddDistributedRedisCache_NoVCAPs_AddsDistributedCache()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            IConfigurationRoot config = new ConfigurationBuilder().Build();

            // Act
            RedisCacheServiceCollectionExtensions.AddDistributedRedisCache(services, config);
            var service = services.BuildServiceProvider().GetService<IDistributedCache>();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<RedisCache>(service);
        }

        [Fact]
        public void AddDistributedRedisCache_WithServiceName_NoVCAPs_ThrowsConnectorException()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            IConfigurationRoot config = new ConfigurationBuilder().Build();

            // Act and Assert
            var ex = Assert.Throws<ConnectorException>(() => RedisCacheServiceCollectionExtensions.AddDistributedRedisCache(services, config, "foobar"));
            Assert.Contains("foobar", ex.Message);

            var ex2 = Assert.Throws<ConnectorException>(() => RedisCacheServiceCollectionExtensions.AddDistributedRedisCache(services, config, config, "foobar"));
            Assert.Contains("foobar", ex2.Message);
        }

        [Fact]
        public void AddDistributedRedisCache_MultipleRedisServices_ThrowsConnectorException()
        {
            // Arrange
            var env1 = @"
{
      'limits': {
        'fds': 16384,
        'mem': 1024,
        'disk': 1024
      },
      'application_name': 'spring-cloud-broker',
      'application_uris': [
        'spring-cloud-broker.apps.testcloud.com'
      ],
      'name': 'spring-cloud-broker',
      'space_name': 'p-spring-cloud-services',
      'space_id': '65b73473-94cc-4640-b462-7ad52838b4ae',
      'uris': [
        'spring-cloud-broker.apps.testcloud.com'
      ],
      'users': null,
      'version': '07e112f7-2f71-4f5a-8a34-db51dbed30a3',
      'application_version': '07e112f7-2f71-4f5a-8a34-db51dbed30a3',
      'application_id': '798c2495-fe75-49b1-88da-b81197f2bf06'
    }
}";
            var env2 = @"
{
      'p-redis': [
        {
            'credentials': {
                'host': '192.168.0.103',
                'password': '133de7c8-9f3a-4df1-8a10-676ba7ddaa10',
                'port': 60287
            },
          'syslog_drain_url': null,
          'label': 'p-redis',
          'provider': null,
          'plan': 'shared-vm',
          'name': 'myRedisService1',
          'tags': [
            'pivotal',
            'redis'
          ]
        }, 
        {
            'credentials': {
                'host': '192.168.0.103',
                'password': '133de7c8-9f3a-4df1-8a10-676ba7ddaa10',
                'port': 60287
            },
          'syslog_drain_url': null,
          'label': 'p-redis',
          'provider': null,
          'plan': 'shared-vm',
          'name': 'myRedisService2',
          'tags': [
            'pivotal',
            'redis'
          ]
        } 
      ]
}
";

            // Arrange
            IServiceCollection services = new ServiceCollection();

            Environment.SetEnvironmentVariable("VCAP_APPLICATION", env1);
            Environment.SetEnvironmentVariable("VCAP_SERVICES", env2);

            ConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddCloudFoundry();
            var config = builder.Build();

            // Act and Assert
            var ex = Assert.Throws<ConnectorException>(() => RedisCacheServiceCollectionExtensions.AddDistributedRedisCache(services, config));
            Assert.Contains("Multiple", ex.Message);

            var ex2 = Assert.Throws<ConnectorException>(() => RedisCacheServiceCollectionExtensions.AddDistributedRedisCache(services, config, config, null));
            Assert.Contains("Multiple", ex2.Message);
        }

        [Fact]
        public void AddRedisConnectionMultiplexer_ThrowsIfServiceCollectionNull()
        {
            // Arrange
            IServiceCollection services = null;
            IConfigurationRoot config = new ConfigurationBuilder().Build();

            // Act and Assert
            var ex = Assert.Throws<ArgumentNullException>(() => RedisCacheServiceCollectionExtensions.AddRedisConnectionMultiplexer(services, config));
            Assert.Contains(nameof(services), ex.Message);

            var ex2 = Assert.Throws<ArgumentNullException>(() => RedisCacheServiceCollectionExtensions.AddRedisConnectionMultiplexer(services, config, "foobar"));
            Assert.Contains(nameof(services), ex2.Message);

            var ex3 = Assert.Throws<ArgumentNullException>(() => RedisCacheServiceCollectionExtensions.AddRedisConnectionMultiplexer(services, config, config, "foobar"));
            Assert.Contains(nameof(services), ex3.Message);
        }

        [Fact]
        public void AddRedisConnectionMultiplexer_ThrowsIfConfigurationNull()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            IConfigurationRoot config = null;
            IConfigurationRoot connectionConfig = new ConfigurationBuilder().Build();

            // Act and Assert
            var ex = Assert.Throws<ArgumentNullException>(() => RedisCacheServiceCollectionExtensions.AddRedisConnectionMultiplexer(services, config));
            Assert.Contains(nameof(config), ex.Message);

            var ex2 = Assert.Throws<ArgumentNullException>(() => RedisCacheServiceCollectionExtensions.AddRedisConnectionMultiplexer(services, config, "foobar"));
            Assert.Contains(nameof(config), ex2.Message);

            var ex3 = Assert.Throws<ArgumentNullException>(() => RedisCacheServiceCollectionExtensions.AddRedisConnectionMultiplexer(services, config, connectionConfig, "foobar"));
            Assert.Contains("applicationConfiguration", ex3.Message);
        }

        [Fact]
        public void AddRedisConnectionMultiplexer_ThrowsIfServiceNameNull()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            IConfigurationRoot config = null;
            string serviceName = null;

            // Act and Assert
            var ex = Assert.Throws<ArgumentNullException>(() => RedisCacheServiceCollectionExtensions.AddRedisConnectionMultiplexer(services, config, serviceName));
            Assert.Contains(nameof(serviceName), ex.Message);
        }

        [Fact]
        public void AddRedisConnectionMultiplexer_NoVCAPs_AddsConnectionMultiplexer()
        {
            // Arrange
            var appsettings = new Dictionary<string, string>()
            {
                ["redis:client:host"] = "127.0.0.1",
                ["redis:client:port"] = "1234",
                ["redis:client:password"] = "password",
                ["redis:client:abortOnConnectFail"] = "false"
            };

            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(appsettings);
            var config = configurationBuilder.Build();

            IServiceCollection services = new ServiceCollection();
            IServiceCollection services2 = new ServiceCollection();

            // Act
            RedisCacheServiceCollectionExtensions.AddRedisConnectionMultiplexer(services, config);
            var service = services.BuildServiceProvider().GetService<IConnectionMultiplexer>();

            RedisCacheServiceCollectionExtensions.AddRedisConnectionMultiplexer(services2, config, config, null);
            var service2 = services2.BuildServiceProvider().GetService<IConnectionMultiplexer>();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<ConnectionMultiplexer>(service);
            Assert.NotNull(service2);
            Assert.IsType<ConnectionMultiplexer>(service2);
        }

        [Fact]
        public void AddRedisConnectionMultiplexer_WithServiceName_NoVCAPs_ThrowsConnectorException()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            IConfigurationRoot config = new ConfigurationBuilder().Build();

            // Act and Assert
            var ex = Assert.Throws<ConnectorException>(() => RedisCacheServiceCollectionExtensions.AddRedisConnectionMultiplexer(services, config, "foobar"));
            Assert.Contains("foobar", ex.Message);

            var ex2 = Assert.Throws<ConnectorException>(() => RedisCacheServiceCollectionExtensions.AddRedisConnectionMultiplexer(services, config, config, "foobar"));
            Assert.Contains("foobar", ex2.Message);
        }
    }
}
