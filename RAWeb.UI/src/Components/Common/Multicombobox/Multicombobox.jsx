import {Component} from "react";
import {bindEvents} from "../../../Utilities/CommonUtil";
import "./filter.less";


class Multicombobox extends Component {
    constructor(props) {
        super(props);
        this.state = {
            filterData:props.filterData,
            isOpenPopup: false,
        };
        bindEvents(this, "openPopup", "okClick", "cancelClick");
    }

    openPopup() {
        this.setState({
            isOpenPopup: true
        });
    }

    okClick() {
        let checkedItem = this.FilterComponent.getData();
        this.props.getFilterData(checkedItem);
        this.setState({
            isOpenPopup: false
        });
    }

    cancelClick() {
        this.setState({
            isOpenPopup: false
        });
    }

    searchRender() {
        return <div className='ra-search-input' onClick={this.openPopup}>
            <span>Source:</span>
            <span>All</span>
            <div className="ra-search-img-dropdown-normal"
                tabIndex="0"
                role="button"
                aria-label="Expand Options Window">
            </div>
        </div>;
    }

    popupRender(item) {
        var FilterComponent = item.filterComponent;
        return <div className='ra-search-popup-container' style={{ display: (this.state.isOpenPopup) ? 'block' : 'none' }}>
            <div>
                <FilterComponent ref={r => this.FilterComponent = r} data={item} />
            </div>
            <div id="rm_control_btn">
                <R.Button color="blue" text={RMResx.RM_JS_Common_OK}
                    onClick={this.okClick} />
                <R.Button color="default" text={RMResx.RM_JS_Common_Cancel}
                    onClick={this.cancelClick} />
            </div>
        </div>;
    }

    render() {
        return <div id='ra-multicombobox'>
            {
                this.state.filterData.map((item, index)=>{
                    return <div key={index}>
                        {this.searchRender()}
                        {this.popupRender(item)}
                    </div>;
                })
            }
        </div>;
    }

}

export {Multicombobox};