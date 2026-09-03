import { IconStatus, SourceFlag, NodeLevel } from "../../Constants/index";
import SourceConfig from "./SourceConfig";

class SharePointOnlineConfig extends SourceConfig {

    static get sourceFlag() {
        return SourceFlag.SharePoint;
    }

    get needPagingLevels() {
        return new Set([
            NodeLevel.WebApplication,
            // NodeLevel.Farm,
        ]);
    }

    get needAutoPagingLevels() {
        return new Set([
            // NodeLevel.Farm
        ]);
    }

    get levelIconMapping() {
        return new Map([
            [IconStatus.NoSet, new Map([
                [NodeLevel.Root, "fia-sharepoint-online"],
                [NodeLevel.WebApplication, "fia-site-collection-group"],
                [NodeLevel.SiteCollection, "fia-site-collection"]
            ])],
            [IconStatus.Break, new Map([

            ])],
            [IconStatus.Inhert, new Map([

            ])]
        ]);
    }

    get noSelectableLevels() {
        return new Set([NodeLevel.Root]);
    }

    get noCheckableLevels() {
        return new Set([NodeLevel.Root]);
    }

    get getRequestControllerName() {
        return "SharePointOnlineTreeQuery";
    }

    assignCheckedNodeValue(checkedNode, actualNode) {
        checkedNode.siteId = actualNode.siteId;
        checkedNode.webId = actualNode.webId;
        checkedNode.listId = actualNode.listId;
    }

    generateKey(actualNode) {
        return actualNode.siteId + actualNode.id;
    }
}

export default SharePointOnlineConfig;