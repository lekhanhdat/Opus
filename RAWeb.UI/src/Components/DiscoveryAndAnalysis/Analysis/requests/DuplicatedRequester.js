import ExceptionHandler from "./ExceptionHandler";

class DuplicatedRequester {
    static getAllActiveExportLocation = (data) => {
        return ExceptionHandler.handleAsync(async () => {
            const requestOptions = {
                url: "/api/StorageDevice/GetAllActiveStorage",
                method: "POST",
                data,
            };
            return await fetchUtility(requestOptions);
        }, {});
    };

    static exportDuplicationReport = (data) => {
        return ExceptionHandler.handleAsync(
            async () => {
                const requestOptions = {
                    url: "/api/RMDiscoveryOffice365DuplicationDataApi/ExportDuplicationReport",
                    method: "POST",
                    data,
                };
                return await fetchUtility(requestOptions);
            },
            {
                MessageType: 1,
                ErrorMessage: RMResx.RM_HS_Criteria_View_Msg_ValidOtherError,
            },
        );
    };
}

export default DuplicatedRequester;
