import React, {
    useImperativeHandle,
    forwardRef,
    useState,
} from "react";
import _ from "lodash";
import { DataSizeType, DataSizeTypeI18ns } from "../../../Analysis/Constants";

const maxValueFile = 99999999999;
const maxValueTime = 12000;
const ExclusionsComponent = ({ info, onChange }, ref) => {
    const onRangeChange = ( sizeOrTime, index, value, defaultValue ) => {
        const clonedInfo = _.cloneDeep(info);
        if (sizeOrTime === 'size'){
            clonedInfo.sizeRangeInfoes[index].generateEqual = clonedInfo.sizeRangeInfoes[index-1]?.lessThan;
            clonedInfo.sizeRangeInfoes[index].lessThan = _.isNil(value) || _.isEmpty(value + "") ? defaultValue : value;
            clonedInfo.sizeRangeInfoes[index].order = index + 1;
            onSizeRangeChange(clonedInfo.sizeRangeInfoes);
        }else if(sizeOrTime === 'time'){
            clonedInfo.dateRangeInfoes[index].unit = _.isNil(value) || _.isEmpty(value + "") ? defaultValue : value;
            clonedInfo.dateRangeInfoes[index].order = index + 1;
            onChange("dateRangeInfoes", clonedInfo.dateRangeInfoes);
        }
    };

    const [sizeLimitError, setSizeLimitError] = useState(false);
    const [timeLimitError, setTimeLimitError] = useState(false);
    const [sizeAtLeastError, setSizeAtLeastError] = useState(false);
    const [timeAtLeastError, setTimeAtLeastError] = useState(false);
    const [maxFileValueError, setMaxFileValueError] = useState(false);
    const [maxTimeValueError, setMaxTimeValueError] = useState(false);

    const displaySizeLimitError = () => {
        if (sizeLimitError){
            return <span className="reco-ac-required-input">{RMResx.RM_FA_Discovery_ConfigFilter_SizeLimitError}</span>;
        }
        return null;
    };

    const displaySizeAtLeastError = () => {
        if (sizeAtLeastError){
            return <span className="reco-ac-required-input">{RMResx.RM_FA_Discovery_ConfigFilter_SizeAtLeastError}</span>;
        }
        return null;
    };

    const displayTimeLimitError = () => {
        if (timeLimitError){
            return <span className="reco-ac-required-input">{RMResx.RM_FA_Discovery_ConfigFilter_TimeLimitError}</span>;
        }
        return null;
    };

    const displayTimeAtLeastError = () => {
        if (timeAtLeastError){
            return <span className="reco-ac-required-input">{RMResx.RM_FA_Discovery_ConfigFilter_TimeAtLeastError}</span>;
        }
        return null;
    };
    
    const displayMaxFileValueError = () => {
        if (maxFileValueError){
            return (
                <span className="reco-ac-required-input" tabIndex="0">
                    <$g.I18NProvider msg={RMResx.RM_FA_Discovery_ConfigFilter_MaxValueError}>
                        <span>{maxValueFile}</span>
                    </$g.I18NProvider>
                </span>
            );
        }
        return null;
    };

    const displayMaxTimeValueError = () => {
        if (maxTimeValueError){
            return (
                <span className="reco-ac-required-input" tabIndex="0">
                    <$g.I18NProvider msg={RMResx.RM_FA_Discovery_ConfigFilter_MaxValueError}>
                        <span>{maxValueTime}</span>
                    </$g.I18NProvider>
                </span>
            );    
        }
        return null;
    };

    useImperativeHandle(ref, () => ({
        onValidate: () => {
            return true;
        },
    }));

    const onSizeRangeChange = (sizeRangeInfoes) => {
        const clonedSizeRangeInfos = _.sortBy(_.cloneDeep(sizeRangeInfoes), item => item.order);
        let prevSizeRangeInfo = clonedSizeRangeInfos[0];
        prevSizeRangeInfo.order = 0;
        for(let i = 1; i < clonedSizeRangeInfos.length; i++) {
            const curSizeRangeInfo = clonedSizeRangeInfos[i];
            curSizeRangeInfo.order = i;
            curSizeRangeInfo.generateEqual = prevSizeRangeInfo.lessThan;
            prevSizeRangeInfo = curSizeRangeInfo;
        }        

        const clonedInfo = _.cloneDeep(info);
        clonedInfo.sizeRangeInfoes = clonedSizeRangeInfos;
        onChange("sizeRangeInfoes", clonedInfo.sizeRangeInfoes);
    }


    const handleSizeAddInput = () => {
        if(info.sizeRangeInfoes.length > 4){
            setSizeLimitError(true);
            return ;
        }
        if(info.sizeRangeInfoes.at(-1).lessThan >= maxValueFile){
            setMaxFileValueError(true);
            return ;
        }
        setMaxFileValueError(false);
        setSizeAtLeastError(false);
        const clonedInfo = _.cloneDeep(info);
        const len = clonedInfo.sizeRangeInfoes.length;
        clonedInfo.sizeRangeInfoes.push(
            {
                name : '',
                generateEqual : len === 0 ? 0 : clonedInfo.sizeRangeInfoes[len - 1].lessThan,
                lessThan : len === 0 ? 1 : clonedInfo.sizeRangeInfoes[len - 1].lessThan + 1,
                order : len
            }
        );
        onSizeRangeChange(clonedInfo.sizeRangeInfoes);
    };

    const handleSizeDeleteInput = () => {
        if(info.sizeRangeInfoes.length <= 1){
            setSizeAtLeastError(true);
            return ;
        }
        setSizeLimitError(false);
        const clonedInfo = _.cloneDeep(info);
        clonedInfo.sizeRangeInfoes.pop();
        onSizeRangeChange(clonedInfo.sizeRangeInfoes);
    };

    const handleTimeAddInput = () => {
        if(info.dateRangeInfoes.length > 9){
            setTimeLimitError(true);
            return ;
        }
        if(info.dateRangeInfoes.at(-1).unit >= maxValueTime){
            setMaxTimeValueError(true);
            return ;
        }
        setMaxTimeValueError(false);
        setTimeAtLeastError(false);
        const clonedInfo = _.cloneDeep(info);
        const len = clonedInfo.dateRangeInfoes.length;
        clonedInfo.dateRangeInfoes.push(
            {
                unit : len === 0 ? 1 : clonedInfo.dateRangeInfoes[len - 1].unit + 1,
                unitType : 1,
                order : len
            }
        );
        onChange("dateRangeInfoes", clonedInfo.dateRangeInfoes);
    };

    const handleTimeDeleteInput = () => {
        if(info.dateRangeInfoes.length <= 1){
            setTimeAtLeastError(true);
            return ;
        }
        setTimeLimitError(false);
        const clonedInfo = _.cloneDeep(info);
        clonedInfo.dateRangeInfoes.pop();
        onChange("dateRangeInfoes", clonedInfo.dateRangeInfoes);
    };

    function SizeRangeInputComponent({index, value, sizeRangeInfoes}){
        return (
            <>
                <div className="reco-input">
                    <R.Input 
                        key={new Date().getTime() + "_o"}
                        value={value}
                        type="number"
                        width={120}
                        min={index === 0 ? 1 : sizeRangeInfoes[index - 1].lessThan + 1}
                        max={index === sizeRangeInfoes.length - 1 ? 10 * 1024 * 1024 * 1024 : sizeRangeInfoes[index + 1].lessThan - 1}
                        onChange={(value) =>
                            onRangeChange('size', index, value, index === 0 ? 1 : sizeRangeInfoes[index - 1].lessThan + 1)
                        }
                    />
                    <span className="reco-input-desc">
                        {DataSizeTypeI18ns.get(DataSizeType.MB)}
                    </span>
                    <span className="reco-input-desc">
                        -
                    </span>
                </div>
            </>
        );
    }

    function ModifiedTimeRangeInputComponent({index, value, timeRangeInfoes}){
        return (
            <>
                <div className="reco-input">
                    <R.Input 
                        key={new Date().getTime() + "_o"}
                        value={value}
                        type="number"
                        width={120}
                        min={index === 0 ? 1 : timeRangeInfoes[index - 1].unit + 1}
                        max={index === timeRangeInfoes.length - 1 ? maxValueTime : timeRangeInfoes[index + 1].unit - 1}
                        onChange={(value) =>
                            onRangeChange('time', index, value, index === 0 ? 1 : timeRangeInfoes[index - 1].unit + 1)
                        }
                    />
                    <span className="reco-input-desc">
                        {RMResx.RM_FA_Discovery_ConfigFilter_Months}
                    </span>
                    <span className="reco-input-desc">
                        -
                    </span>
                </div>
            </>
        );
    }

    return (
        <div className="reco-analysis-configurator-exclusions-info">
            <section className="reco-ac-component-title-main">
                <span tabIndex="0">{RMResx.RM_FA_Discovery_JobPage_Exclusion_Title}</span>
            </section>
            <section style={{ marginBottom: 24 }}>
                <div className="reco-ac-component-title-secondary" tabIndex={0}>
                    {RMResx.RM_FA_Discovery_ConfigFilter_SizeRange}
                    <span className="reco-ac-required-input">*</span>
                </div>
                <div className="reco-input-container">
                    <div className="reco-input">
                        <R.Input
                            key={new Date().getTime() + "_o"}
                            value={0}
                            type="number"
                            width={120}
                            min={0}
                            readonly={true}
                        />
                        <span className="reco-input-desc">
                            {DataSizeTypeI18ns.get(DataSizeType.MB)}
                        </span>
                        <span className="reco-input-desc">
                            -
                        </span>
                    </div>
                    {
                        Array.from({ length: info?.sizeRangeInfoes.length }, (_, index) => (
                            <SizeRangeInputComponent index={index} value={info.sizeRangeInfoes[index]?.lessThan} key={index} sizeRangeInfoes={info.sizeRangeInfoes}/>
                        ))
                    }
                    <div className="reco-input">
                        <R.Input
                            key={new Date().getTime() + "_o"}
                            value={RMResx.RM_FA_Discovery_ConfigFilter_SizeMax}
                            type="text"
                            width={120}
                            min={0}
                            readonly={true}
                        />
                        {/* <span className="reco-input-desc">
                            {DataSizeTypeI18ns.get(DataSizeType.MB)}
                        </span> */}
                    </div>
                    <div className="reco-button-container">
                        <R.Button className="reco-range-button" text={RMResx.RM_FA_Discovery_ConfigButton_AddRange} icon="fia-plus" type="button" classify="blank" disabled="false" onClick={handleSizeAddInput} />
                        <R.Button className="reco-range-button" text={RMResx.RM_FA_Discovery_ConfigButton_DeleteRange} icon="fia-close" type="button" classify="blank" disabled="false" onClick={handleSizeDeleteInput} />
                    </div>
                    {displaySizeAtLeastError()}
                    {displaySizeLimitError()}
                    {displayMaxFileValueError()}
                </div>
            </section>

            <section style={{ marginBottom: 24 }}>
                <div className="reco-ac-component-title-secondary"  tabIndex={0}>
                    {RMResx.RM_FA_Discovery_ConfigFilter_TimeRange}
                    <span className="reco-ac-required-input">*</span>
                </div>
                <div className="reco-input-container">
                    <div className="reco-input">
                        <R.Input
                            key={new Date().getTime() + "_o"}
                            value={RMResx.RM_FA_Discovery_ConfigFilter_TimeCurrent}
                            type="text"
                            width={120}
                            min={0}
                            readonly={true}
                        />
                        <span className="reco-input-desc">
                            -
                        </span>
                    </div> 
                    {
                        Array.from({ length: info?.dateRangeInfoes.length }, (_, index) => (
                            <ModifiedTimeRangeInputComponent index={index} value={info.dateRangeInfoes[index]?.unit} key={index} timeRangeInfoes={info.dateRangeInfoes} />
                        ))
                    }
                    <div className="reco-input">
                        <R.Input
                            key={new Date().getTime() + "_o"}
                            value={RMResx.RM_FA_Discovery_ConfigFilter_TimeMax}
                            type="text"
                            width={120}
                            min={0}
                            readonly={true}
                        />
                    </div>
                    <div className="reco-button-container">
                        <R.Button className="reco-range-button" text={RMResx.RM_FA_Discovery_ConfigButton_AddRange} icon="fia-plus" type="button" classify="blank" disabled="false" onClick={handleTimeAddInput} />
                        <R.Button className="reco-range-button" text={RMResx.RM_FA_Discovery_ConfigButton_DeleteRange} icon="fia-close" type="button" classify="blank" disabled="false" onClick={handleTimeDeleteInput} />
                    </div>
                    {displayTimeAtLeastError()}
                    {displayTimeLimitError()}
                    {displayMaxTimeValueError()}
                </div>
            </section>
        </div>
    );
};

export default forwardRef(ExclusionsComponent);
