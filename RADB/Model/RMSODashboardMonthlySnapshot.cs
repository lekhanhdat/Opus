using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvePoint.RA.DB.Model
{
    public class RMSODashboardMonthlySnapshot : BaseModel
    {
        [Key]
        [Column(TypeName = "nvarchar")]
        [MaxLength(64)]
        [Required]
        public string Id { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(6)]
        [Required]
        [Index]
        public string Period { get; set; }          // yyyyMM

        [Column(TypeName = "nvarchar")]
        [MaxLength(64)]
        [Required]
        [Index]
        public string O365TenantId { get; set; }

        [Column(TypeName = "bigint")]
        public long SpoArchivedSize { get; set; }

        [Column(TypeName = "bigint")]
        public long OdArchivedSize { get; set; }

        [Column(TypeName = "bigint")]
        public long SpoDestroyedFromArchiveSize { get; set; }

        [Column(TypeName = "bigint")]
        public long OdDestroyedFromArchiveSize { get; set; }

        [Column(TypeName = "bigint")]
        public long SpoDestroyedFromLiveSize { get; set; }

        [Column(TypeName = "bigint")]
        public long OdDestroyedFromLiveSize { get; set; }

        [Column(TypeName = "bigint")]
        public long CreatedTime { get; set; }
    }
}