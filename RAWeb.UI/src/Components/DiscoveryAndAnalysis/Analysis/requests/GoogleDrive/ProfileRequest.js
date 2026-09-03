import ExceptionHandler from "../ExceptionHandler";

class ProfileRequester {
    static addInactiveProfileInfo = (dataInfo) => {
        return ExceptionHandler.handleAsync(async () => {
            const requestOption = {
                url: "/api/RMDiscoveryGoogleProfileApi/AddInactiveProfileInfo",
                method: "POST",
                data: dataInfo
            };
            return await fetchUtility(requestOption);
        }, {
            MessageType: 1,
            ErrorMessage: RMResx.RM_HS_Criteria_View_Msg_ValidOtherError
        });
    };

    static updateInactiveProfileInfo = (dataInfo) => {
        return ExceptionHandler.handleAsync(async () => {
            const requestOption = {
                url: "/api/RMDiscoveryGoogleProfileApi/UpdateInactiveProfileInfo",
                method: "POST",
                data: dataInfo
            };
            return await fetchUtility(requestOption);
        }, {
            MessageType: 1,
            ErrorMessage: RMResx.RM_HS_Criteria_View_Msg_ValidOtherError
        });
    };

    static deleteInactiveProfileInfo = (dataInfo) => {
        return ExceptionHandler.handleAsync(async () => {
            const requestOption = {
                url: "/api/RMDiscoveryGoogleProfileApi/DeleteInactiveProfileInfo",
                method: "POST",
                data: dataInfo
            };
            return await fetchUtility(requestOption);
        }, {
            MessageType: 1,
            ErrorMessage: RMResx.RM_HS_Criteria_View_Msg_ValidOtherError
        });
    }

    static getInactiveProfileInfoList = (organizationId) => {
        if(_.isNil(organizationId)) {
            return [];
        }
        return ExceptionHandler.handleAsync(async () => {
            const requestOption = {
                url: `/api/RMDiscoveryGoogleProfileApi/GetInactiveProfileInfoList?organizationId=${organizationId}`,
                method: "GET",
            };
            return await fetchUtility(requestOption);
        }, []);
    }

    static addRotProfileInfo = (dataInfo) => {
        return ExceptionHandler.handleAsync(async () => {
            const requestOption = {
                url: "/api/RMDiscoveryGoogleProfileApi/AddRotProfileInfo",
                method: "POST",
                data: dataInfo
            };
            return await fetchUtility(requestOption);
        }, {
            MessageType: 1,
            ErrorMessage: RMResx.RM_HS_Criteria_View_Msg_ValidOtherError
        });
    };

    static updateRotProfileInfo = (dataInfo) => {
        return ExceptionHandler.handleAsync(async () => {
            const requestOption = {
                url: "/api/RMDiscoveryGoogleProfileApi/UpdateRotProfileInfo",
                method: "POST",
                data: dataInfo
            };
            return await fetchUtility(requestOption);
        }, {
            MessageType: 1,
            ErrorMessage: RMResx.RM_HS_Criteria_View_Msg_ValidOtherError
        });
    };

    static deleteRotProfileInfo = (dataInfo) => {
        return ExceptionHandler.handleAsync(async () => {
            const requestOption = {
                url: "/api/RMDiscoveryGoogleProfileApi/DeleteRotProfileInfo",
                method: "POST",
                data: dataInfo
            };
            return await fetchUtility(requestOption);
        }, {
            MessageType: 1,
            ErrorMessage: RMResx.RM_HS_Criteria_View_Msg_ValidOtherError
        });
    }

    static getRotProfileInfoList = (organizationId) => {
        if(_.isNil(organizationId)) {
            return [];
        }
        return ExceptionHandler.handleAsync(async () => {
            const requestOption = {
                url: `/api/RMDiscoveryGoogleProfileApi/GetRotProfileInfoList?organizationId=${organizationId}`,
                method: "GET",
            };
            return await fetchUtility(requestOption);
        }, []);
    }
}

export default ProfileRequester;