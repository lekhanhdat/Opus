using AvePoint.RA.Contract.Discovery.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AvePoint.RA.DB.Model.Discovery.Plan
{
    [Table("RMDiscoveryDalJobConfiguration")]
    public class RMDiscoveryDalJobConfiguration : RMDiscoveryDBTable
    {
        [Key]
        [Column(TypeName = "int")]
        public RMDiscoveryConfigurationType ConfigurationType { get; set; }

        [Column(TypeName = "nvarchar")]
        public string ValueJson { get; set; }

        [Column(TypeName = "bigint")]
        public long CreateTime { get; set; }

        [Column(TypeName = "bigint")]
        public long ModifiedTime { get; set; }
    }
}
