import AzureFileShareConfig from "./AzureFileShareConfig";
import BoxConfig from "./BoxConfig";
import SharePointOnlineConfig from "./SharePointOnlineConfig";

export default new Map([
    [SharePointOnlineConfig.sourceFlag, new SharePointOnlineConfig()],
    [AzureFileShareConfig.sourceFlag, new AzureFileShareConfig()],
    [BoxConfig.sourceFlag, new BoxConfig()],
]);