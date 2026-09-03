using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Multi_Geo.Model
{
    [DataContract]
    public class SyncCommonAzureInfoDto
    {
        [DataMember]
        public string BlobName { get; set; }
        [DataMember]
        public string SasUrl { get; set; }
    }
}
