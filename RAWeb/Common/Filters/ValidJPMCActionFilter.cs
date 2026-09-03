using AvePoint.RA.DB.CosmosDBControl;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.Filters
{
    public class ValidJPMCActionFilter : BaseActionFilter
    {
        public ValidJPMCActionFilter() { }

        protected override Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            if (!RMCosmosDBIndependentController.IsEnabledIndependent())
            {
                actionContext.Result = new ObjectResult("Access Denied") { StatusCode = (int)HttpStatusCode.Forbidden };
            }
            return Task.CompletedTask;
        }
    }
}
