const LogicType = {
    None: 0,
    And: 1,
    Or: 2,
};

const LogicTypeI18ns = new Map([
    [LogicType.And, RMResx.RM_HS_SearchKeywordAnd],
    [LogicType.Or, RMResx.RM_HS_SearchKeywordOr],
]);

const LogicTypeExpression = new Map([
    [LogicType.And, "&&"],
    [LogicType.Or, "||"],
]);

const LogicConstants = {
    type: LogicType,
    i18n: LogicTypeI18ns,
    expression: LogicTypeExpression,
};

export default LogicConstants;
