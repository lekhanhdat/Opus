﻿import { Link } from 'react-router-dom';
import GainsightUtil from '../../Utilities/GainsightUtil';

class SiteMap extends React.Component {
    constructor(props) {
        super(props);
        this.setPageTitle();
    }

    componentDidMount() {
        this.showToolTip(this.props.data);
        //this.addAnalyticsForPage();
        setTimeout(() => {
            const pageIndex = document.querySelector(".reco-layout-content");
            pageIndex.focus();
            const modal = document.querySelector(".aui-dialog-modal");
            if(modal) {
                modal.focus();
            }
        }, 0);
    }

    componentDidUpdate() {
        this.showToolTip(this.props.data);
    }

    addAnalyticsForPage = () => {
        GainsightUtil.PageSimple();
    }

    showToolTip(data) {
        for (let index in data) {
            if ($('#breadcrumb' + index).width() < 310) {
                $('#breadcrumb' + index).children().removeAttr('title');
            }
        }
    }

    setPageTitle() {
        let data = this.props.data;
        if (data && data.length > 0) {
            let pageLink = data[data.length - 1];
            if (pageLink && pageLink.text) {
                window.document.title = pageLink.text;
            }
        }
    }
    onClickLink(item) {
        if (this.props.onClickLink) {
            this.props.onClickLink(item);
        }
    }

    getMapData(){
        let mapData = !this.props.data ? [] : RM.deepcopy(this.props.data);
        if(mapData.length > 0){ 
            for(let index in mapData){
                if(mapData.length - 1 == index){
                    mapData[index].url = false; 
                }
            }
        }
        return mapData;
    }

    render() {
        let mapData = this.getMapData();
        let mapChildren = this.props.children;
        return <React.Fragment>
            {
                mapData.length > 0 &&
                <div id="rmTopNav">
                    <div id="rmBreadcrumb">
                        <R.Breadcrumb items={mapData} />
                    </div>
                    <div id="rmTopRibbon">
                        {mapChildren}
                    </div>
                </div>
            }
        </React.Fragment>;
    }
}

export { SiteMap };