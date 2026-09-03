import { ConditionCategoryInfo } from "../Constants/ConditionInfoes";

class CriteriaConditionAnalyzer {
    static getCriteriaComponentInfo = (
        criterias,
        criteriaType,
        conditionCategory,
        conditionLogic
    ) => {
        const criteria = criterias.find((item) => item.value === criteriaType);
        let condition = criteria.conditions[0];
        if(!_.isNil(conditionCategory) && !_.isNil(conditionLogic)) {
            condition = criteria.conditions.find(
                (item) =>
                    item.category === conditionCategory &&
                    item.type === conditionLogic
            );        
        }
        
        const categoryInfo = ConditionCategoryInfo.get(condition.category);
        const componentInfo = categoryInfo.componentInfo.get(condition.type);
        return {
            criteriaType: criteriaType,
            conditionInfo: condition,
            categoryInfo: categoryInfo,
            componentInfo: componentInfo,
            defaultValue: _.isNil(condition.defaultValue)
                ? componentInfo.component.defaultValue
                : condition.defaultValue,
            extraDefaultValue: _.isNil(condition.extraDefaultValue)
                ? componentInfo.extraComponent?.extraDefaultValue
                : condition.extraDefaultValue,
            validate: _.isNil(condition.validate)
                ? componentInfo.component.validate
                : condition.validate,
            extraValidate: _.isNil(condition.extraValidate)
                ? componentInfo.extraComponent?.validate
                : condition.extraValidate,
        };
    };
}

export default CriteriaConditionAnalyzer;