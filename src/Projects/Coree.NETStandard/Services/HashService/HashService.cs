using System;
using System.Collections.Generic;
using System.Text;

using Coree.NETStandard.Abstractions.ServiceFactory;
using Microsoft.Extensions.Logging;

namespace Coree.NETStandard.Services.HashService
{
    public partial class HashService : ServiceFactory<HashService>, IHashService
    {
        private readonly ILogger<HashService>? _logger;


        public HashService(ILogger<HashService>? logger = null)
        {
            this._logger = logger;
        }
    }


    public interface IHashService
    {

    }
}
