import _ from "lodash";
import { forwardRef, useEffect, useImperativeHandle, useState } from "react";
import { TableTemplate } from "./TableTemplate";

const buildInColumns = [
    {
        header: RMResx.RM_JS_BCM_Explorer_ExoMoveToSP_TableCol_ExoCol,
        width: 250,
    },
    {
        headerTemplate: (
            <div>
                <span>{RMResx.RM_JS_BCM_Explorer_ExoMoveToSP_TableCol_SPCol}</span>
                <$g.Popover>{RMResx.RM_JS_BCM_Explorer_ExoMoveToSP_TableCol_SPColMsg}</$g.Popover>
            </div>
        ),
        width: 250,
    },
    {
        header: "",
        width: 50,
    }
];

const moveDataDefaultObj = {
    ExoColumn: "Cc",
    SPColumn: "",
};

const ExoMoveToSP = ({ moveToSPDataList }, ref) => {

    const [moveDataList, setMoveDataList] = useState([]);

    const [isNoData, setIsNoData] = useState(false);

    const [isTooLong, setIsTooLong] = useState(false);

    const [isEmpty, setIsEmpty] = useState(false);

    useEffect(() => {
        if (moveToSPDataList.length === 0) {
            setMoveDataList([_.cloneDeep(moveDataDefaultObj)]);
        } else {
            setMoveDataList(moveToSPDataList);
        }
    }, [moveToSPDataList]);

    useImperativeHandle(ref, () => ({
        getMoveToSPDataList: () => {
            return moveDataList;
        },
        isValid: () => {
            if (moveDataList.some(item => item.SPColumn === "")) {
                setIsEmpty(true);
                return false;
            }
            if (isNoData || isTooLong) {
                return false;
            }
            return true;
        }
    }));

    const onInitValid = () => {
        setIsTooLong(false);
        setIsNoData(false);
        setIsEmpty(false);
    };

    const onAddBtn = () => {
        onInitValid();
        let add = _.cloneDeep(moveDataDefaultObj);
        let cloneMoveDataList = _.cloneDeep(moveDataList);
        if (cloneMoveDataList.length >= 50) {
            setIsTooLong(true);
            return false;
        } else {
            cloneMoveDataList.push(add);
            setMoveDataList(cloneMoveDataList);
        }
    };

    const onRowEvent = (args) => {
        onInitValid();
        let rowData = args.rowData;
        let rowIndex = args.rowIndex;
        let cloneMoveDataList = _.cloneDeep(moveDataList);
        switch (args.type) {
            case 'setRowData':
                cloneMoveDataList[rowIndex] = rowData;
                if (cloneMoveDataList.some(item => item.SPColumn === "")) {
                    setIsEmpty(true);
                }
                break;
            case 'deleteData':
                cloneMoveDataList.splice(rowIndex, 1);
                if (cloneMoveDataList.length === 0) {
                    setIsNoData(true);
                }
                break;
            default:
                break;
        }
        setMoveDataList(cloneMoveDataList);
    };

    return <div>
        <div className="margin-bottom-m">
            <R.Table
                id="raExoMoveToSPTable"
                height={[101, 305]}
                columns={buildInColumns}
                rowTemplate={TableTemplate}
                items={moveDataList}
                onRowEvent={onRowEvent}
            />
        </div>
        <div>
            <$g.ValidationMsg show={isTooLong}>
                {RMResx.RM_JS_BCM_Explorer_ExoMoveToSP_Validation_MoreThan50}
            </$g.ValidationMsg>
            <$g.ValidationMsg show={!isTooLong && isNoData}>
                {RMResx.RM_JS_BCM_Explorer_ExoMoveToSP_Validation_NoData}
            </$g.ValidationMsg>
            <$g.ValidationMsg show={!isTooLong && !isNoData && isEmpty}>
                {RMResx.RM_JS_RDM_CreateRule_Validation_ConditionNoValue}
            </$g.ValidationMsg>
        </div>
        <div>
            <R.Button
                id="raAddBtn"
                classify="blank"
                icon="fia-plus"
                text={RMResx.RM_JS_BCM_Explorer_ExoMoveToSP_AddBtn}
                onClick={onAddBtn}
            />
        </div>
    </div>;
};

export default forwardRef(ExoMoveToSP);