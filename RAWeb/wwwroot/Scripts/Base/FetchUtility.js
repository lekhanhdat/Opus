/*Covered by AvePoint copyright and license agreement*/

window.fetchUtility = function (options, errorFun) {
    var request = {
        method: !options.method ? 'POST' : options.method,
        headers: {
            'Accept': 'application/json',
            "Content-Type": "application/json;charset=utf-8",
        },
        cache: 'no-store',
        body: JSON.stringify(options.data),
        credentials: "include",
        isParseJson: true,
        download: false
    };
    return fetch(options.url, request)
        .then(function (response) {
            if (response.ok) {
                if (request.download) {
                    return response;
                }
                else {
                    return response.text().then(function (dataString) {
                        return {
                            responseStatus: response.status,
                            responseString: dataString,
                            isParseJson: request.isParseJson,
                            isPassStatus: request.isPassStatus
                        };
                    });
                }
            }
            else{
                if (response.status == 403) {
                    // $$.messagebox(true, {
                    //     classify: "warn",
                    //     width: "550px",
                    //     hideActions: false,
                    //     title: RMResx.RM_JS_Common_RecourdAutomation,
                    //     content: RMResx.RM_JS_Common_NoPermissionLicense,
                    //     buttons: [
                    //         {
                    //             text: RMResx.RM_JS_Common_OK,
                    //             classify: "theme",
                    //             onClick: () => { $$.messagebox(false); }
                    //         }
                    //     ]
                    // });
                    console.log("403")
                    if (errorFun) {
                        console.log("403-fun")
                        errorFun(response, response.headers.get('CustomErrorCode'));
                    }
                } else if (response.status == 409) {
                        
                } else if (response.status == 401) {
                        
                } else {
                    if (errorFun) {
                        errorFun(response, response.headers.get('CustomErrorCode'));
                    }
                    else {
                        
                    }
                }
                return Promise.reject({
                    status: response.status,
                    statusText: response.statusText
                  })
            
            }
            
        }).then(function (fetchResult) {

            if (request.download) { return fetchResult };

            var queryResult = null;

            try {
                if (!fetchResult.responseString) {
                    return null;
                }
                if (fetchResult.isParseJson && fetchResult.responseString) {
                    if (!fetchResult.responseString || (typeof fetchResult.responseString != "string" && $.isEmptyObject(fetchResult.responseString))) {
                        queryResult = "";
                    } else {
                        queryResult = JSON.parse(fetchResult.responseString);
                        if (fetchResult.isPassStatus) {
                            queryResult[FetchResponsePropName.status] = fetchResult.responseStatus;
                        }
                    }
                } else {
                    queryResult = fetchResult.responseString;
                }
            }
            catch (ex) {
                console.log("An error happened while fetching information. Error:", ex);
            }
            return queryResult;
        });
};