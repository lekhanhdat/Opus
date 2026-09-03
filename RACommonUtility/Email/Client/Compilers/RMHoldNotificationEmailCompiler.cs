using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Email.Client.Config;
using AvePoint.RA.RACommonUtility.Email.Model;
using System;
using System.Reflection;


namespace AvePoint.RA.RACommonUtility.Email.Client.Compilers
{
    internal class RMHoldNotificationEmailCompiler : IRMEmailCompiler
    {
        public RMEmailTemplateType TemplateType => RMEmailTemplateType.HoldNotification;

        public string CompileBody(string body, RMEmailTemplateParameters parameters)
        {
            const string opusRequestForReviewPlaceholder = "AvePoint Opus &#62; Manage Holds";
            var jobNotificationParameters = parameters as RMHoldNotificationEmailTemplateParameters;
            body = ReplacePlaceholders(body, parameters);

            var link = RMEmailTemplateHtml.HOLD_NOTIFICATION_REVIEW_LINK.Replace("@Link", jobNotificationParameters.RequestLink);
            body = body.Replace(opusRequestForReviewPlaceholder, link);
            return body;
        }
        public string CompileSubject(string subject, RMEmailTemplateParameters parameters)
        {
            return ReplacePlaceholders(subject, parameters);
        }

        private static string ReplacePlaceholders(string content, RMEmailTemplateParameters parameters)
        {
            var jobNotificationParameters = parameters as RMHoldNotificationEmailTemplateParameters;
            var propertyInfoList = jobNotificationParameters.GetType().GetProperties();
            foreach (var propertyInfo in propertyInfoList)
            {
                var placeholder = propertyInfo.GetAttribute<RMEmailTemplatePlaceholderAttribute>()?.PlaceHolder;
                var value = propertyInfo.GetValue(jobNotificationParameters)?.ToString();
                if (!string.IsNullOrEmpty(placeholder) && !string.IsNullOrEmpty(value))
                {
                    content = content.Replace(placeholder, value);
                }
            }

            return content;
        }
    }
}
