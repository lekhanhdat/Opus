using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvePoint.RA.DB.Model
{
    public class MultiGeoSettingInfo : BaseModel
    {
        [Key]
        [Column(TypeName = "uniqueidentifier")]
        public Guid Id { set; get; }
        [Index]
        [MaxLength(255)]
        [Column(TypeName = "nvarchar")]
        [Required]
        public string DataCenter { set; get; }
        [Column(TypeName = "nvarchar(MAX)")]
        public string IPAddresses { set; get; }
        [Column(TypeName = "bit")]
        public bool IsDeleted { set; get; }
        [Column(TypeName = "bigint")]
        public long CreateTime { get; set; }
        [Column(TypeName = "bigint")]
        public long UpdateTime { get; set; }
    }
}
