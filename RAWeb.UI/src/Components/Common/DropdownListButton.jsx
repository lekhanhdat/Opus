import { Component } from "react";
import '../../Less/Common/DropdownListButton.less';

class DropdownListButton extends Component {
    constructor(props) {
        super(props);
        this.state = {
            show: props.show,
            buttonText: props.buttonText,
            dropdownMenuOnShow: false,
            items: this.props.items
        };
    }
    componentDidMount() {
        document.addEventListener('click', this.hideDropDownMenu);
    }

    componentWillUnmount() {
        document.removeEventListener('click', this.hideDropDownMenu);
    }

    hideDropDownMenu = (e) => {
        this.setState({
            dropdownMenuOnShow: false,
        });
    }

    showDropdownMenu = (e) => {
        e.nativeEvent.stopImmediatePropagation();
        this.setState({ dropdownMenuOnShow: true });
    }
    render() {
        return (
            <div className="dropdown-buttom-container" onClick={this.showDropdownMenu}>
                <div>
                    <button className="ra-iconbtn ra-dropdown-buttom" type="button">
                        <span className={this.props.iconClass}></span>
                        <span className="ra-iconbtn-text">{this.props.buttonText}</span>
                    </button>
                    <div className="ra-dropdown-list">
                        <div className="arrow-down-img"></div>
                    </div>
                </div>
                <div className="dropdown_button_div" style={{ display: this.state.dropdownMenuOnShow ? "block" : "none" }}>
                    {this.state.items.map((item, index) =>
                        <div className={item.disabled ? "hold-button hold-disable-button" : "hold-button"} tabIndex="0" key={index} onClick={() => { if (!item.disabled) { item.func(); } }} >{item.text} </div>
                    )}
                </div>
            </div>
        );
    }
}

export { DropdownListButton };