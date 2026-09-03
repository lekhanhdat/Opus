using AvePoint.RA.Contract.Discovery.Model.Enums;
using AvePoint.RA.Contract.Explorer;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Model
{
    public class RMDiscoverySpecificSite : BaseModel
    {
        [Key]
        [Column(TypeName = "bigint", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [Index]
        [Column(TypeName = "nvarchar")]
        [MaxLength(400)]
        public string Url { get; set; }
        [Column(TypeName = "int")]
        public SpecifySiteFlag Type { get; set; }
        [Column(TypeName = "int")]
        public SourceFlag SourceFlag { get; set; }
    }
}
