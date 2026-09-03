import _ from "lodash";
import { forwardRef, useImperativeHandle, useState } from "react";

const buildInColumns = [
    {
        header: RMResx.RM_AR_RC_TableCol_SourceSite,
        width: 240,
    },
    {
        header: RMResx.RM_AR_RC_TableCol_DestinationSite,
        width: 240,
    },
    {
        header: "",
        width: 100,
    }
];

const siteMappingDefaultObj = {
    SourceSiteUrl: "",
    TargetSiteUrl: "",
    IsShowDeleteBtn: false,
}

const AddMapping = ({}, ref) => {

    const [addMappingList, setAddMappingList] = useState([_.cloneDeep(siteMappingDefaultObj)]);

    const [isTooLong, setIsTooLong] = useState(false);

    const [isEmpty, setIsEmpty] = useState(false);

    useImperativeHandle(ref, () => ({
        getAddMapping: () => {
            return addMappingList;
        },
        isValid: () => {
            if (addMappingList.some(item => item.SourceSiteUrl === "" || item.TargetSiteUrl === "")) {
                setIsEmpty(true);
                return false;
            }
            return true;
        }
    }));

    const onInitValid = () => {
        setIsTooLong(false);
        setIsEmpty(false);
    };

    const onRowEvent = (args) => {
        onInitValid();
        let rowData = args.rowData;
        let rowIndex = args.rowIndex;
        let cloneAddMappingList = _.cloneDeep(addMappingList);
        switch (args.type) {
            case 'setRowData':
                cloneAddMappingList[rowIndex] = rowData;
                break;
            case 'deleteData':
                cloneAddMappingList.splice(rowIndex, 1);
                if (cloneAddMappingList.length > 1) {
                    cloneAddMappingList.forEach(item => item.IsShowDeleteBtn = true);
                } else {
                    cloneAddMappingList.forEach(item => item.IsShowDeleteBtn = false);
                }
                break;
            case 'addRowData':
                onInitValid();
                const add = _.cloneDeep(siteMappingDefaultObj);
                if (cloneAddMappingList.length >= 10) {
                    setIsTooLong(true);
                } else {
                    cloneAddMappingList.push(add);
                    if (cloneAddMappingList.length > 1) {
                        cloneAddMappingList.forEach(item => item.IsShowDeleteBtn = true);
                    } else {
                        cloneAddMappingList.forEach(item => item.IsShowDeleteBtn = false);
                    }
                }
                break;
            default:
                break;
        }
        setAddMappingList(cloneAddMappingList);
    };

    return <div>
        <div className="margin-bottom-m">
            <R.Table
                id="raAddMappingTable"
                columns={buildInColumns}
                rowTemplate={AddMappingTableTemplate}
                items={addMappingList}
                onRowEvent={onRowEvent}
            />
        </div>
        <div>
            <$g.ValidationMsg show={isTooLong}>
                {RMResx.RM_AR_RC_AddMapping_ErrorMsg}
            </$g.ValidationMsg>
            <$g.ValidationMsg show={!isTooLong && isEmpty}>
                {RMResx.RM_JS_RDM_CreateRule_Validation_ConditionNoValue}
            </$g.ValidationMsg>
        </div>
    </div>;
};

export default forwardRef(AddMapping);


class AddMappingTableTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    onChanged(args0, args1) {
        switch (args0) {
            case 'SourceColumn':
                this.props.rowData.SourceSiteUrl = args1;
                break;
            case 'TargetColumn':
                this.props.rowData.TargetSiteUrl = args1;
                break;
            default:
                break;
        }
        this.dispatch('setRowData');
    }

    removeData(args) {
        this.dispatch('deleteData');
    }

    addData(args) {
        this.dispatch('addRowData');
    }

    render(Row, Cell) {

        const rowData = this.props.rowData;

        return (
            <Row>
                <Cell>
                    <div className="flex ra-flex-align-top">
                        <div className="ra-move-cell">
                            <R.Input
                                id="raSourceSiteUrlIpt"
                                type="text"
                                maxlength={255}
                                value={rowData.SourceSiteUrl}
                                onChange={this.onChanged.bind(this, "SourceColumn")}
                            />
                        </div>
                    </div>
                </Cell>
                <Cell>
                    <div className="flex ra-flex-align-top">
                        <div className="ra-move-cell">
                            <R.Input
                                id="raTargetSiteUrlIpt"
                                type="text"
                                maxlength={255}
                                value={rowData.TargetSiteUrl}
                                onChange={this.onChanged.bind(this, "TargetColumn")}
                            />
                        </div>
                    </div>
                </Cell>
                <Cell>
                    {rowData.IsShowDeleteBtn && <R.Button
                        type="bald"
                        icon="crm-criteria fia-close"
                        tooltip={RMResx.RM_JS_Common_Delete}
                        onClick={this.removeData.bind(this)}
                    />}
                    <R.Button
                        type="bald"
                        icon="crm-criteria fia-plus"
                        tooltip={RMResx.RM_JS_BCM_Explorer_MRR_Add_Button_Add}
                        onClick={this.addData.bind(this)}
                    />
                </Cell>
            </Row>
        );
    }
}