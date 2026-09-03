import { IconStatus, SourceFlag, NodeLevel } from "../../Constants/index";
import SourceConfig from "./SourceConfig";

class AzureFileShareConfig extends SourceConfig {

    static get sourceFlag() {
        return SourceFlag.AzureFileShare;
    }

    get needPagingLevels() {
        return new Set([
            NodeLevel.Root,
            NodeLevel.AzureFileShareGroup,
            NodeLevel.AzureFileShareConnection,
            NodeLevel.AzureFileShareDirectory,
            // NodeLevel.Farm,
        ]);
    }

    get needAutoPagingLevels() {
        return new Set([
            NodeLevel.Root,
            NodeLevel.AzureFileShareGroup,
            NodeLevel.AzureFileShareConnection,
            NodeLevel.AzureFileShareDirectory,
        ]);
    }

    get levelIconMapping() {
        return new Map([
            [IconStatus.NoSet, new Map([
                [NodeLevel.Root, "fia-azure-file"],
                [NodeLevel.AzureFileShareGroup, "fia-connection-group"],
                [NodeLevel.AzureFileShareConnection, "fia-connection"],
                [NodeLevel.AzureFileShareDirectory, "fia-folder"]
            ])],
            [IconStatus.Break, new Map([
                [NodeLevel.AzureFileShareGroup, "fia-connection-group-unique-c"],
                [NodeLevel.AzureFileShareConnection, "fia-connection-unique-c"],
                [NodeLevel.AzureFileShareDirectory, "fia-folder-unique-c"]
            ])],
            [IconStatus.Inhert, new Map([
                [NodeLevel.AzureFileShareConnection, "fia-connection-inherit-b"],
                [NodeLevel.AzureFileShareDirectory, "fia-folder-inherit-b"]
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
        return "AzureFileShareTreeQuery";
    }

    assignCheckedNodeValue(checkedNode, actualNode) {
        // checkedNode.siteId = actualNode.siteId;
        // checkedNode.webId = actualNode.webId;
        // checkedNode.listId = actualNode.listId;
    }

    generateKey(actualNode) {
        return actualNode.id;
    }
}

export default AzureFileShareConfig;