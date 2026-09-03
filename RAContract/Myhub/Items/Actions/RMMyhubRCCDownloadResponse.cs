using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Myhub.Items.Actions
{
    public class RMMyhubReportDownloadResponse
    {
        public string FileData { get; set; }      
        public string FileName { get; set; }      
        public string ContentType { get; set; }   
        public long FileSize { get; set; }        
        public bool Success { get; set; }         
        public string Message { get; set; }       
    }
}
