import { TooptipByApiCellType } from "../../Constants/Constants";
export class NormalCell extends R.Component {
    constructor(props) {
        super(props);
        this.state ={};
    }
   
    render () { 
        let {Cell, contentText, tooltip, children} = this.props;
        return <Cell>
            <div className="text-overflow" data-tooltip data-tooltip-wrap="force" aria-label={tooltip || contentText} tabIndex="0">
                {children || contentText}
            </div>
        </Cell>;
    }
}

export class TooptipByApiCell extends R.Component {
    constructor(props) {
        super(props);
        this.state ={
            rowData: this.props.rowData
        };
    }

    UNSAFE_componentWillReceiveProps(nextProps){
        this.setState({rowData: nextProps.rowData});
    }

    getFullPathApiUrl(){
        let rowData = this.state.rowData;
        switch(this.props.cellType){
            case TooptipByApiCellType.Term:
                return `/api/TermManagementApi/GetTermWithPath/?termId=${rowData.TermId}`;
            case TooptipByApiCellType.HomeLocation:
                return `/api/PhysicalRecordApi/GetPhysicalObjectFullPathById/?id=${rowData.Id}`;
            case TooptipByApiCellType.PredictTerm:
                return `/api/TermManagementApi/GetTermWithPath/?termId=${rowData.PredictTermId}`;
            default:
        }
    }

    setFullPath(fullPathInfo){
        let rowData = this.state.rowData;
        switch(this.props.cellType){
            case TooptipByApiCellType.Term:
                rowData.IsTermShowFullPath = true;
                rowData.FullPath = JSON.parse(fullPathInfo).FullPath;
                break;
            case TooptipByApiCellType.HomeLocation:
                rowData.IsHomeLocationShowFullPath = true;
                rowData.FullPath = fullPathInfo;
                break;
            case TooptipByApiCellType.PredictTerm:
                rowData.IsPredictTermShowFullPath = true;
                rowData.FullPath = JSON.parse(fullPathInfo).FullPath;
                break;
            default:
        } 
    }

    getIsShowFullPath(){
        let rowData = this.state.rowData;
        switch(this.props.cellType){
            case TooptipByApiCellType.Term:
                return rowData.IsTermShowFullPath;
            case TooptipByApiCellType.HomeLocation:
                return rowData.IsHomeLocationShowFullPath;
            case TooptipByApiCellType.PredictTerm:
                return rowData.IsPredictTermShowFullPath;
            default:
        }  
    }

    showFullPath = () => {
        if(!this.getIsShowFullPath()){
            let option = {
                method: "GET",
                url: this.getFullPathApiUrl()
            };
            fetchUtility(option).then((result) => {
                this.setFullPath(result);
                this.forceUpdate();
            }).catch((e) => {
                $$.loading(false);
            });
        }
    };

    render () {
        let isShowFullPath = this.getIsShowFullPath();
        return <React.Fragment>
            {
                isShowFullPath && <div className="text-overflow" data-tooltip={true} aria-label={this.state.rowData.FullPath}>
                    {this.props.contentText}
                </div>
            }
            {
                !isShowFullPath && <div className="text-overflow" onMouseEnter={this.showFullPath}>
                    {this.props.contentText}
                </div>
            }
        </React.Fragment>;
    }
}

