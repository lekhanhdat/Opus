using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Multi_Geo.Enum
{
    [Flags]
    public enum MultiGeoCommonSyncAzure : long
    {
        None = 0,
        ImageEmailTemplate = 1 << 0,
    }
}
