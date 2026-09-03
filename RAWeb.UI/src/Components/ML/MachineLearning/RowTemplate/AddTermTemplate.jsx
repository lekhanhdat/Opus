import { NormalCell } from "../../../Common/TableTemplateCell";
// import { TrainingStatusType, TrainingStatus } from '../Config/Constains';

export default class Template extends R.TableRow {
    constructor(props) {
        super(props);
        // this.isReadyLeastTermCount = 200;
    }

    // getTrainingStatusInfo = (ActiveCount) => {
    //     if(ActiveCount < this.isReadyLeastTermCount){
    //         return TrainingStatus.get(TrainingStatusType.NotReady);
    //     }else{
    //         return TrainingStatus.get(TrainingStatusType.Ready);
    //     }
    // }

    render(Row, Cell) {
        let {
            Name,
            FullPath,
            Description
        } = this.props.rowData;

        return <Row>
            <NormalCell Cell={Cell} contentText={Name}/>
            <NormalCell Cell={Cell} contentText={FullPath}/>
            <NormalCell Cell={Cell} contentText={Description}/>
        </Row>;
    }
}