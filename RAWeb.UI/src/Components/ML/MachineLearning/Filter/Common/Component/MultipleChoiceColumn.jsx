import { useEffect, useState } from 'react';
import { getChoiceOptions } from "../Function";

const MultipleChoiceColumn = ({label, options, selectedOptionValueList, onChange, searchable = false} ) => {

    const [columnOptions, setColumnOptions] = useState(options);

    useEffect(()=>{
        let columnOptions = getChoiceOptions(options, selectedOptionValueList || []);
        setColumnOptions(columnOptions);
    },[JSON.stringify(selectedOptionValueList), options]);

    const onChangeOptions = (args) => {
        let columnOptionValueList = args.newValue.map((item)=> { return item.value;});
        if(columnOptionValueList.length == columnOptions.length ){
            columnOptionValueList = [];
        }
        onChange(columnOptionValueList);
    };

    return  <$g.FormRow label={label}>
        <R.Multicombobox
            width={"100%"}
            searchable={searchable}
            required={true}
            textField="name"
            items={columnOptions}
            onChange={onChangeOptions}
        />
    </$g.FormRow>;
};

export default MultipleChoiceColumn;