import Config from "../Config/index";

class NodeParser {

    constructor(sourceFlag) {
        const sourceTreeConfig = Config.get(sourceFlag);
        this.sourceFlag = sourceFlag;
        this.needPagingLevels = sourceTreeConfig.needPagingLevels;
        this.needAutoPagingLevels = sourceTreeConfig.needAutoPagingLevels;
        this.levelIconMapping = sourceTreeConfig.levelIconMapping;
        this.noSelectableLevels = sourceTreeConfig.noSelectableLevels;
        this.getRootNodeRequestUrl = `/api/${sourceTreeConfig.getRequestControllerName}/GetRootNode`;
        this.getChildrenWithSettingIconRequestUrl = `/api/${sourceTreeConfig.getRequestControllerName}/GetChildrenWithSettingIcon`;
        this.getPagingChildrenWithSettingIconRequestUrl = `/api/${sourceTreeConfig.getRequestControllerName}/GetPagingChildrenWithSettingIcon`;
        this.generateKey = sourceTreeConfig.generateKey;
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
                selectable: !this.noSelectableLevels.has(node.level),
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

    getRootNodeRequstOption() {
        return {
            url: this.getRootNodeRequestUrl,
        };
    }

    getChildrenRequestOption(node) {
        return {
            url: this.getChildrenWithSettingIconRequestUrl,
            data: node
        };
    }

    getPagingChildrenRequestOption(node) {
        return {
            url: this.getPagingChildrenWithSettingIconRequestUrl,
            data: node
        };
    }

}

export default NodeParser;