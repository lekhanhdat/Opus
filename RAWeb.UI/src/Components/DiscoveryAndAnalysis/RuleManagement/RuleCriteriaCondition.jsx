import _ from "lodash";
import { useEffect, useImperativeHandle, useState, forwardRef } from "react";
import { ConditionCategoryInfo } from "./Constants/ConditionInfoes";
import { CriteriaConditionAnalyzer } from "./util";

const RuleCriteriaCondition = ({ criterias, criteriaInfo, onChange }, ref) => {
    const [innerCriteriaInfo, setInnerCriteriaInfo] = useState({
        criteriaOptions: [],
        conditionOptions: [],
        selectedConditionInfo: {
            componentInfo: {
                layout: "",
            },
        },
    });

    const [validateInfo, setValidateInfo] = useState({ isValidated: true });

    const [validateExtraInfo, setValidateExtraInfo] = useState({ isValidated: true });

    useImperativeHandle(ref, () => ({
        onValidate: () => {
            const hasExtraComponent = !!innerCriteriaInfo.selectedConditionInfo.componentInfo.extraComponent;

            if (hasExtraComponent) {
                const extraValidateRes = innerCriteriaInfo.selectedConditionInfo.extraValidate(
                    criteriaInfo.conditionInfo.extraValue
                );
                const validateRes = innerCriteriaInfo.selectedConditionInfo.validate(
                    criteriaInfo.conditionInfo.value
                );

                setValidateExtraInfo(extraValidateRes);
                setValidateInfo(validateRes);

                return extraValidateRes.isValidated && validateRes.isValidated;
            }
            const validateRes =
                innerCriteriaInfo.selectedConditionInfo.validate(
                    criteriaInfo.conditionInfo.value
                );
            setValidateInfo(validateRes);
            return validateRes.isValidated;
        },
    }));

    useEffect(() => {
        const criteriaOptions = criterias.map((item) => ({
            name: item.name,
            tooltip: item.tooltip,
            value: item.value,
            checked: item.value === criteriaInfo.criteriaType,
        }));

        const criteria = criterias.find(
            (item) => item.value === criteriaInfo.criteriaType
        );
        const conditionOptions = criteria.conditions.map((item) => {
            const categoryInfo = ConditionCategoryInfo.get(item.category);
            return {
                name: categoryInfo.i18n.get(item.type),
                value: `${item.category}_${item.type}`,
                checked:
                    item.category === criteriaInfo.conditionInfo.category &&
                    item.type === criteriaInfo.conditionInfo.logic,
            };
        });

        const criteriaComponentInfo =
            CriteriaConditionAnalyzer.getCriteriaComponentInfo(
                criterias,
                criteriaInfo.criteriaType,
                criteriaInfo.conditionInfo.category,
                criteriaInfo.conditionInfo.logic
            );

        setInnerCriteriaInfo({
            criteriaOptions: criteriaOptions,
            conditionOptions: conditionOptions,
            selectedConditionInfo: criteriaComponentInfo,
        });
    }, [criteriaInfo]);

    const onCriteriaChange = (value) => {
        const clonedCriteriaInfo = _.cloneDeep(criteriaInfo);
        clonedCriteriaInfo.criteriaType = value;

        const criteriaComponentInfo =
            CriteriaConditionAnalyzer.getCriteriaComponentInfo(
                criterias,
                value
            );

        clonedCriteriaInfo.conditionInfo["category"] =
            criteriaComponentInfo.conditionInfo.category;
        clonedCriteriaInfo.conditionInfo["logic"] =
            criteriaComponentInfo.conditionInfo.type;
        clonedCriteriaInfo.conditionInfo["value"] =
            criteriaComponentInfo.defaultValue;
        clonedCriteriaInfo.conditionInfo["extraValue"] =
            criteriaComponentInfo.extraDefaultValue;
        setValidateInfo({ isValidated: true });
        setValidateExtraInfo({ isValidated: true });
        onChange(clonedCriteriaInfo);
    };

    const onConditionChange = (value) => {
        const condition = Number.parseInt(value.split("_")[1]);
        const category = Number.parseInt(value.split("_")[0]);
        const clonedCriteriaInfo = _.cloneDeep(criteriaInfo);
        clonedCriteriaInfo.conditionInfo.logic = condition;

        const criteriaComponentInfo =
            CriteriaConditionAnalyzer.getCriteriaComponentInfo(
                criterias,
                criteriaInfo.criteriaType,
                category,
                condition
            );

        clonedCriteriaInfo.conditionInfo["category"] = category;
        clonedCriteriaInfo.conditionInfo["value"] =
            criteriaComponentInfo.defaultValue;
        clonedCriteriaInfo.conditionInfo["extraValue"] =
            criteriaComponentInfo.extraDefaultValue;
        setValidateInfo({ isValidated: true });
        setValidateExtraInfo({ isValidated: true });
        onChange(clonedCriteriaInfo);
    };

    const onExtraValueChange = (value) => {
        const clonedCriteriaInfo = _.cloneDeep(criteriaInfo);
        clonedCriteriaInfo.conditionInfo.extraValue = value;
        if (!validateExtraInfo.isValidated) {
            const validateRes =
                innerCriteriaInfo.selectedConditionInfo.extraValidate(value);
            setValidateExtraInfo(validateRes);
        }
        onChange(clonedCriteriaInfo);
    };

    const onValueChange = (value) => {
        const clonedCriteriaInfo = _.cloneDeep(criteriaInfo);
        clonedCriteriaInfo.conditionInfo.value = value;
        if (!validateInfo.isValidated) {
            const validateRes =
                innerCriteriaInfo.selectedConditionInfo.validate(value);
            setValidateInfo(validateRes);
        }
        onChange(clonedCriteriaInfo);
    };

    const handleValidate = () => {
        let firstInValid = null;

        if (!validateExtraInfo.isValidated) {
            firstInValid = validateExtraInfo;
        } else if (!validateInfo.isValidated) {
            firstInValid = validateInfo;
        }

        if (_.isNil(firstInValid)) {
            return null;
        }

        return (
            <div className="reco-error-messages">
                {firstInValid.errorMessages.map((item, index) => (
                    <div
                        key={index}
                        tabIndex="0"
                        className="reco-error-message"
                    >
                        {item}
                    </div>
                ))}
            </div>
        );
    }

    return (
        <div className="reco-criteria-condition-container">
            <div
                className={
                    innerCriteriaInfo.selectedConditionInfo.componentInfo.layout
                }
            >
                <div>
                    <R.Combobox
                        id="raCriteriaCom1"
                        width={"100%"}
                        popupMaxHeight={400}
                        searchable={false}
                        items={innerCriteriaInfo.criteriaOptions}
                        textField="name"
                        valueField="value"
                        tooltipField="tooltip"
                        onChange={(args) =>
                            onCriteriaChange(args.newValue.value)
                        }
                    />
                </div>
                {[innerCriteriaInfo.selectedConditionInfo].map(
                    (item, index) => {
                        if (_.isNil(item.componentInfo.extraComponent)) {
                            return null;
                        }
                        return (
                            <item.componentInfo.extraComponent
                                key={index}
                                value={criteriaInfo.conditionInfo.extraValue}
                                onChange={(value) => {
                                    onExtraValueChange(value);
                                }}
                            />
                        );
                    }
                )}
                <div>
                    <R.Combobox
                        id="raCriteriaCom2"
                        width={"100%"}
                        popupMaxHeight={400}
                        searchable={false}
                        items={innerCriteriaInfo.conditionOptions}
                        textField="name"
                        valueField="value"
                        onChange={(args) =>
                            onConditionChange(args.newValue.value)
                        }
                    />
                </div>
                {[innerCriteriaInfo.selectedConditionInfo].map(
                    (item, index) => {
                        if (_.isNil(item.componentInfo.component)) {
                            return <div key={index}></div>;
                        }
                        return (
                            <item.componentInfo.component
                                key={`${index}_${criteriaInfo.conditionInfo.value}_${criteriaInfo.conditionInfo.logic}`}
                                value={criteriaInfo.conditionInfo.value}
                                onChange={(value) => {
                                    onValueChange(value);
                                }}
                            />
                        );
                    }
                )}
            </div>
            {handleValidate()}
        </div>
    );
};

export default forwardRef(RuleCriteriaCondition);
