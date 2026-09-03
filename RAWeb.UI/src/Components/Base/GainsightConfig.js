const GainsightConfig = async () => {
    try {
        if (RM.gData.disabledGainsight) {
            return;
        }
        MaskUrl();

    } catch (error) {
        console.error(error);
    }
};

const MaskUrl = () => {
    const maskRegex = /(https|http)\:\/\/(.)*\/(.)*/i; //Mask

    const productionRegex =
        /https:\/\/(.)*\.avepointonlineservices\.com\/(.)*/i; //Keep
    const testRegex = /https:\/\/(.)*\.sharepointguild\.com\/(.)*/i; //Keep

    var config = {
        filterUrls: ["*"],

        maskUrlFunction: (urlPayload, mode) => {
            if (
                (RM.gData.enviromentName.indexOf("test") < 0 && 
                 RM.gData.enviromentName.indexOf("GCP Test") < 0 &&
                 !productionRegex.test(urlPayload.url)) ||
                ((RM.gData.enviromentName.indexOf("test") >= 0 || 
                  RM.gData.enviromentName.indexOf("GCP Test") >= 0) &&
                 !testRegex.test(urlPayload.url))
            ) {
                urlPayload.url = urlPayload.url.replace(
                    maskRegex,
                    "https://apt.maksed.domain/apt-maksed-url"
                );
            }
            return urlPayload;
        },
        espProxyDomain:"https://px-esp.avepointonlineservices.com", 
        contentProxyDomain:"https://px-sdk.avepointonlineservices.com"
    };

    let apiKey = "AP-NQTLLA8ZNFZC-2";
    if (RM.gData.enviromentName.indexOf("test") >= 0 || RM.gData.enviromentName.indexOf("GCP Test") >= 0) {
        apiKey = "AP-NQTLLA8ZNFZC-2-3";
    }

    (function (n, t, a, e, x) {
        var i = "aptrinsic";
        (n[i] =
            n[i] ||
            function () {
                (n[i].q = n[i].q || []).push(arguments);
            }),
            (n[i].p = e);
        n[i].c = x;
        var r = t.createElement("script");
        (r.async = !0), (r.src = a + "?a=" + e);
        var c = t.getElementsByTagName("script")[0];
        c.parentNode.insertBefore(r, c);
    })(
        window,
        document,
        "https://px-sdk.avepointonlineservices.com/api/aptrinsic.js",
        apiKey,
        config
    );
};

const GetSegmentJsKey = () => {
    return new Promise((resolve, reject) => {
        resolve("8lwV5JQoBYdkcOK1ys0wfJttbf0PUmKn");
    });
};

const BindingAnalytics = (segmentJsKey) => {
    const analytics = (window.analytics = window.analytics || []);
    if (!analytics.initialize) {
        if (analytics.invoked) {
            window.console &&
                console.error &&
                console.error("Segment snippet included twice.");
        } else {
            analytics.invoked = !0;
            analytics.methods = [
                "trackSubmit",
                "trackClick",
                "trackLink",
                "trackForm",
                "pageview",
                "identify",
                "reset",
                "group",
                "track",
                "ready",
                "alias",
                "debug",
                "page",
                "once",
                "off",
                "on",
                "addSourceMiddleware",
                "addIntegrationMiddleware",
                "setAnonymousId",
                "addDestinationMiddleware",
            ];
            analytics.factory = function (e) {
                return function () {
                    const t = Array.prototype.slice.call(arguments);
                    t.unshift(e);
                    analytics.push(t);
                    return analytics;
                };
            };
            for (var e = 0; e < analytics.methods.length; e++) {
                const key = analytics.methods[e];
                analytics[key] = analytics.factory(key);
            }
            analytics.load = function (key, e) {
                const t = document.createElement("script");
                t.type = "text/javascript";
                t.async = !0;
                t.src =
                    "https://cdn.segment.com/analytics.js/v1/" +
                    key +
                    "/analytics.min.js";
                var n = document.getElementsByTagName("script")[0];
                n.parentNode.insertBefore(t, n);
                analytics._loadOptions = e;
            };
            analytics._writeKey = segmentJsKey;
            analytics.SNIPPET_VERSION = "4.15.3";
            analytics.load(segmentJsKey);
            analytics.page();
        }
    }
};

export default GainsightConfig;
