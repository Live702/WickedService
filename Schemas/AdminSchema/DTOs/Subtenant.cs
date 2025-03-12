using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AdminSchema;

public partial class Subtenant : TenancyConfig
{
    public Subtenant()
    {
    }

    public Subtenant(string tenancyConfigJson, string id) : base(tenancyConfigJson, id)
    {
    }

    public Subtenant(TenancyConfigPacked packed, string id) : base(packed, id)
    {
    }
}
