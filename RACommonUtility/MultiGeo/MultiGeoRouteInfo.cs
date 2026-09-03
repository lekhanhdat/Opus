using System.Collections.Generic;

namespace AvePoint.RA.RACommonUtility.MultiGeo;

internal sealed class MultiGeoRouteInfo
{
    public bool IsRoute { get; init; }

    public string MainDataCenter { get; init; }

    public string MainApiUrl { get; init; }

    public IReadOnlyCollection<MultiGeoApiTarget> RouteApis { get; init; } = [];

    public bool IsEnableMultiGeoFeature { get; init; } = true;
}