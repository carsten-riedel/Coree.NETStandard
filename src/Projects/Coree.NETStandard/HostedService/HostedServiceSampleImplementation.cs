﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Threading;
using Coree.NETStandard.Options;

namespace Coree.NETStandard.HostedService
{

    public class HostedServiceSampleImplementationOptions
    {
        public string? Option1 { get; set; }
    }

    public class HostedServiceSampleImplementation : IHostedService
    {
        private readonly ILogger<HostedServiceSampleImplementation> _logger;
        private readonly Guid _guid;
        private readonly HostedServiceSampleImplementationOptions hostedSampleimpOptions;

        public HostedServiceSampleImplementation(IHostedServicesCollectionOptionsProvider<HostedServiceSampleImplementationOptions> optionsList, ILogger<HostedServiceSampleImplementation> logger)
        {
            hostedSampleimpOptions = optionsList.MoveNext();
            _logger = logger;
            _guid = Guid.NewGuid();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Start {_guid} {hostedSampleimpOptions.Option1}");
            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Stop {_guid}");
            await Task.CompletedTask;
        }
    }
}
