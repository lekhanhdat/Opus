using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvePoint.RA.DB.Model
{
    public class RMStorageCostEvaluation : BaseModel
    {
        [Key]
        [Column(Order = 0, TypeName = "varchar")]
        [MaxLength(64)]
        public string TenantId { set; get; }

        [Key]
        [Column(Order = 1, TypeName = "varchar")]
        [MaxLength(64)]
        public string StorageId { get; set; }

        [Column(TypeName = "datetime2")]
        [Required]
        public DateTime CalculatedDate { get; set; }

        [Column(TypeName = "float")]
        public double TotalArchivedSizeInGB { get; set; } = 0;

        [Column(TypeName = "float")]
        public double TotalBlobSizeInGB { get; set; } = 0;

        [Column(TypeName = "float")]
        public double TotalUnrecordedSizeInGB { get; set; } = 0;
    }
}
