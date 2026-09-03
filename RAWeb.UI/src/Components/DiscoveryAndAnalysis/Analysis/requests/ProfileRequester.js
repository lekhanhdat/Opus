import ExceptionHandler from "./ExceptionHandler";

class ProfileRequester {
    static addInactiveProfileInfo = (dataInfo) => {
        return ExceptionHandler.handleAsync(async () => {
            const requestOption = {
                url: "/api/RMDiscoveryOffice365ProfileApi/AddInactiveProfileInfo",
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
                url: "/api/RMDiscoveryOffice365ProfileApi/UpdateInactiveProfileInfo",
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
                url: "/api/RMDiscoveryOffice365ProfileApi/DeleteInactiveProfileInfo",
                method: "POST",
                data: dataInfo
            };
            return await fetchUtility(requestOption);
        }, {
            MessageType: 1,
            ErrorMessage: RMResx.RM_HS_Criteria_View_Msg_ValidOtherError
        });
    }

    static getInactiveProfileInfoes = (o365TenantId) => {
        if(_.isNil(o365TenantId)) {
            return [];
        }
        return ExceptionHandler.handleAsync(async () => {
            const requestOption = {
                url: `/api/RMDiscoveryOffice365ProfileApi/GetInactiveProfileInfoes?o365TenantId=${o365TenantId}`,
                method: "GET",
            };
            return await fetchUtility(requestOption);
        }, []);
    }

    static addRotProfileInfo = (dataInfo) => {
        return ExceptionHandler.handleAsync(async () => {
            const requestOption = {
                url: "/api/RMDiscoveryOffice365ProfileApi/AddRotProfileInfo",
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
                url: "/api/RMDiscoveryOffice365ProfileApi/UpdateRotProfileInfo",
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
                url: "/api/RMDiscoveryOffice365ProfileApi/DeleteRotProfileInfo",
                method: "POST",
                data: dataInfo
            };
            return await fetchUtility(requestOption);
        }, {
            MessageType: 1,
            ErrorMessage: RMResx.RM_HS_Criteria_View_Msg_ValidOtherError
        });
    }

    static getRotProfileInfoes = (o365TenantId) => {
        if(_.isNil(o365TenantId)) {
            return [];
        }
        return ExceptionHandler.handleAsync(async () => {
            const requestOption = {
                url: `/api/RMDiscoveryOffice365ProfileApi/GetRotProfileInfoes?o365TenantId=${o365TenantId}`,
                method: "GET",
            };
            return await fetchUtility(requestOption);
        }, []);
    }
}

export default ProfileRequester;