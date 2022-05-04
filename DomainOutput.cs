using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileHelpers;

namespace AdvertisingPolicyValidationTool
{
    [DelimitedRecord(",")]
    internal class DomainOutput
    {
        public string Id { get; set; }

        public string Worker { get; set; }

        public string Name { get; set; }

        public bool IsDisalloweded { get; set; }

        public string Words { get; set; }

        public string Error { get; set; }
    }
}
