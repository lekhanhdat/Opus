import { IconStatus, NodeLevel, SourceFlag } from "../../Constants";
import SourceConfig from "./SourceConfig";

class BoxConfig extends SourceConfig {

    static get sourceFlag() {
        return SourceFlag.Box;
    }

    get needPagingLevels() {
        return new Set([
            NodeLevel.Root,
            NodeLevel.BoxConnectionGroup,
            NodeLevel.BoxConnection,
            NodeLevel.BoxUser,
            NodeLevel.BoxDirectory,
        ]);
    }

    get needAutoPagingLevels() {
        return new Set([
            NodeLevel.Root,
            NodeLevel.BoxConnectionGroup,
            NodeLevel.BoxConnection,
            NodeLevel.BoxUser,
            NodeLevel.BoxDirectory,
        ]);
    }

    get levelIconMapping() {
        return new Map([
            [IconStatus.NoSet, new Map([
                [NodeLevel.Root, "fia-box-black-b"],
                [NodeLevel.BoxConnectionGroup, "fia-connection-group"],
                [NodeLevel.BoxConnection, "fia-connection"],
                [NodeLevel.BoxUser, "fia-user"],
                [NodeLevel.BoxDirectory, "fia-folder"],
            ])],
            [IconStatus.Break, new Map([
                [NodeLevel.BoxConnectionGroup, "fia-connection-group-unique-c"],
                [NodeLevel.BoxConnection, "fia-connection-unique-c"],
                [NodeLevel.BoxUser, "fia-box-unique-c"],
                [NodeLevel.BoxDirectory, "fia-folder-unique-c"],
            ])],
            [IconStatus.Inhert, new Map([
                [NodeLevel.BoxConnection, "fia-connection-inherit-b"],
                [NodeLevel.BoxUser, "fia-box-inherit-b"],
                [NodeLevel.BoxDirectory, "fia-folder-inherit-b"],
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
        return "BoxTreeQuery";
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

export default BoxConfig;