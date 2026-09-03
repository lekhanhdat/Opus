using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Model
{
    public class RMFSMyHubDashboard : BaseModel
    {
        [Key]
        [Column(TypeName = "uniqueidentifier", Order = 1)]
        public Guid NodeId { set; get; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid GroupId { set; get; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid ScopeId { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string MetaData { get; set; }
        [Column(TypeName = "nvarchar")]
        public string FullPath { get; set; }
    }
}
