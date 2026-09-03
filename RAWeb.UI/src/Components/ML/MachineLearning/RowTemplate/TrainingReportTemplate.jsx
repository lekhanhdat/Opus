import { NormalCell, TooptipByApiCell } from "../../../Common/TableTemplateCell";
import { SourceFlags, TooptipByApiCellType } from "../../../../Constants/Constants";
import { TrainingScopeStatusType, TrainingScopeStatus } from '../Config/Constains';
import { getSourceIcon } from "../../../../Utilities/CommonUtil";
import { IconText, LinkText } from '../../../Common/IconText';

export default class Template extends R.TableRow {
    constructor(props) {
        super(props);
    }

    getStatus(status){
        switch(status){
            case TrainingScopeStatusType.NotTrain:
                return { status: "Disabled", name: TrainingScopeStatus.get(status)};
            case TrainingScopeStatusType.Training:
                return { status: "Info", name: TrainingScopeStatus.get(status)};
            case TrainingScopeStatusType.Trained:
                return { status: "Success", name: TrainingScopeStatus.get(status)};
        }
    }
    
    render(Row, Cell) {
        let {
            FileName,
            PredictTermName,
            ChangeTermName,
            SourceFlag,
            FullPath,
            RecordsID,
            Type,
            DateString,
            Status
        } = this.props.rowData;
        return <Row>
            <NormalCell Cell={Cell} tooltip={FullPath}>
                <IconText icon={getSourceIcon(SourceFlag)}>
                    {SourceFlag == SourceFlags.Google ? (
                        <div tabIndex={0} className="ra-ellipsis">{FileName}</div>
                    ) : (
                        <LinkText href={FullPath} text={FileName}/>
                    )}
                </IconText>
            </NormalCell>
            <NormalCell Cell={Cell}>
                {<TooptipByApiCell 
                    cellType={TooptipByApiCellType.PredictTerm} 
                    rowData={this.props.rowData}
                    contentText={PredictTermName}
                />}
            </NormalCell>
            <NormalCell Cell={Cell}>
                {<TooptipByApiCell 
                    cellType={TooptipByApiCellType.Term} 
                    rowData={this.props.rowData}
                    contentText={ChangeTermName}
                />}
            </NormalCell>
            <NormalCell Cell={Cell}>
                {RecordsID}
            </NormalCell>
            <NormalCell Cell={Cell}>
                {Status}
            </NormalCell>
            <NormalCell Cell={Cell}>
                {Type}
            </NormalCell>
            <NormalCell Cell={Cell}>
                {DateString}
            </NormalCell>
        </Row>; 
    }
}