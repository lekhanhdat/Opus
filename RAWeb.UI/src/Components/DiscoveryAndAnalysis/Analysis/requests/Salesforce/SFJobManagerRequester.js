import ExceptionHandler from "../ExceptionHandler";

class JobManagerRequester {
    static getLatest = () => {
        return ExceptionHandler.handleAsync(async () => {
            const requestOption = {
                url: "/api/RMDiscoverySalesforceJobManagementApi/GetLatest",
                method: "GET",
            };
            return await fetchUtility(requestOption);
        }, []);
    };
}

export default JobManagerRequester;