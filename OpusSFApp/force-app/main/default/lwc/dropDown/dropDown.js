import { LightningElement, api } from 'lwc';

export default class Dropdown extends LightningElement {
    @api id;
    @api dropdownOpen;
    @api customClass;
    @api directionalHint = 'left'

    count = 1;
    renderedCallback() {
        if (!this.dropdownOpen) {
            this.count = 1;
        }
        this.syncWithParent(this.dropdownOpen);

        if (this.directionalHint === 'right') {
            if (this.refs.dropdownButton && this.refs.dropdownMenu) {
                const buttonWidth = this.refs.dropdownButton?.offsetWidth ?? 0;
                const menuWidth = this.refs.dropdownMenu?.offsetWidth ?? 0;
                const offset = buttonWidth - menuWidth;
                const dropdownMenu = this.template.querySelector(`.${this.dropdownMenuClass.split(' ').join('.')}`);
                if (dropdownMenu) {
                    dropdownMenu.style.transform = `translateX(${offset}px)`;
                }
            }
        }
    }


    syncWithParent(newValue) {
        const toggleChangeEvent = new CustomEvent('togglechange', {
            detail: { isOpen: newValue }
        });
        this.dispatchEvent(toggleChangeEvent);
    }

    handleToggleKeyDown(event) {
        if (event.key === 'Enter' || event.key === ' ') {
            event.preventDefault();

            this.toggleDropdown(event);
        }
    }

    toggleDropdown() {
        this.count = 0;
        this.dropdownOpen = !this.dropdownOpen;
    }

    handleKeyDown(event) {
        if (event.key === 'Enter' || event.key === ' ') {
            event.preventDefault();

            this.handleMenuClick(event);
        }
    }

    handleMenuClick(event) {
        event.stopPropagation();
    }

    handleClickOutside(event) {
        const dropdownButton = this.template.querySelector(`.dropdown-button-${this.id}`);
        const dropdownMenu = this.template.querySelector(`.dropdown-menu-${this.id}`);
        if (this.dropdownOpen && dropdownButton && dropdownMenu && this.count != 0) {
            if (!dropdownButton.contains(event.target) && !dropdownMenu.contains(event.target)) {
                this.dropdownOpen = false;
            }
        }
        this.count = 1;
    }

    connectedCallback() {
        document.addEventListener('click', this.handleClickOutside.bind(this));
    }

    disconnectedCallback() {
        document.removeEventListener('click', this.handleClickOutside.bind(this));
    }

    get dropdownMenuClass() {
        if (this.customClass == undefined) {
            return this.dropdownOpen ? `dropdown-menu-${this.id} dropdown-menu-default slds-show` : `dropdown-menu-${this.id} dropdown-menu-default slds-hide`;
        }
        return this.dropdownOpen ? `dropdown-menu-${this.id} ${this.customClass} slds-show` : `dropdown-menu-${this.id} ${this.customClass} slds-hide`;
    }

    get dropdownButtonClass() {
        return `dropdown-button-${this.id}`;
    }
}
