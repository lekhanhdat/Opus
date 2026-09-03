const getChoiceOptions = (options, valueList) => {
    let columnOptions = [];
    for(let [optionValue, optionName] of options.entries()){
        columnOptions.push(
            {
                value: optionValue,
                name: optionName,
                checked: valueList.length == 0 || valueList.includes(optionValue)
            }
        );
    }
    return columnOptions;
};

export { getChoiceOptions };