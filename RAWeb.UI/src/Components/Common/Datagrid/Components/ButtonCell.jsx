import { Component } from 'react';
import { GridCellButtonType } from "../../../../Constants/Constants";

class ButtonCell extends Component {
    constructor(props) {
        super(props);
        this.state = {};
        this.initBinding();

    }
    initBinding() {
        const eventsArr = ['editClick'];
        eventsArr.forEach((ev) => {
            this[ev] = this[ev].bind(this);
        });
    }
    componentDidMount() {

    }

    editClick() {
        this.props.editClick();
    }

    renderBtns() {
        let btns = this.props.buttons,
            rowData = this.props.rowData;
        if (btns && btns.length > 0) {
            return btns.map((btn, idx) => {
                if (!btn.isShow || btn.isShow(rowData)) {
                    let disabled = false;
                    if (btn.isDisabled) {
                        disabled = btn.isDisabled(rowData);
                    }

                    let btnType = btn.buttonType || GridCellButtonType.IconLink;
                    switch (btnType) {
                        case GridCellButtonType.IconLink:
                            var btnProps = Object.assign({}, btn.props),
                                btnClick = btnProps.onClick;
                            if (btnClick) {
                                btnProps.onClick = (e) => btnClick(rowData);
                            }
                            return <$g.IconButton
                                key={idx}
                                disabled={disabled}
                                {...btnProps}
                            />;
                        case GridCellButtonType.Switch:
                            var switchProps = Object.assign({}, btn.props),
                                switchChange = switchProps.onChange;
                            if (switchChange) {
                                switchProps.onChange = (checked, e) => switchChange(checked, e, rowData);
                            }
                            return <R.SwitchButton
                                key={idx}
                                disabled={disabled}
                                checked={btn.isChecked(rowData)}
                                {...switchProps}
                            />;
                        default:
                            return null;
                    }
                    
                    
                } else {
                    return null;
                }
            });
        } else {
            return null;
        }
    }
    render() {
        return <div className="ra-grid-cell-buttons">
            {this.renderBtns()}
        </div>;
    }
}

ButtonCell.type = 'GridRow';

export default ButtonCell;