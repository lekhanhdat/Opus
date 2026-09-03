import { SourceFlag } from "../../../Constants/index";
import Config from "../Config";

class NodeParser {

    constructor(sourceFlag) {

        if(sourceFlag === SourceFlag.None || sourceFlag === SourceFlag.All) {
            return;
        }

        const sourceTreeConfig = Config.get(sourceFlag);
        this.sourceFlag = sourceFlag;
        this.needPagingLevels = sourceTreeConfig.needPagingLevels;
        this.needAutoPagingLevels = sourceTreeConfig.needAutoPagingLevels;
        this.levelIconMapping = sourceTreeConfig.levelIconMapping;
        this.noCheckableLevels = sourceTreeConfig.noCheckableLevels;
        this.generateKey = sourceTreeConfig.generateKey;
        this.getRootNodeRequestUrl = `/api/${sourceTreeConfig.getRequestControllerName}/GetRootNode`;
        this.getChildrenRequestUrl = `/api/${sourceTreeConfig.getRequestControllerName}/GetChildren`;
        this.getPagingChildrenRequestUrl = `/api/${sourceTreeConfig.getRequestControllerName}/GetPagingChildren`;
        this.assignCheckedNodeValue = sourceTreeConfig.assignCheckedNodeValue;
        this.nodesCache = new Map();
    }

    convertToSimpleNode(nodes) {
        if(nodes === null || nodes === undefined) {
            return null;
        }

        const result = [];
        for (const node of nodes) {
            const key = this.addNodeToCache(node);
            const simpleNode = {
                key: key,
                title: node.displayName,
                icon: this.getNodeIcon(node),
                enablePagingChildren: this.needPagingLevels.has(node.level),
                autoPagingChildren: this.needAutoPagingLevels.has(node.level),
                checkable: !this.noCheckableLevels.has(node.level),
                autoExpand: false,
            };

            result.push(simpleNode);
        }

        return result;
    }

    getNodeByKey(key) {
        if (!this.nodesCache.has(key)) {
            return null;
        }
        
        return this.nodesCache.get(key);
    }

    getNodeIcon(node) {
        const icons = this.levelIconMapping.get(node.iconStatus);
        return icons.get(node.level);
    }

    addNodeToCache(node) {
        const key = this.generateKey(node);
        this.nodesCache.set(key, node);
        return key;
    }

    internalAssignCheckedNodeValue(checkedNode) {
        const actualNode = this.getNodeByKey(checkedNode.key);
        checkedNode.id = actualNode.id;
        checkedNode.level = actualNode.level;
        checkedNode.leafName = actualNode.leafName;
        checkedNode.fullPath = actualNode.fullPath;
        checkedNode.containerId = actualNode.containerId;
        this.assignCheckedNodeValue(checkedNode, actualNode);
    }

    getRootNodeRequstOption() {
        return {
            url: this.getRootNodeRequestUrl,
        };
    }

    getChildrenRequestOption(node) {
        return {
            url: this.getChildrenRequestUrl,
            data: node
        };
    }

    getPagingChildrenRequestOption(node) {
        return {
            url: this.getPagingChildrenRequestUrl,
            data: node
        };
    }
}

export default NodeParser;