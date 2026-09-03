import { AnalyseMethodCriterias, LogicConstants } from "../Constants";
import { ConditionCategoryInfo } from "../Constants/ConditionInfoes";

class LogicBuilder {
    static build = (criteriaInfoes) => {
        const reversedCriteriaInfoes = [...criteriaInfoes].reverse();
        let leftBracketCount = 1;
        let logicText = ")";
        let beforeLogicType = LogicConstants.type.None;
        const criteriaInfoCount = reversedCriteriaInfoes.length;
        for (let index = 0; index < criteriaInfoCount; index++) {
            let criteriaLogic = "";

            const currentCriteriaInfo = reversedCriteriaInfoes[index];
            criteriaLogic = criteriaLogic + currentCriteriaInfo.order;

            let nextLogic = LogicConstants.type.None;
            if (index + 1 < criteriaInfoCount) {
                nextLogic = reversedCriteriaInfoes[index + 1].logic;
                criteriaLogic =
                    (nextLogic === LogicConstants.type.And ? ` ${LogicConstants.expression.get(LogicConstants.type.And)} ` : ` ${LogicConstants.expression.get(LogicConstants.type.Or)} `) +
                    criteriaLogic;
            }

            if (
                nextLogic !== LogicConstants.type.None &&
                beforeLogicType !== LogicConstants.type.None &&
                nextLogic !== beforeLogicType
            ) {
                criteriaLogic = criteriaLogic + ")";
                leftBracketCount++;
            }

            beforeLogicType = nextLogic;
            logicText = criteriaLogic + logicText;
        }
        return new Array(leftBracketCount).fill("(").join("") + logicText;
    };

    static translate = (logicText) => {

        let res = logicText;

        for(let logicType of [LogicConstants.type.And, LogicConstants.type.Or]) {
            const expression = LogicConstants.expression.get(logicType);
            const i18n = LogicConstants.i18n.get(logicType);
            res = res.replaceAll(expression, i18n);
        }

        return res;
    }

    static getCriteriaDisplayInfoes = (analyseMethod, criteriaInfoes) => {
        const res = [];
        const criterias = AnalyseMethodCriterias.get(analyseMethod);
        for(let criteriaInfo of criteriaInfoes) {
            const criteria = criterias.find(item => item.value === criteriaInfo.criteriaType);
            const condition = criteria.conditions.find(item => item.category === criteriaInfo.conditionInfo.category && item.type === criteriaInfo.conditionInfo.logic);
            const categoryInfo = ConditionCategoryInfo.get(condition.category);
            res.push({
                order: criteriaInfo.order,
                criteriaName: criteria.name,
                conidtionName: categoryInfo.i18n.get(condition.type),
                extraComponent: categoryInfo.componentInfo.get(condition.type).extraComponent || null,
                value: categoryInfo.componentInfo.get(condition.type).component.getDisplayText(criteriaInfo.conditionInfo.value),
                extraValue: categoryInfo.componentInfo.get(condition.type).extraComponent?.getDisplayText(criteriaInfo.conditionInfo.extraValue),
            });
        }

        return res;
    }
}

export default LogicBuilder;
