using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileHelpers;

namespace AdvertisingPolicyValidationTool
{
    [DelimitedRecord(",")]
    internal class DomainInput
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public DomainOutput CloneOutput(int workerId)
        {
            return new DomainOutput
            {
                Id = Id,
                Worker = $"W-{workerId}",
                Name = Name,
            };
        }
    }
}
