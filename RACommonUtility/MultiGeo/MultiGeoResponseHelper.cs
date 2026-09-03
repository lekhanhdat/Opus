using AvePoint.RA.Common;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Object;
using System;

namespace AvePoint.RA.RACommonUtility.MultiGeo;

internal static class MultiGeoResponseHelper
{
    public static TResponse CreateUnsupportedCommonDataResponse<TResponse>(string errorMessage)
    {
        if (typeof(TResponse) == typeof(string))
        {
            return (TResponse)(object)errorMessage;
        }

        if (typeof(TResponse) == typeof(RAReturnMessage))
        {
            return (TResponse)(object)new RAReturnMessage
            {
                MessageType = RAMessageType.Failed,
                ErrorMessage = errorMessage,
            };
        }

        var response = Activator.CreateInstance<TResponse>();
        var responseType = response?.GetType();
        if (responseType == null)
        {
            throw new InvalidOperationException($"Unable to create response for type [{typeof(TResponse).FullName}].");
        }

        var messageTypeProperty = responseType.GetProperty(nameof(RAReturnMessage.MessageType));
        if (messageTypeProperty?.CanWrite == true && messageTypeProperty.PropertyType == typeof(RAMessageType))
        {
            messageTypeProperty.SetValue(response, RAMessageType.Failed);
        }

        var errorMessageProperty = responseType.GetProperty(nameof(RAReturnMessage.ErrorMessage));
        if (errorMessageProperty?.CanWrite == true && errorMessageProperty.PropertyType == typeof(string))
        {
            errorMessageProperty.SetValue(response, errorMessage);
        }

        return response;
    }
}