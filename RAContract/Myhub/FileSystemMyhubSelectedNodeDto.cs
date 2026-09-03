using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Myhub
{
    public class FileSystemMyhubSelectedNodeDto
    {
        public Guid GroupId { get; set; }
        public Guid NodeId { set; get; }
        public string FullPath { get; set; }
        public string PartitionKeyId { get; set; }
        public int Level { get; set; }
    }
}
