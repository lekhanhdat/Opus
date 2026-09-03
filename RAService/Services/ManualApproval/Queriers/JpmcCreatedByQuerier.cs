using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Explorer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace AvePoint.RA.Service.Services.ManualApproval.Queriers
{
    internal class JpmcCreatedByQuerier : IFilterWithHistory
    {
        private readonly IRALogger _logger = new RALogger(typeof(JpmcCreatedByQuerier));
        public ManualApprovalFilterOptions FilterOption => ManualApprovalFilterOptions.JpmcCreatedBy;

        public async Task<Expression<Func<ManualApprovalRecord, bool>>> GetCosmosDBFilterExpressionAsync(string value)
        {
            var lowerValue = value.ToLower();
            return (root) => Enumerable.Contains(root.CreatedBy_Array, lowerValue);
        }

        public async Task<ManualApprovalSqlDefintion> GetHistorySqlDefinitionAsync(string value)
        {
            var sql = "CreatedBy LIKE '%'+@CreatedBy+'%'";
            var parameter = new SqlParameter("@CreatedBy", value);
            var result = new ManualApprovalSqlDefintion
            {
                Sql = sql,
            };
            result.Parameter.Add(parameter);
            return result;
        }

    }

}
