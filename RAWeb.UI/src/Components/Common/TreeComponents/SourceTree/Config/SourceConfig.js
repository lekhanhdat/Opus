import NotImplementedError from "../../Errors/NotImplementedError";

class SourceConfig  {

    static get sourceFlag() {
        throw new NotImplementedError("SourceConfig", "sourceFlag");
    }

    get needPagingLevels() {
        throw new NotImplementedError("SourceConfig", "needPagingLevels");
    }

    get needAutoPagingLevels() {
        throw new NotImplementedError("SourceConfig", "needAutoPagingLevels");
    }

    get levelIconMapping() {
        throw new NotImplementedError("SourceConfig", "levelIconMapping");
    }

    get noSelectableLevels() {
        throw new NotImplementedError("SourceConfig", "noSelectableLevels");
    }

    get noCheckableLevels() {
        throw new NotImplementedError("SourceConfig", "noCheckableLevels");
    }

    get getRequestControllerName() {
        throw new NotImplementedError("SourceConfig", "getRequestControllerName");
    }

    assignCheckedNodeValue(checkedNode, actualNode) {
        throw new NotImplementedError("SourceConfig", "assignCheckedNodeValue");
    }

    generateKey(actualNode) {
        return actualNode.id;
    }
}

export default SourceConfig;