using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Model
{
    public class RMHoldMemberships : BaseModel
    {
        [Key]
        [Column(TypeName = "int", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }
        [Index]
        [Column(TypeName = "varchar")]
        [MaxLength(1024)]
        public string HoldId { set; get; }
        [Index]
        [Column(TypeName = "varchar")]
        [MaxLength(64)]
        public string UserId { get; set; }
             
    }
}
