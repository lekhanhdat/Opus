import { Component } from "react";
import PropTypes from "prop-types";
import { bindEvents } from "../../Utilities/CommonUtil";

const allPeoplePickerInputs = {};
$("body").on("blur", ".ra-peoplePicker .aui-richcombobox-input", function (e) {
    let ppId = $(this).parents(".ra-peoplePicker").attr("id");
    let inputRef = allPeoplePickerInputs[ppId];
    let inputId = inputRef.element.id;
    if (inputRef) {
        setTimeout(() => {
            if ($(document.activeElement).parents(`#${inputId}`).length == 0 && !$(document.activeElement).attr('id')) {
                if ($(".ra-peoplePicker").length != 0) {
                    inputRef.clearInput();
                }
            }
        }, 100);
    }
});

let peoplePickerIdIndex = 0;
export default class PeoplePicker extends Component {
    constructor(props) {
        super(props);
        this.state = {
            items: props.items || [],
        };
        bindEvents(this, "selectionChanged", "onSearch", "onFocusChange", "onSearchByDefault", "onSearchBySpecify");
        this.peoplePickerId = "raPeoplePicker_" + peoplePickerIdIndex++;
    }

    UNSAFE_componentWillReceiveProps(nextProps) {
        if (JSON.stringify(nextProps.items) != JSON.stringify(this.props.items)) {
            this.setState({ items: nextProps.items });
        }
    }

    componentDidMount() {
        allPeoplePickerInputs[this.peoplePickerId] = this.refPicker;
    }

    selectionChanged(args) {
        let selections = RM.deepcopy(args.newValue);
        if (!args.oldValue && selections.length == 0) {
            return;
        }
        for (let item of selections) {
            if (item.OnlyId) {
                delete item.OnlyId;
            }
        }
        this.props.selectionChanged(selections);
    }

    onSearchByDefault(args) {
        let searchValue = args.key;
        let urlData = `/api/BCMCommonSettingApi/SearchAADUsers?tenantId=&key=${searchValue}&onlyFromRecord=${this.props.onlyFromRecord}`;
        let option = {
            url: urlData,
            method: "get"
        };
        if (searchValue) {
            return fetchUtility(option).then((res) => {
                let users = RM.deepcopy(res.Users);
                return this.addOnlyIdToItems(users);
            }).catch((e) => {

            });
        }
    }

    onSearchBySpecify(args) {
        let searchValue = args.key;
        let appProfileId = this.props.getSpecifyAppProfileId();
        let urlData = `/api/BCMCommonSettingApi/SearchAADUsersByApp?key=${searchValue}&appProfileId=${appProfileId}`;
        let option = {
            url: urlData,
            method: "get"
        };
        if (searchValue) {
            return fetchUtility(option).then((res) => {
                let users = RM.deepcopy(res.Users);
                return this.addOnlyIdToItems(users);
            }).catch((e) => {

            });
        }
    }

    onSearchByAadUser(args) {
        let searchValue = args.key;
        
        let urlData = `/api/BCMCommonSettingApi/SearchAADUsers4FSConnection?tenantId=&key=${searchValue}`;
        let option = {
            url: urlData,
            method: "get"
        };
        if (searchValue) {
            return fetchUtility(option).then((res) => {
                let users = RM.deepcopy(res.Users);
                return this.addFSConnectionIdToItems(users);
            }).catch((e) => {

            });
        }
    }

    onSearchUsersByPermissionScope(args) {
        let searchValue = args.key;
        let urlData = `/api/CPApi/SearchUsersByPermissionScope?keyword=${searchValue}`;
        let option = {
            url: urlData,
            method: "get"
        };
        if (searchValue) {
            return fetchUtility(option).then((res) => {
                let users = RM.deepcopy(res.Users);
                return this.addFSConnectionIdToItems(users);
            }).catch((e) => {

            });
        }
    }
 
    onSearch(args) {
        if(this.props.specifyAppProfile) {
            return this.onSearchBySpecify(args);
        }
        if(this.props.onlyIncludeAAdUser) {
            return this.onSearchByAadUser(args);
        }

        if(this.props.searchUsersByPermissionScope) {
            return this.onSearchUsersByPermissionScope(args);
        }

        return this.onSearchByDefault(args);
    }

    addOnlyIdToItems(users) {
        for (let item of users) {
            item.OnlyId = item.UserId || item.Id;
        }
        return users;
    }

    addFSConnectionIdToItems(users) {
        for (let item of users) {
            item.OnlyId = item.Id;
        }
        return users;
    }

    doMatch = (args) =>{
        let items = [];
        for (let value of args.list) {
            let item = {};
            item.invalid = false; 
            item.Checked = true;
            item.DisplayName = value;
            item.OnlyId = value;
            items.push(item);
        }
        return items;
    }

    render() {
        let users = this.props.onlyIncludeAAdUser ? this.addFSConnectionIdToItems(this.state.items) : this.addOnlyIdToItems(this.state.items);
        if (users && users.length == 0 && this.refPicker) {
            this.refPicker.clear();
        }
        let doMatch = this.props.isAllowCustomizeUser ? { doMatch: this.doMatch } : {};
        let placeholder = this.props.isAllowCustomizeUser ? 
            RMResx.RM_Common_CustomizePeoplePicker_Watermark : 
            RMResx.RM_Common_PeoplePicker_Watermark;
        
        let iconProp = {};
        const isStringIcon = typeof this.props.icon === 'string' && this.props.icon !== "false" && this.props.icon.trim() !== "";
        const isObjectIcon = typeof this.props.icon === 'object' && this.props.icon !== null;

        if (isStringIcon || isObjectIcon) {
            iconProp.icon = this.props.icon;
        }

        return <div className="ra-peoplePicker" id={this.peoplePickerId}>
            <R.RichCombobox
                asyncSearch
                ref={r => this.refPicker = r}
                searchMinChars={3}
                items={users}
                value={users}
                id={this.props.id}
                {...iconProp}
                width={this.props.width}
                height={this.props.height}
                disabled={this.props.disabled}
                singleMode={this.props.singleMode}
                searchPlaceholder={placeholder}
                tooltipField="UserPrincipalName"
                textField="DisplayName"
                valueField="OnlyId"
                checkedField="Checked"
                aria={{ ariaLabel: this.props.ariaId }}
                doLoad={this.onSearch}
                onChange={this.selectionChanged}
                {...doMatch}
            />
        </div>;
    }
}

PeoplePicker.propTypes = { 
    items: PropTypes.array,
    disabled: PropTypes.bool,
    singleMode: PropTypes.bool,
    selectionChanged: PropTypes.func,
    onlyFromRecord: PropTypes.bool,
    id: PropTypes.string,
    isAllowCustomizeUser: PropTypes.bool,
    specifyAppProfile: PropTypes.bool,
    getSpecifyAppProfileId: PropTypes.func,
    icon: PropTypes.oneOfType([
        PropTypes.string,
        PropTypes.object,
    ]),
    onlyIncludeAAdUser: PropTypes.bool,

};
PeoplePicker.defaultProps = {
    items: [],
    disabled: false,
    singleMode: false,
    width: "300",
    onlyFromRecord: false,
    id: null,
    isAllowCustomizeUser: false,
    icon: null,
    specifyAppProfile: false,
    onlyIncludeAAdUser: false,
};