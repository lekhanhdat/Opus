import ExceptionHandler from "../ExceptionHandler";

class ProfileRequester {
    static addInactiveProfileInfo = (dataInfo) => {
        return ExceptionHandler.handleAsync(async () => {
            const requestOption = {
                url: "/api/RMDiscoveryFSProfileApi/AddInactiveProfileInfo",
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
                url: "/api/RMDiscoveryFSProfileApi/UpdateInactiveProfileInfo",
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
                url: "/api/RMDiscoveryFSProfileApi/DeleteInactiveProfileInfo",
                method: "POST",
                data: dataInfo
            };
            return await fetchUtility(requestOption);
        }, {
            MessageType: 1,
            ErrorMessage: RMResx.RM_HS_Criteria_View_Msg_ValidOtherError
        });
    }

    static getInactiveProfileInfoList = () => {
        return ExceptionHandler.handleAsync(async () => {
            const requestOption = {
                url: `/api/RMDiscoveryFSProfileApi/GetInactiveProfileInfoList}`,
                method: "GET",
            };
            return await fetchUtility(requestOption);
        }, []);
    }

    static addRotProfileInfo = (dataInfo) => {
        return ExceptionHandler.handleAsync(async () => {
            const requestOption = {
                url: "/api/RMDiscoveryFSProfileApi/AddRotProfileInfo",
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
                url: "/api/RMDiscoveryFSProfileApi/UpdateRotProfileInfo",
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
                url: "/api/RMDiscoveryFSProfileApi/DeleteRotProfileInfo",
                method: "POST",
                data: dataInfo
            };
            return await fetchUtility(requestOption);
        }, {
            MessageType: 1,
            ErrorMessage: RMResx.RM_HS_Criteria_View_Msg_ValidOtherError
        });
    }

    static getRotProfileInfoList = () => {
        return ExceptionHandler.handleAsync(async () => {
            const requestOption = {
                url: `/api/RMDiscoveryFSProfileApi/GetRotProfileInfoList`,
                method: "GET",
            };
            return await fetchUtility(requestOption);
        }, []);
    }
}

export default ProfileRequester;