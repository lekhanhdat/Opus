const LevelIconMapping = new Map([
    ["Root", "fia-term-group"],
    ["TermGroup", "fia-term-group"],
    ["TermSet", "fia-term-set"],
    ["Term", "fia-term"]
]);

const NoCheckableLevels = new Set([
    "Root",
    "TermGroup"
]);

class NodeParser {

    constructor() {
        this.levelIconMapping = LevelIconMapping;
        this.noCheckableLevels = NoCheckableLevels;
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
                title: node.Name,
                icon: this.getNodeIcon(node),
                enablePagingChildren: false,
                autoPagingChildren: false,
                checkable: !this.noCheckableLevels.has(node.Type),
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
        return this.levelIconMapping.get(node.Type);
    }

    addNodeToCache(node) {
        const key = this.generateKey(node);
        this.nodesCache.set(key, node);
        return key;
    }

    generateKey(node) {
        return node.UniqueId.toString();
    }
}

export default NodeParser;