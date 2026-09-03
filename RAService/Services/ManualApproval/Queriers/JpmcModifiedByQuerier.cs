using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Explorer.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ManualApproval.Queriers
{
    internal class JpmcModifiedByQuerier : IFilterWithHistory
    {
        private readonly IRALogger _logger = new RALogger(typeof(JpmcModifiedByQuerier));
        public ManualApprovalFilterOptions FilterOption => ManualApprovalFilterOptions.JpmcModifiedBy;

        public async Task<Expression<Func<ManualApprovalRecord, bool>>> GetCosmosDBFilterExpressionAsync(string value)
        {
            var lowerValue = value.ToLower();
            return (root) => Enumerable.Contains(root.ModifiedBy_Array, lowerValue);
        }

        public async Task<ManualApprovalSqlDefintion> GetHistorySqlDefinitionAsync(string value)
        {
            var sql = "ModifiedBy LIKE '%'+@ModifiedBy+'%'";
            var parameter = new SqlParameter("@ModifiedBy", value);
            var result = new ManualApprovalSqlDefintion
            {
                Sql = sql,
            };
            result.Parameter.Add(parameter);
            return result;
        }
    }
}
