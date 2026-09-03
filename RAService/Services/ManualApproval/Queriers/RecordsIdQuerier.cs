using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.DB.Explorer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ManualApproval.Queriers
{
    internal class RecordsIdQuerier : ISorter
    {
        public ManualApprovalOrderOptions OrderOption => ManualApprovalOrderOptions.RecordsId;

        public Expression<Func<ManualApprovalRecord, dynamic>> GetCosmosDBOrderExpression()
        {
            return (root) => root.RecordsId;
        }
    }
}
