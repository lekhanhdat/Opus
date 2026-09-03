import "../../Less/PRM/barcodeTemplete.less";
export default class BarcodePreview extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
      
        };
    }

    componentInit() {

    }

    render(){
        let selectedAreaBName = this.props.data.selectedAreaBName,
            selectedAreaCName = this.props.data.selectedAreaCName,
            selectedAreaDNames = this.props.data.selectedAreaDNames || [],
            selectedAreaEName = this.props.data.selectedAreaEName,
            selectedAreaFName = this.props.data.selectedAreaFName,
            uploadTemplateUrl = this.props.data.uploadTemplateUrl,
            //由于barcode template和barcode print样式有区别，type: （不传：barcode template，"1":barcode print）。
            isBorcodePrint = this.props.type == "1",
            previewContentRightClass = isBorcodePrint ?"preview-content-right barcodePrint-content-right-width" : "preview-content-right",
            previewLongColClass = isBorcodePrint ?"preview-long-col barcodePrint-long-col-width" : "preview-long-col",
            previewShortColLeftClass = isBorcodePrint ?"preview-short-col-left barcodePrint-short-col-width" : "preview-short-col-left",
            previewShortColRightClass = isBorcodePrint ?"preview-short-col-right barcodePrint-short-col-width" : "preview-short-col-right";

        return <div id="raBarcodepReview">
            <div className="preview-content">
                {
                    uploadTemplateUrl &&  <div className="pull-left">
                        <div className="preview-img" style={{"background":`url("${uploadTemplateUrl}")`}}></div>
                    </div>
                }
                {
                    !uploadTemplateUrl &&  <div className="preview-default-img fia-placeholder" style={{border:"none"}}></div>
                }
                <div className={previewContentRightClass}>
                    <div className="preview-barcord-column-content">
                        <div className={previewShortColLeftClass}>{selectedAreaBName}</div>
                        <div className={previewShortColRightClass}>{selectedAreaCName}</div>
                        <div className={previewLongColClass}>
                            <div className="preview-long-col-content">
                                {
                                    selectedAreaDNames.map((name,idx)=>{
                                        return  <div key={idx} className="preview-template-column-name">{name}</div>;
                                    })
                                }
                            </div>
                        </div>
                        <div className={previewShortColLeftClass}>{selectedAreaEName}</div>
                        <div className={previewShortColRightClass}>{selectedAreaFName}</div>
                    </div>
                    <div className="margin-top-s preview-barcord-img-content">
                        {this.props.barcodeImg}
                    </div>
                </div>
            </div>
        </div>
        ;
    }
}