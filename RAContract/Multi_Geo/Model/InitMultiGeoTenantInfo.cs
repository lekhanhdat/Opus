using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.RMWeb.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Multi_Geo.Model
{
    [DataContract]
    public class InitMultiGeoTenantInfo
    {
        [DataMember]
        public string RegisterEmail { get; set; }
        [DataMember]
        public string JPMCMultiGeoDC { get; set; }
        [DataMember]
        public string JPMCMultiGeoMainDC { get; set; }
        [DataMember]
        public string HasUpgradeTeams { get; set; }
        [DataMember]
        public string EnableTeamsFeature { get; set; }

        [DataMember]
        public List<AccountDto> AdminAccountInfo { get; set; }
        [DataMember]
        public string EnableFolderPath { get; set; }
    }
}
