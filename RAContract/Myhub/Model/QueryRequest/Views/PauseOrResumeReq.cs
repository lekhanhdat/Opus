using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Myhub.Model.QueryRequest.Views
{
    public class PauseOrResumeReq
    {
        public List<string> NodeIds { get; set; }

        public int IsPause { get; set; }
    }


}
