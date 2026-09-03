using AvePoint.RA.Common.Security;
using AvePoint.RA.RACommonUtility.Email.Client.Config;
using AvePoint.RA.RACommonUtility.Email.Model;
using AvePoint.Wrapper.Common;
using System;
using System.Reflection;

namespace AvePoint.RA.RACommonUtility.Email.Client.Compilers
{
    public class RMBorrowerNotificationEmailCompiler : IRMEmailCompiler
    {
        public RMEmailTemplateType TemplateType => RMEmailTemplateType.BorrowerNotification;

        public string CompileBody(string body, RMEmailTemplateParameters parameters)
        {
            const string reviewPlaceholder = "AvePoint Opus &#62; Physical Records &#62; Explorer";
            var linkedUrl = AveUrlUtility.CombineUrl(RMSSOHelper.RecoSsoLoginUrl, "?redirect=Root/PRM/RecordsExplorer");
            body = ReplacePlaceholders(body, parameters);
            body = body.Replace(reviewPlaceholder, RMEmailTemplateHtml.JOB_NOTIFICATION_BORROWER_LINK.Replace("@Link", linkedUrl));
            return body;
        }

        public string CompileSubject(string subject, RMEmailTemplateParameters parameters)
        {
            return ReplacePlaceholders(subject, parameters);
        }

        private static string ReplacePlaceholders(string content, RMEmailTemplateParameters parameters)
        {
            var borrowerParameters = parameters as RMBorrowerNotificationEmailTemplateParameters;
            var propertyInfoList = borrowerParameters.GetType().GetProperties();
            foreach (var propertyInfo in propertyInfoList)
            {
                var placeholder = propertyInfo.GetAttribute<RMEmailTemplatePlaceholderAttribute>()?.PlaceHolder;
                var value = propertyInfo.GetValue(borrowerParameters)?.ToString();
                if (!string.IsNullOrEmpty(placeholder) && !string.IsNullOrEmpty(value))
                {
                    content = content.Replace(placeholder, value);
                }
            }
            return content;
        }
    }
}
