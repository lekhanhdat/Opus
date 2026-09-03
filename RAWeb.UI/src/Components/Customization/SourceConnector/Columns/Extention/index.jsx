import React, { forwardRef, useImperativeHandle, useRef } from "react";
import _ from "lodash";
import { ColumnType } from "../../Common/Constants";
import ChoiceOptions from "./ChoiceOptions";

const nonExtentionColumnTypes = [ColumnType.SingleText, ColumnType.MultipleText, ColumnType.PeopleOrGroup, ColumnType.DateTime, ColumnType.Taxonomy];

const choiceExtentionColumnTypes = [ColumnType.SingleChoice, ColumnType.MultipleChoice];

const Extention = forwardRef(({ columnType, extentionDefinition, onChange }, ref) => {

    const choiceOptionsRef = useRef();

    useImperativeHandle(ref, () => ({
        onValidate: () => {
            if(choiceExtentionColumnTypes.includes(columnType)) {
                return choiceOptionsRef.current.onValidate();
            }

            return true;
        }
    }));

    return (
        <div className="reco-connector-extention">
            {
                nonExtentionColumnTypes.includes(columnType) &&
                <></>
            }
            {
                choiceExtentionColumnTypes.includes(columnType) &&
                <ChoiceOptions
                    ref={choiceOptionsRef}
                    definitionOptions={_.isEmpty(extentionDefinition) ? [] : JSON.parse(extentionDefinition)}
                    onChange={value => onChange(JSON.stringify(value))}
                />
            }
        </div>
    );
});

Extention.displayName = "Extention";

export default Extention;