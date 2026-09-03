"use strict";

/******/ var __webpack_modules__ = ({

/***/ "./src/core/element/text-contents.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   textContents: () => (/* binding */ textContents)
/* harmony export */ });
function traverseTextNode(acc, node, query, trim, applyQuery = true) {
  const matchQuery = () => !applyQuery || !query || query === '*' || node.matches(query);
  switch (node?.nodeType) {
    case Node.TEXT_NODE:
      {
        let content = node.nodeValue;
        content = content.replaceAll(/[^\S ]/gu, ''); // remove all non-space whitespace characters.
        if (trim === 'combine') {
          content = content.replaceAll(/(?:^\s+)|(?:\s+$)/g, ' ');
        } else if (trim) {
          content = content.trim();
        }
        if (content) acc.push(content);
        break;
      }
    case Node.ELEMENT_NODE:
      {
        const name = node.localName;
        if (name === 'svg') break;
        if (name === 'slot') {
          if (matchQuery()) {
            const assigned = node.assignedNodes({
              flatten: true
            });
            assigned.forEach(n => traverseTextNode(acc, n, query, trim));
          }
        } else if (node.shadowRoot) {
          node.shadowRoot.childNodes.forEach(n => traverseTextNode(acc, n, query, trim));
        } else if (matchQuery()) {
          node.childNodes.forEach(n => traverseTextNode(acc, n, query, trim));
        }
      }
  }
}
function textContents(element, query, trim) {
  const result = [];
  if (element) traverseTextNode(result, element, query, trim, false);
  return result;
}

/***/ },

/***/ "./src/features/accordion/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
const IDL = {
  /** @returns {number} */
  level: function () {
    const lv = this.getAttribute('level');
    return Number(lv);
  },
  /** @returns {boolean} */
  expanded: function () {
    const trigger = IDL.trigger.call(this);
    if (!trigger) return null; ///
    return trigger.getAttribute('aria-expanded') === 'true';
  },
  /** @todo @returns {Array<Element>} */
  group: function () {
    return [];
  },
  /** @returns {Element|null} */
  trigger: function () {
    return this.shadowRoot.querySelector('.trigger');
  },
  /** @returns {Element|null} */
  customHeader: function () {
    const slot = this.shadowRoot.querySelector('slot[name="header"]'),
      [header] = slot.assignedElements({
        flatten: true
      });
    return header || null;
  },
  /** @returns {Element|null} */
  body: function () {
    const slot = this.shadowRoot.querySelector('slot[name="body"]'),
      [body] = slot.assignedElements({
        flatten: true
      });
    return body || null;
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/avatar/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
const IDL = {
  /** @returns {string} */
  name: function () {
    const target = this.querySelector('.avatar');
    return target.ariaLabel;
  },
  /** @returns {string|null} */
  avatar: function () {
    const target = this.shadowRoot.querySelector('.avatar > img');
    return target?.src || null;
  },
  /** @returns {string|null} */
  identity: function () {
    const target = this.shadowRoot.querySelector('.avatar > .identity');
    return target?.textContent || null;
  },
  /** @returns {string|null} */
  status: function () {
    const box = this.querySelector('.avatar > .status');
    if (!box) return null;
    const [...classes] = box.classList;
    return classes.findLast(c => c !== 'status') || null;
  },
  /** @returns {string|null} */
  icon: function () {
    const target = this.shadowRoot.querySelector('.avatar > .icon');
    if (!target) return null;
    const [...classes] = target.classList;
    return classes.filter(c => c !== 'icon').join(' ');
  },
  /** @returns {Element|null} */
  groupItem: function (index) {
    const outer = this.shadowRoot.querySelector(`.outer[name="${index}"]`),
      outerTarget = outer?.assignedElements({
        flatten: true
      }) || [];
    if (outerTarget) return outerTarget[0] || null;
    const inner = this.shadowRoot.querySelector('#popup-body'),
      {
        from
      } = inner.dataset,
      list = inner.assignedElements({
        flatten: true
      });
    return list[index - from] || null;
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/badge/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
const IDL = {
  /** @returns {number} */
  value: function () {
    const element = this.shadowRoot.querySelector('.number');
    return Number(element?.dataset.count || 0);
  },
  /** @returns {string|null} */
  classify: function () {
    const content = this.shadowRoot.querySelector('.content');
    return content?.dataset.classify ?? null;
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/breadcrumb/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
const IDL = {
  /** @returns {Element|null} */
  item: function (index) {
    const list = this.querySelectorAll('.aui-breadcrumb-item');
    return list[index] || null;
  },
  /** @returns {Element|null} */
  more: function () {
    return this.querySelector('.aui-breadcrumb-more-button');
  },
  /** @returns {Element|null} */
  popItem: function (index) {
    const list = this.querySelectorAll('.aui-breadcrumb-popup .aui-breadcrumb-item');
    return list[index] || null;
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/button/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
const isButton = el => el && el.localName?.includes('button');
const IDL = {
  /** @private @returns {Element|null} */
  queryElement: function (element) {
    if (element?.nodeType === Node.ELEMENT_NODE) {
      if (isButton(element)) return element;
      for (const child of element.querySelectorAll('*')) {
        if (isButton(child)) return child;
      }
    }
    return null;
  },
  /** @returns {Element|null} */
  element: function () {
    if (!isButton(this)) return null;
    const scope = this.shadowRoot || this;
    return scope.querySelector('.button') || null;
  },
  /** @returns {string|null} */
  classify: function () {
    const element = IDL.element.call(this);
    return element?.dataset.classify ?? null;
  },
  /** @returns {string} */
  name: function () {
    const element = IDL.element.call(this);
    return element?.name;
  },
  /** @returns {boolean} */
  disabled: function () {
    const element = IDL.element.call(this);
    return element?.disabled || false;
  },
  /** @returns {string|null} */
  icon: function () {
    const element = IDL.element.call(this),
      className = 'button-icon-part';
    if (!element) return null;
    for (const icon of element.querySelectorAll(`.${className}`)) {
      if (icon.localName === 'slot') {
        const [el] = icon.assignedElements({
          flatten: true
        });
        if (el) return el.className;
      } else {
        const [...classList] = icon.classList;
        const list = Array.prototype.filter.call(icon.classList, c => c !== className);
        if (list.length) return list.join(' ');
      }
    }
    return null;
  },
  /** @returns {Promise<boolean>} */
  click: function () {
    const util = globalThis._Util,
      element = IDL.element.call(this);
    if (!element) return util.delay(false);
    if (element) element.click();
    return util.delay(true);
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/buttonfitbox/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__("./src/features/button/_test.idl.js");

const IDL = {
  /** @returns {Element} */
  more: function () {
    const target = this.shadowRoot.getElementById('more');
    return _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].element.call(target);
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/buttongroup/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__("./src/features/button/_test.idl.js");

const IDL = {
  /** @returns {Element|null} */
  defaultButton: function () {
    const target = this.shadowRoot.querySelector('.button-main');
    return target ? _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].element.call(target) : null;
  },
  /** @returns {Element} */
  triggerButton: function () {
    const target = this.shadowRoot.querySelector('.trigger');
    return _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].element.call(target);
  },
  /** @returns {Array<Element>} */
  buttonList: function () {
    return Array.prototype.flatMap.call(this.children, el => {
      return el.hasAttribute('slot') ? [] : _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].element.call(el);
    });
  },
  /** @returns {Element|null} */
  buttonByIndex: function (index) {
    const children = IDL.buttonList.call(this);
    return children[index] || null;
  },
  /** @returns {Element|null} */
  buttonById: function (id) {
    const target = this.querySelector(`:scope > #${id}`);
    return _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].element.call(target);
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/calendar/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__("./src/features/button/_test.idl.js");

const $ = function () {
  let jQuery; /// jq
  return function (...args) {
    if (!jQuery) {
      const frames = Array.from(window.top.frames);
      ({
        jQuery
      } = frames.find(f => f.jQuery));
      if (!jQuery) console.error('jQuery not found in any frame.');
    }
    return jQuery(...args);
  };
}();
const IDL = {
  format,
  /** @returns {Element|null} */
  defaultView: function (index) {
    return this.querySelectorAll('.aui-calendar-month');
  },
  /** @returns {Element|null} */
  monthView: function () {
    return this.querySelector('.aui-calendar-month');
  },
  /** @returns {Element|null} */
  monthItem: function (index) {
    const list = this.querySelectorAll('.aui-calendar-month .aui-basecalendar-body .aui-basecalendar-cell.inview');
    return list[index] || null;
  },
  /** @returns {Element|null} */
  yearView: function () {
    return this.querySelector('.aui-calendar-year');
  },
  /** @returns {Element|null} */
  yearItem: function (index) {
    const list = this.querySelectorAll('.aui-calendar-year .aui-calendar-cell');
    return list[index] || null;
  },
  /** @returns {Element|null} */
  decadeView: function () {
    return this.querySelector('.aui-calendar-decade');
  },
  /** @returns {Element|null} */
  decadeItem: function (index) {
    const list = this.querySelectorAll('.aui-calendar-decade .aui-calendar-cell.inview');
    return list[index] || null;
  },
  /** @returns {Element|null} */
  today: function () {
    return this.querySelector('.aui-calendar-today > button');
  },
  /** @returns {Element|null} */
  title: function (index = 0) {
    const list = this.querySelectorAll('.aui-calendar-title > button');
    return list[index] || null;
  },
  /** @returns {Record<'left'|'right', Element>} */
  pageButtons: function () {
    const [left, right] = this.querySelectorAll('.aui-calendar-arrow:not(.aui-calendar-cross-swipe)');
    return {
      left: _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].element.call(left),
      right: _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].element.call(right)
    };
  },
  /** @returns {number|null} */
  getValue: function () {
    const view = this.querySelector('.aui-calendar-body > :not(:empty)'),
      selected = view?.querySelector('[role="gridcell"][aria-selected="true"]');
    if (!selected) return null;
    return Number.parseInt(selected.dataset.start, 10) || null;
  },
  /** @returns {Array<number>} */
  getMultiValue: function () {
    const values = $(this).data('aui5Calendar').cache.selectedDate; /// jq
    if (!values?.length) return [];
    return values.map(v => Number(new Date(v)));
  },
  /** @returns {Record<'start'|'end', number>} */
  getRangeValue: function () {
    const {
      start,
      end
    } = $(this).data('aui5Calendar').cache.selectedDate; /// jq
    if (!start && !end) return {};
    return {
      start: start?.getTime(),
      end: end?.getTime()
    };
  }
};

/**
 * Intl implementation of `$$.date.format`.
 * @param {Date} date
 * @param {string} pattern
 * @returns {string}
 */
function format(date, pattern) {
  const FormatMapping = new Map([['dddd', {
    weekday: 'long'
  }], ['ddd', {
    weekday: 'short'
  }], ['yyyy', {
    year: 'numeric'
  }], ['yy', {
    year: '2-digit'
  }], ['MMMM', {
    month: 'long'
  }], ['MMM', {
    month: 'short'
  }], ['MM', {
    month: '2-digit'
  }], ['M', {
    month: 'numeric'
  }], ['dd', {
    day: '2-digit'
  }], ['d', {
    day: 'numeric'
  }], ['HH', {
    hour: '2-digit',
    hour12: false
  }], ['H', {
    hour: 'numeric',
    hour12: false
  }], ['hh', {
    hour: '2-digit',
    hour12: true
  }], ['h', {
    hour: 'numeric',
    hour12: true
  }], ['mm', {
    minute: '2-digit'
  }], ['m', {
    minute: 'numeric'
  }], ['ss', {
    second: '2-digit'
  }], ['s', {
    second: 'numeric'
  }], ['tt', {
    hour12: true
  }]]);
  const options = {},
    listMapping = {};
  let useHour12 = false;
  pattern.match(/[a-z]+/gi)?.forEach(part => {
    const option = FormatMapping.get(part);
    if (!option) return;
    Object.assign(options, option);
    if (part === 'tt') {
      useHour12 = true;
      listMapping['dayPeriod'] = part;
    } else {
      const [k] = Object.keys(option);
      if (k) listMapping[k] = part;
    }
  });
  if (useHour12) Object.assign(options, {
    hour: '2-digit',
    hour12: true
  });
  const formatter = new Intl.DateTimeFormat('en-US', options);
  let result = pattern;
  for (const {
    type,
    value
  } of formatter.formatToParts(date)) {
    if (type === 'dayPeriod' ? useHour12 : type !== 'literal') {
      result = result.replace(listMapping[type], value);
    }
  }
  return result;
}
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/carousel/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
const IDL = {
  /** @returns {number} */
  active: function () {
    const element = this.shadowRoot.querySelector("li[aria-selected='true']");
    const active = element.dataset.value;
    return Number(active);
  },
  /** @returns {string} */
  ctrlSide: function () {
    const element = this.shadowRoot.querySelector('aui-indicator');
    const className = element.className;
    return className;
  },
  /** @returns {boolean} */
  ordinal: function () {
    const element = this.shadowRoot.querySelector('.aui-dot');
    const bool = !element;
    return bool;
  },
  /** @returns {Record<'left'|'right', Element>} */
  switch: function () {
    const left = this.shadowRoot.querySelector('.faui-angle-left');
    const right = this.shadowRoot.querySelector('.faui-angle-right');
    return {
      left: left,
      right: right
    };
  },
  /** @returns {Array<Element>} */
  indicator: function () {
    const [...elements] = this.shadowRoot.querySelectorAll('li');
    return elements.flatMap(el => el.className === 'aui-dash' ? [] : el);
  },
  item: function (index) {
    return this.querySelector(`:scope>[slot="${index}"]`);
  },
  /** @returns {Element} */
  content: function () {
    const indicator = this.shadowRoot.querySelector('aui-indicator'),
      active = indicator.getAttribute('active');
    return IDL.item.call(this, active);
  },
  /** @returns {Element} */
  carouselBox: function () {
    return this.shadowRoot.querySelector('.content');
  },
  /** @returns {string|null} */
  classify: function () {
    const element = this.shadowRoot.querySelector('.aui-indicator');
    return element?.dataset.classify || null;
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/checkbox/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
const IDL = {
  /** @returns {Array<Element>} */
  checkboxList: function () {
    return this.querySelectorAll('aui-checkbox');
  },
  /** @returns {Array<Element>} */
  groupBox: function () {
    return this.querySelector('.aui-choice-group');
  },
  /** @returns {Element} */
  label: function () {
    return this.querySelector('.aui-checkbox');
  },
  /** @returns {Element} */
  input: function () {
    return this.querySelector('.aui-choice-input');
  },
  /** @returns {string} */
  checked: function () {
    const element = this.querySelector('.aui-choice-input');
    const checked = element.getAttribute('aria-checked');
    return checked;
  },
  /** @returns {string} */
  classifyColor: function () {
    const element = this.querySelector('.aui-choice-input');
    return window.getComputedStyle(element).getPropertyValue('border-color');
  },
  /** @returns {boolean} */
  solid: function () {
    const element = this.querySelector('.aui-choice-input-solid');
    return !!element;
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/colorpicker/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
const IDL = {
  // Palette values: 0~100 for S and V
  /** @returns {[S: number, V: number]} */
  getPaletteValue: function () {
    const {
        style
      } = this.shadowRoot.querySelector('.selector'),
      left = Number.parseInt(style.left, 10),
      top = Number.parseInt(style.top, 10);
    return [left, 100 - top];
  },
  /** @returns {void} */
  setPaletteValue: function (S, V) {
    const palette = this.shadowRoot.querySelector('.palette'),
      win = palette.ownerDocument.defaultView,
      rect = palette.getBoundingClientRect(),
      x = rect.left + S / 100 * rect.width,
      y = rect.top + (100 - V) / 100 * rect.height,
      args = {
        clientX: x,
        clientY: y,
        pageX: x + win.pageXOffset,
        pageY: y + win.pageYOffset
      };
    palette.dispatchEvent(new win.MouseEvent('mousedown', {
      ...args,
      button: 0,
      bubbles: true,
      composed: true
    }));
  },
  // Hue values: 0~360
  /** @returns {number} */
  getHue: function () {
    const slider = this.shadowRoot.querySelector('.slider-hue');
    return Number(slider.value);
  },
  /** @returns {void} */
  setHue: function (hue) {
    const slider = this.shadowRoot.querySelector('.slider-hue'),
      win = slider.ownerDocument.defaultView;
    slider.value = hue;
    slider.dispatchEvent(new win.InputEvent('input', {
      bubbles: true,
      composed: true
    }));
  },
  // Opacity values: 0~1
  /** @returns {number|null} */
  getOpacity: function () {
    const slider = this.shadowRoot.querySelector('.slider-opacity');
    if (!slider) return null;
    return Number(slider.value);
  },
  /** @returns {void} */
  setOpacity: function (opacity) {
    const slider = this.shadowRoot.querySelector('.slider-opacity');
    if (slider) {
      const win = slider.ownerDocument.defaultView;
      slider.value = opacity;
      slider.dispatchEvent(new win.InputEvent('input', {
        bubbles: true,
        composed: true
      }));
    }
  },
  /** @returns {"HEX" | "RGB" | void} */
  colorModel: function (name) {
    const model = this.shadowRoot.querySelector('.color-model > .combobox');
    if (name == null) return model.getValue();
    model.setValue(name);
  },
  /** @returns {HTMLInputElement} The main value input. */
  input: function () {
    return this.shadowRoot.querySelector('.color-model > input');
  },
  /** @returns {HTMLInputElement|void} The opacity input. */
  opacity: function () {
    return this.shadowRoot.querySelector('.opacity > input');
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/combobox/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _comboboxshell_test_idl_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__("./src/features/comboboxshell/_test.idl.js");
/* harmony import */ var _selection_test_old_idl_js__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__("./src/features/selection/_test.old.idl.js");


const {
  arias,
  clear,
  closeAll,
  content,
  disabled,
  placeholder,
  popup,
  popupContent,
  popupSize,
  size,
  tooltip,
  triggerButton
} = _comboboxshell_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"];
const IDL = {
  arias,
  clear,
  closeAll,
  content,
  disabled,
  placeholder,
  popup,
  popupContent,
  popupSize,
  size,
  tooltip,
  triggerButton
};
Object.assign(IDL, {
  /** @returns {Promise<void>} */
  setUIValue: async function (content) {
    if (IDL.disabled.call(this)) return Promise.reject();
    IDL.triggerButton.call(this).click();
    await _selection_test_old_idl_js__WEBPACK_IMPORTED_MODULE_1__["default"].setUIValue.call(IDL.selection.call(this), content);
    return Promise.resolve();
  },
  /** @returns {Element} */
  selection: function () {
    return this.querySelector('.aui-selection');
  }
});
for (const [key, value] of Object.entries(_selection_test_old_idl_js__WEBPACK_IMPORTED_MODULE_1__["default"])) {
  if (key === 'setUIValue') continue;
  IDL[key] = function (...args) {
    const selection = IDL.selection.call(this);
    return value.call(selection, ...args);
  };
}
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/comboboxshell/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__("./src/features/button/_test.idl.js");

function getElement(context) {
  const name = 'aui-combobox-shell';
  if (context.localName === name) return context;
  const scope = context.shadowRoot || context;
  return scope.querySelector(name);
}
/** Fallback for get assigned content element. */
function contentElement(context) {
  const element = getElement(context);
  if (!element) return null;
  const slot = element.shadowRoot.querySelector('#content'),
    assigned = slot.assignedElements({
      flatten: true
    });
  if (assigned?.length) return assigned[0];
  return IDL.input.call(element);
}
const IDL = {
  /** @returns {Element} */
  input: function () {
    const element = getElement(this);
    return element?.shadowRoot.querySelector('#input');
  },
  /** @returns {string|null} */
  content: function () {
    const util = globalThis._Util,
      element = contentElement(this);
    if (!element) return null;
    return element.nodeName === 'INPUT' ? element.value : util.textContent(element);
  },
  /** @returns {string|null} */
  placeholder: function () {
    const element = contentElement(this);
    if (!element) return null;
    if (element.nodeName === 'INPUT') {
      return element.matches(':placeholder-shown') ? element.placeholder : null;
    } else {
      const placeholder = element.querySelector('[data-placeholder="true"]');
      return placeholder?.checkVisibility() ? placeholder.textContent : null;
    }
  },
  /** @returns {boolean} */
  disabled: function () {
    const input = IDL.input.call(this);
    return input.disabled;
  },
  /** @returns {Element} */
  triggerButton: function () {
    const element = getElement(this);
    return element?.shadowRoot.querySelector('.trigger');
  },
  /** @returns {Element|null} */
  clear: function (checkDisabled = true, checkVisible = false) {
    const element = getElement(this),
      clearButton = element?.shadowRoot.querySelector('.clear > *');
    if (!clearButton || checkDisabled && _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].disabled.call(clearButton)) return null;
    const box = element.shadowRoot.querySelector('.box');
    box.classList.add('focus-visible'); // add .focus-visible for auto-test
    if (!checkVisible || clearButton.checkVisibility()) return _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].element.call(clearButton);
    console.warn('clear button invisible');
    return null;
  },
  /** @returns {string|null} */
  icon: function () {
    const scope = getElement(this)?.shadowRoot;
    if (!scope) return null;
    const [...iconButtons] = scope.getElementById('icon-box').querySelectorAll('.icon:not(#clear)'),
      iconButton = iconButtons.find(btn => btn.checkVisibility());
    return _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].icon.call(iconButton);
  },
  /** @returns {Record<"width" | "height", string>} */
  size: function () {
    const trigger = IDL.triggerButton.call(this),
      {
        width,
        height
      } = trigger.style,
      {
        width: computedWidth,
        height: computedHeight
      } = getComputedStyle(trigger);
    return {
      width: width || computedWidth,
      height: height || computedHeight
    };
  },
  /** @returns {[type: "true"|"ifneed"|false, content: string|null]} */
  tooltip: function () {
    const element = getElement(this),
      target = element?.shadowRoot.querySelector('[data-tooltip]'),
      type = target?.dataset.tooltip || false;
    if (type !== 'true') return [type, null];
    const content = element.shadowRoot.querySelector('#tooltip');
    return [type, content?.textContent || ''];
  },
  /** @returns {Record<string, string>} */
  arias: function () {
    const util = globalThis._Util,
      input = IDL.input.call(this);
    return util.arias(input);
  },
  /** @returns {Element} */
  popup: function () {
    const element = getElement(this),
      pop = element?.shadowRoot.querySelector('#popup');
    return pop.shadowRoot.querySelector('.pop');
  },
  /** @returns {Record<string, string>} */
  popupSize: function () {
    const popup = IDL.popup.call(this);
    return popup.style;
  },
  /** @returns {Element} */
  popupContent: function () {
    const element = getElement(this),
      [...children] = element?.children || [];
    return children.find(child => !child.hasAttribute('slot'));
  },
  /** @returns {void} */
  closeAll: function () {
    const win = this.ownerDocument.defaultView;
    win.$$.closeAll();
  },
  /** @returns {Record<string, Element>} */
  buttons: function () {
    const element = getElement(this),
      [...children] = element?.children || [];
    const data = {};
    for (const child of children) {
      if (child.getAttribute('slot') === 'buttons') {
        const button = _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].queryElement(child);
        if (button) {
          const name = _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].name.call(button);
          data[name] = _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].element.call(button);
        }
      }
    }
    return data;
  },
  /** @returns {Element|null} */
  button: function (name) {
    const buttons = IDL.buttons.call(this);
    return buttons[name] || null;
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/comboboxshell/_test.old.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__("./src/features/button/_test.idl.js");
/* harmony import */ var _icon_test_idl_js__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__("./src/features/icon/_test.idl.js");


const IDL = {
  /** @returns {Element|null} */
  input: function (editable = true) {
    const query = '.aui-comboboxshell-input:not([aria-hidden="true"])';
    const input = this.querySelector(editable ? `${query}:read-write` : query);
    return input || null;
  },
  /** @returns {string|null} */
  content: function () {
    if (this.classList.contains('aui-combobox-none')) return null;
    const content = this.querySelector('.aui-comboboxshell-content');
    if (content) return content.textContent;
    const input = IDL.input.call(this, false);
    return input?.value || null;
  },
  /** @returns {string|null} */
  placeholder: function () {
    if (this.classList.contains('aui-combobox-none')) {
      const content = this.querySelector('.aui-comboboxshell-content');
      if (content) return content.textContent;
    }
    const input = IDL.input.call(this, false);
    return input && !input.value && input.placeholder || null;
  },
  /** @returns {boolean} */
  disabled: function () {
    return this.getAttribute('aria-disabled') === 'true';
  },
  /** @returns {Element} */
  triggerButton: function () {
    return this.querySelector('.aui-comboboxshell-icon');
  },
  /** @returns {Element|null} */
  clear: function (checkDisabled = true) {
    const {
      classList
    } = this;
    if (!classList.contains('aui-comboboxshell-clear') || checkDisabled && classList.contains('aui-comboboxshell-clear-disabled')) return null;
    const clearButton = this.querySelector('.aui-comboboxshell-icon-clear');
    if (!clearButton) return null;
    classList.add('focus-visible'); // add .focus-visible for auto-test (Chrome not support JS focusVisible option)
    return clearButton.checkVisibility({
      visibilityProperty: true
    }) ? clearButton : null;
  },
  /** @returns {string|null} */
  icon: function () {
    const box = this.querySelector('.aui-comboboxshell-icon-box'),
      icon = box?.querySelector(':scope > .aui-comboboxshell-icon:not(.aui-comboboxshell-icon-clear)');
    if (!icon) return null;
    return _icon_test_idl_js__WEBPACK_IMPORTED_MODULE_1__["default"].icon.call(icon);
  },
  /** @returns {Record<"width" | "height", string>} */
  size: function () {
    const styleDeclaration = getComputedStyle(this);
    return styleDeclaration;
  },
  /** @returns {[type: "true"|"ifneed"|false, content: string|null]} */
  tooltip: function () {
    const type = this.dataset.tooltip || false;
    if (type !== 'true') return [type, null];
    const content = this.getAttribute('aria-label');
    return [type, content || ''];
  },
  /** @returns {Record<string, string>} */
  arias: function () {
    const util = globalThis._Util;
    return util.arias(this);
  },
  /** @returns {Element} */
  popup: function () {
    const [id] = this.getAttribute('aria-controls').split(' '),
      doc = this.ownerDocument;
    return doc.getElementById(id);
  },
  /** @returns {Record<string, string>} */
  popupSize: function () {
    const popup = IDL.popup.call(this);
    return popup.style;
  },
  /** @returns {Element} */
  popupContent: function () {
    const popup = IDL.popup.call(this);
    return popup.querySelector('.aui-comboboxshell-content');
  },
  /** @returns {void} */
  closeAll: function () {
    const win = this.ownerDocument.defaultView;
    win.$$.closeAll();
  },
  /** @returns {Record<string, Element>} */
  buttons: function () {
    const popup = IDL.popup.call(this),
      buttonQuery = ':is(aui-button,aui-button-lite)',
      header = popup.querySelector('.aui-comboboxshell-header');
    let buttons;
    if (header?.checkVisibility()) {
      buttons = popup.querySelectorAll(`.aui-popup-header .aui-popup-buttons ${buttonQuery}`);
    } else {
      buttons = popup.querySelectorAll(`.aui-comboboxshell-footer ${buttonQuery}`);
    }
    const data = {};
    for (const button of buttons) {
      const name = _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].name.call(button);
      data[name] = _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].element.call(button);
    }
    return data;
  },
  /** @returns {Element|null} */
  button: function (name) {
    const buttons = IDL.buttons.call(this);
    return buttons[name] || null;
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/datepicker/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _rangepicker_test_idl_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__("./src/features/rangepicker/_test.idl.js");
/* harmony import */ var _timepicker_test_idl_js__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__("./src/features/timepicker/_test.idl.js");


const IDL = {
  ..._rangepicker_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"],
  /** @returns {Promise<void>} */
  setUIValue: function (content) {
    const input = IDL.input.call(this, false);
    if (!input || input.matches(':disabled')) return Promise.reject();
    content = String(content ?? '');
    input.value = content;
    input.dispatchEvent(new KeyboardEvent('keydown', {
      key: 'Enter',
      bubbles: true
    }));
    input.dispatchEvent(new KeyboardEvent('keydown', {
      key: 'Escape',
      bubbles: true
    }));
    return input.value === content ? Promise.resolve() : Promise.reject();
  },
  /** @returns {Element|null} */
  timepart: function () {
    const popup = IDL.popup.call(this);
    return popup.querySelector('aui-time-part');
  },
  /** @returns {Element|null} */
  hourItem: function (index) {
    const timepart = IDL.timepart.call(this);
    if (!timepart) return null;
    return _timepicker_test_idl_js__WEBPACK_IMPORTED_MODULE_1__["default"].hourItem.call(timepart, index);
  },
  /** @returns {Element|null} */
  minuteItem: function (index) {
    const timepart = IDL.timepart.call(this);
    if (!timepart) return null;
    return _timepicker_test_idl_js__WEBPACK_IMPORTED_MODULE_1__["default"].minuteItem.call(timepart, index);
  },
  /** @returns {Element|null} */
  periodItem: function (value) {
    const timepart = IDL.timepart.call(this);
    if (!timepart) return null;
    return _timepicker_test_idl_js__WEBPACK_IMPORTED_MODULE_1__["default"].periodItem.call(timepart, value);
  },
  /** @returns {Element|null} */
  today: function () {
    const popup = IDL.popup.call(this);
    return popup.querySelector('.aui-datepicker-today');
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/dialog/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__("./src/features/button/_test.idl.js");

const IDL = {
  /** @returns {boolean} */
  shown: function () {
    const dialog = IDL.element.call(this);
    return dialog.open;
  },
  /** @returns {Element|null} */
  element: function (checkVisible) {
    const name = 'aui-modal',
      modal = this.localName === name ? this : this.shadowRoot.querySelector(name),
      dialog = modal.shadowRoot.querySelector('dialog');
    return !checkVisible || dialog.open ? dialog : null;
  },
  /** @returns {Element} */
  close: function () {
    const dialog = IDL.element.call(this),
      action = dialog.querySelector('aui-button-lite[icon="faui-close"]');
    return _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].element.call(action);
  },
  /** @returns {Element} */
  maximum: function () {
    const dialog = IDL.element.call(this),
      action = dialog.querySelector('aui-button-lite[icon="faui-maximize"]');
    return _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].element.call(action);
  },
  /** @returns {Element} */
  minimum: function () {
    const dialog = IDL.element.call(this),
      action = dialog.querySelector('aui-button-lite[icon="faui-restore"]');
    return _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].element.call(action);
  },
  /** @returns {string} */
  header: function () {
    const dialog = IDL.element.call(this),
      header = dialog.shadowRoot.querySelector('slot[name="header"]'),
      elements = header.assignedElements({
        flatten: true
      });
    return elements.map(el => el.textContent).join(' ');
  },
  /** @returns {Array<Element>} */
  body: function () {
    const dialog = IDL.element.call(this),
      slot = dialog.querySelector('.body > slot'),
      elements = slot.assignedElements({
        flatten: true
      });
    return elements.filter(el => el.id !== 'body-placeholder');
  },
  /** @returns {Element} */
  main: function () {
    const dialog = IDL.element.call(this);
    return dialog.querySelector('.body');
  },
  /** @returns {Record<string, Element>} */
  buttons: function () {
    const dialog = IDL.element.call(this),
      slot = dialog.querySelector('.footer > slot[name="buttons"]'),
      elements = slot.assignedElements({
        flatten: true
      });
    const data = {};
    for (const el of elements) {
      const button = _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].queryElement(el);
      if (button) {
        const name = _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].name.call(button);
        data[name] = _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].element.call(button);
      }
    }
    return data;
  },
  /** @returns {Element|null} */
  backdrop: function () {
    const modal = this.shadowRoot.querySelector('aui-modal'),
      backdrop = modal.shadowRoot.querySelector('aui-backdrop');
    if (!backdrop) return null;
    return backdrop.shadowRoot.querySelector('.backdrop');
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/editor/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__("./src/features/button/_test.idl.js");

const IDL = {
  /** @returns {Promise<void>} */
  setUIValue: function (content) {
    const element = this.shadowRoot.querySelector('.editor');
    if (!element || element.matches('.disabled,.readonly')) return Promise.reject();
    const {
      promise,
      resolve,
      reject
    } = Promise.withResolvers();
    const setValue = () => {
      try {
        const {
            contentDocument: doc
          } = element.querySelector('iframe'),
          child = doc.createElement('p');
        child.textContent = String(content ?? '');
        doc.body.replaceChildren(child);
        doc.body.dispatchEvent(new Event('input', {
          bubbles: true
        }));
        resolve();
      } catch (e) {
        reject(e);
      }
    };
    if (element.querySelector('textarea')) {
      setValue();
    } else {
      element.focus({
        focusVisible: false,
        preventScroll: true
      });
      setTimeout(setValue, 500);
    }
    return promise;
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/ellipsis/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
const IDL = {
  /** @returns {number} */
  rows: function () {
    const element = this.shadowRoot.querySelector('slot');
    const str = window.getComputedStyle(element).getPropertyValue('--ellipsis-row');
    return str === '' ? null : Number(str);
  },
  /** @returns {void} */
  setValue: function (num) {
    this.setAttribute('row', num);
  },
  /** @returns {Element} */
  action: function () {
    const element = this.shadowRoot.querySelector('.text-end>button');
    return element;
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/eventcalendar/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__("./src/features/button/_test.idl.js");

const IDL = {
  /** @returns {Element} */
  today: function () {
    const action = this.querySelector('.aui-eventcalendar-today');
    return _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].element.call(action);
  },
  /** @returns {Record<'prev'|'next', Element>} */
  rangeAction: function (index) {
    const prev = this.querySelector('.aui-eventcalendar-left'),
      next = this.querySelector('.aui-eventcalendar-right');
    return {
      prev: _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].element.call(prev),
      next: _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].element.call(next)
    };
  },
  /** @returns {Element|null} */
  viewSwitcher: function () {
    return this.querySelector('.aui-eventcalendar-switchview');
  },
  /** @returns {Element} */
  rangePicker: function () {
    return this.querySelector('.title-date-box');
  },
  /** @returns {Element} */
  rangePickerCalendar: function () {
    const picker = IDL.rangePicker.call(this),
      popupId = picker.getAttribute('data-popup'),
      popup = this.ownerDocument.getElementById(popupId);
    return popup.querySelector('.aui-calendar');
  },
  /** @returns {Element|null} */
  agendaPanel: function () {
    const box = this.closest('.aui-eventcalendar-box');
    return box.querySelector('.aui-eventcalendar-panel');
  },
  /** @returns {string|null} */
  agendaPanelTitle: function () {
    const panel = IDL.agendaPanel.call(this);
    if (!panel) return null;
    return panel.querySelector('.aui-eventcalendar-panel-title > span').textContent;
  },
  /** @returns {Array<Element>} */
  agendaPanelItems: function () {
    const panel = IDL.agendaPanel.call(this),
      box = panel?.querySelector('.aui-agendaevents-container');
    if (!box) return null;
    return Array.from(box.children);
  },
  /** @returns {Record<'color'|'name'|'status'|'start'|'end', string|number>} */
  agendaItemInfo: function (agenda) {
    const {
      shadowRoot
    } = agenda;
    return {
      color: shadowRoot.querySelector('.classify').style.color,
      name: shadowRoot.querySelector('.name').textContent,
      status: shadowRoot.querySelector('.duration').getAttribute('data-status'),
      start: Number(shadowRoot.querySelector('.duration > [data-start]').getAttribute('data-start')),
      end: Number(shadowRoot.querySelector('.duration > [data-end]').getAttribute('data-end'))
    };
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/filepreview/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _dialog_test_idl_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__("./src/features/dialog/_test.idl.js");

const IDL = {
  /** @returns {Element} */
  files: function () {
    return this.shadowRoot.querySelector('.files');
  },
  /** @returns {Element} */
  activeElement: function () {
    return this.shadowRoot.querySelector('.files > .active');
  },
  /** @returns {Element} */
  previousElement: function () {
    const active = this.shadowRoot.querySelector('.files > .active'),
      parent = this.shadowRoot.querySelector('.files');
    let previous = active.previousSibling;
    if (!previous) {
      previous = parent.lastChild;
    }
    return previous;
  },
  /** @returns {Element} */
  nextElement: function () {
    const active = this.shadowRoot.querySelector('.files > .active'),
      parent = this.shadowRoot.querySelector('.files');
    let next = active.nextSibling;
    if (!next) {
      next = parent.firstChild;
    }
    return next;
  },
  /** @returns {Record<string, Element>} */
  actions: function () {
    const [fit, actualSize, zoomOut, zoomIn] = this.shadowRoot.querySelectorAll('[slot="actions"]');
    const close = _dialog_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].close.call(this);
    return {
      fit,
      actualSize,
      zoomOut,
      zoomIn,
      close
    };
  },
  /** @returns {Record<'left'|'right', Element>} */
  switch: function () {
    const switches = this.shadowRoot.querySelector('.pager'),
      [left, right] = switches.children;
    return {
      left,
      right
    };
  },
  /** @returns {Array<Element>} */
  thumbnail: function () {
    const [...elements] = this.shadowRoot.querySelector('.tabs').children;
    return elements;
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/icon/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
const IDL = {
  /** @returns {string} */
  icon: function () {
    const target = this.querySelector('span');
    return target.className;
  },
  /** @returns {boolean} */
  isLoading: function () {
    const target = this.querySelector('.aui-icon-loading');
    return target?.checkVisibility() || false;
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/imgcrop/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__("./src/features/button/_test.idl.js");

const IDL = {
  /** @returns {Element} */
  ok: function () {
    const target = this.shadowRoot.querySelector('#ok');
    return _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].element.call(target);
  },
  /** @returns {Element} */
  cancel: function () {
    const target = this.shadowRoot.querySelector('#cancel');
    return _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].element.call(target);
  },
  /** @returns {Element|null} */
  zoomSlider: function () {
    return this.shadowRoot.querySelector('#zoom-slider');
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/input/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
const IDL = {
  /** @returns {Promise<void>} */
  setUIValue: function (content) {
    const element = IDL.element.call(this);
    if (!element.matches(':read-write')) return Promise.reject();
    element.value = String(content ?? '');
    element.dispatchEvent(new Event('change', {
      bubbles: true
    }));
    return Promise.resolve();
  },
  /** @returns {void} */
  setValue: function (str) {
    this.setAttribute('value', str);
  },
  /** @returns {Element} */
  element: function () {
    const scope = this.shadowRoot || this;
    return scope.querySelector('.aui-input-target');
  },
  /** @returns {string|null} */
  classify: function () {
    const element = IDL.element.call(this);
    return element?.dataset.classify ?? null;
  },
  /** @returns {Element} */
  upIcon: function () {
    return this.querySelector('.faui-angle-up-s');
  },
  /** @returns {Element} */
  downIcon: function () {
    return this.querySelector('.faui-angle-down-s');
  },
  /** @returns {Element} */
  reveal: function () {
    return this.querySelector('.faui-eye-slash');
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/loading/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
const IDL = {
  /**
   * @this {Document|Element} Context document (for global loading), or loading element(container element).
   * @returns {Element|null}
   */
  element: function () {
    const {
        nodeType
      } = this || {},
      name = 'aui-loading';
    if (nodeType === Node.DOCUMENT_NODE) return this.body.querySelector(`:scope > ${name}`);
    if (nodeType === Node.ELEMENT_NODE) {
      if (this.localName === name) return this;
      return this.querySelector(name);
    }
    return null;
  },
  /** @returns {boolean} */
  shown: function () {
    const element = IDL.element.call(this),
      target = element.shadowRoot.querySelector('.loading');
    return target.checkVisibility();
  },
  /** @returns {string|false} */
  text: function () {
    if (!IDL.shown.call(this)) return false;
    const element = IDL.element.call(this),
      target = element.shadowRoot.querySelector('.text');
    return target.textContent || false;
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/messagebar/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__("./src/features/button/_test.idl.js");

const IDL = {
  /** @returns {boolean} */
  visible: function () {
    return this.shadowRoot.childElementCount > 0;
  },
  /** @returns {Element|null} */
  close: function () {
    const target = this.shadowRoot.querySelector('.close');
    if (!target) return null;
    return _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].element.call(target);
  },
  /** @returns {string|null} */
  classify: function () {
    const target = this.shadowRoot.querySelector('.messagebar');
    if (!target) return null;
    return target.dataset.classify ?? null;
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/messagedialog/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__("./src/features/button/_test.idl.js");
/* harmony import */ var _dialog_test_idl_js__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__("./src/features/dialog/_test.idl.js");


const IDL = {
  /**
   * @this {Element} Context document.
   * @returns {Element}
   */
  element: function () {
    if (this.nodeType === Node.DOCUMENT_NODE) return this.body.querySelector(':scope > #aui-message-dialog');
    return null;
  },
  /** @returns {boolean} */
  shown: function () {
    const element = IDL.element.call(this);
    if (!element) return false;
    return _dialog_test_idl_js__WEBPACK_IMPORTED_MODULE_1__["default"].shown.call(element);
  },
  /** @returns {string|null} */
  title: function () {
    const element = IDL.element.call(this);
    if (!element) return null;
    return _dialog_test_idl_js__WEBPACK_IMPORTED_MODULE_1__["default"].header.call(element) || null;
  },
  /** @returns {string|null} */
  contents: function () {
    const element = IDL.element.call(this);
    if (!element) return null;
    const elements = _dialog_test_idl_js__WEBPACK_IMPORTED_MODULE_1__["default"].body.call(element);
    return elements.map(el => el.innerText).join(' ') || null;
  },
  /** @returns {Element|null} */
  action: function (name) {
    const element = IDL.element.call(this);
    if (!element) return null;
    const [...actions] = element.querySelectorAll(':scope > [slot="buttons"]');
    const result = actions.find(action => {
      const button = _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].element.call(action);
      return button.name === name || button.id === name;
    });
    return result || null;
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/multicombobox/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__("./src/features/button/_test.idl.js");
/* harmony import */ var _combobox_test_idl_js__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__("./src/features/combobox/_test.idl.js");
/* harmony import */ var _comboboxshell_test_idl_js__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__("./src/features/comboboxshell/_test.idl.js");
/* harmony import */ var _selection_test_old_idl_js__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__("./src/features/selection/_test.old.idl.js");




const IDL = {
  ..._combobox_test_idl_js__WEBPACK_IMPORTED_MODULE_1__["default"],
  /** @returns {Promise<void>} */
  setUIValue: async function (content) {
    if (IDL.disabled.call(this)) return Promise.reject();
    IDL.triggerButton.call(this).click();
    const clear = this.querySelector('#clear');
    _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].element.call(clear)?.click();
    await _selection_test_old_idl_js__WEBPACK_IMPORTED_MODULE_3__["default"].setUIValue.call(IDL.selection.call(this), content);
    IDL.ok.call(this).click();
    return Promise.resolve();
  },
  /** @returns {Map<string, boolean>} */
  itemsSelected: function () {
    const items = IDL.items.call(this),
      result = new Map();
    for (const item of items) {
      const value = item.dataset.value,
        {
          checked
        } = item.querySelector('input[type="checkbox"]');
      result.set(value, checked);
    }
    return result;
  },
  /** @returns {Element} */
  ok: function () {
    return _comboboxshell_test_idl_js__WEBPACK_IMPORTED_MODULE_2__["default"].button.call(this, 'ok');
  },
  /** @returns {Element} */
  cancel: function () {
    return _comboboxshell_test_idl_js__WEBPACK_IMPORTED_MODULE_2__["default"].button.call(this, 'cancel');
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/navpanel/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__("./src/features/button/_test.idl.js");

const IDL = {
  /** @returns {Element} */
  element: function () {
    return this.shadowRoot.querySelector('nav');
  },
  /** @returns {Element|null} */
  header: function () {
    const slot = this.shadowRoot.querySelectorAll('[slot="header"]'),
      elements = slot.assignedElements({
        flatten: true
      });
    return elements[0] || null;
  },
  /** @returns {Element|null} */
  footer: function () {
    const slot = this.shadowRoot.querySelectorAll('[slot="footer"]'),
      elements = slot.assignedElements({
        flatten: true
      });
    return elements[0] || null;
  },
  /** @returns {boolean} */
  expanded: function () {
    const nav = IDL.element.call(this);
    return nav.ariaExpanded === 'true';
  },
  /** @returns {Element|null} */
  toggleButton: function () {
    const button = this.shadowRoot.querySelector('#toggle');
    if (!button) return null;
    return _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].element.call(button);
  },
  /** @returns {Element|null} */
  item: function (id) {
    const util = globalThis._Util,
      _id = util.kebabCase(id),
      item = this.shadowRoot.querySelector(`li#${_id}`);
    return item?.querySelector(':scope > .item') || null;
  },
  /** @returns {Element|null} */
  itemIcon: function (id) {
    const item = IDL.item.call(this, id);
    return item?.querySelector(':scope > .item-icon') || null;
  },
  /** @returns {Element|null} */
  group: function (name) {
    const groups = this.shadowRoot.querySelectorAll('.group-header');
    for (const group of groups) {
      if (group.textContent === name) {
        return group;
      }
    }
    return null;
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/notification/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _badge_test_idl_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__("./src/features/badge/_test.idl.js");
/* harmony import */ var _button_test_idl_js__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__("./src/features/button/_test.idl.js");
/* harmony import */ var _popup_test_idl_js__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__("./src/features/popup/_test.idl.js");



const IDL = {
  /** @returns {Element} */
  trigger: function () {
    const trigger = this.querySelector('.aui-notification-trigger');
    return _button_test_idl_js__WEBPACK_IMPORTED_MODULE_1__["default"].element.call(trigger);
  },
  /** @returns {number} */
  value: function () {
    const badge = this.querySelector('aui-badge');
    return _badge_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].value.call(badge);
  },
  /** @returns {Element} */
  popup: function () {
    const trigger = this.querySelector('.aui-notification-trigger'),
      id = trigger.dataset.popup,
      doc = trigger.ownerDocument,
      popup = doc.getElementById(id);
    return popup;
  },
  /** @returns {Element} */
  close: function () {
    const popup = IDL.popup.call(this);
    return _popup_test_idl_js__WEBPACK_IMPORTED_MODULE_2__["default"].close.call(popup);
  },
  /** @returns {Array<Element>} */
  actions: function () {
    const popup = IDL.popup.call(this),
      box = popup.querySelector('.aui-notification-actions'),
      [...actions] = box.querySelectorAll('aui-button');
    return actions;
  },
  /** @returns {Array<Element>} */
  items: function () {
    const popup = IDL.popup.call(this),
      [...items] = popup.querySelectorAll('aui-notification-list > .aui-notification-item');
    return items.map(item => item.firstElementChild);
  },
  /** @returns {Element|null} */
  item: function (index) {
    const items = IDL.items.call(this);
    return items[index] || null;
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/pager/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _input_test_idl_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__("./src/features/input/_test.idl.js");

const IDL = {
  /** @returns {Element} */
  next: function () {
    return this.shadowRoot.querySelector('.next');
  },
  /** @returns {Element} */
  prev: function () {
    return this.shadowRoot.querySelector('.previous');
  },
  /** @returns {Element} */
  first: function () {
    return this.shadowRoot.querySelector('.button-first');
  },
  /** @returns {Element} */
  last: function () {
    return this.shadowRoot.querySelector('.button-last');
  },
  /** @returns {Element} */
  showRows: function () {
    return this.shadowRoot.querySelector('.page-size-select');
  },
  /** @returns {Element} */
  pageSizeElement: function () {
    return this.shadowRoot.querySelector('.page-size');
  },
  /** @returns {Element} */
  input: function () {
    const shadow = this.shadowRoot,
      simple = shadow.querySelector('.content-simple'),
      type = window.getComputedStyle(simple).display === 'none' ? 'default' : 'simple',
      target = type === 'default' ? shadow.querySelector('.page-jump>.jump-input') : shadow.querySelector('.content-simple>.jump-input');
    return _input_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].element.call(target);
  },
  /** @returns {Element} */
  goto: function () {
    return this.shadowRoot.querySelector('.goto');
  },
  /** @returns {string} */
  type: function () {
    const simple = this.shadowRoot.querySelector('.content-simple');
    return window.getComputedStyle(simple).display === 'none' ? 'default' : 'simple';
  },
  /** @returns {number} */
  selectedPage: function () {
    const simple = this.shadowRoot.querySelector('.content-simple'),
      type = window.getComputedStyle(simple).display === 'none' ? 'default' : 'simple';
    let selectedPage;
    if (type === 'default') {
      const input = this.shadowRoot.querySelector('.page-jump>.jump-input');
      selectedPage = input ? input.getAttribute('value') : 1;
    } else {
      const input = this.shadowRoot.querySelector('.content-simple>.jump-input');
      selectedPage = input ? input.getAttribute('value') : 1;
    }
    return Number(selectedPage);
  },
  /** @returns {number} */
  pageCount: function () {
    const simple = this.shadowRoot.querySelector('.content-simple'),
      type = window.getComputedStyle(simple).display === 'none' ? 'default' : 'simple';
    let total;
    if (type === 'default') {
      const last = this.shadowRoot.querySelector('.pagination>aui-button-lite:last-child');
      total = last ? last.textContent : 1;
    } else {
      const last = this.shadowRoot.querySelector('.pagecount');
      total = last ? last.textContent : 1;
    }
    return Number(total);
  },
  /** @returns {number} */
  pageSize: function () {
    const select = this.shadowRoot.querySelector('.page-size>aui-select-lite'),
      size = 10;
    return select ? Number(select.getAttribute('value')) : size;
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/panel/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _dialog_test_idl_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__("./src/features/dialog/_test.idl.js");

const {
  shown,
  element,
  close,
  header,
  body,
  main,
  buttons,
  backdrop
} = _dialog_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"];
const IDL = {
  shown,
  element,
  close,
  header,
  body,
  main,
  buttons,
  backdrop
};
Object.assign(IDL, {
  /** @returns {Element} */
  back: function () {
    const modal = this.shadowRoot.querySelector('aui-modal');
    return modal.querySelector('aui-button-lite[slot="action-start"]');
  }
});
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/popover/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
const IDL = {
  /** @returns {Element} */
  element: function () {
    const pop = this.shadowRoot.querySelector('aui-pop');
    const element = pop.shadowRoot.querySelector('.pop');
    return element;
  },
  /** @returns {Element} */
  trigger: function () {
    const slot = this.shadowRoot.querySelector('slot[name="trigger"]'),
      [trigger] = slot.assignedElements({
        flatten: true
      });
    return trigger;
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/popup/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__("./src/features/button/_test.idl.js");

const IDL = {
  /** @returns {Element} */
  close: function () {
    const actions = this.querySelector('.aui-popup-header .aui-popup-actions'),
      action = actions.querySelector('.aui-popup-close');
    return _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].element.call(action);
  },
  /** @returns {Element} */
  body: function () {
    const body = this.querySelector('.aui-popup-body');
    return body.firstElementChild;
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/profile/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
const IDL = {
  /** @returns {Element} */
  avatar: function () {
    return this.shadowRoot.querySelector('aui-avatar');
  },
  /** @returns {string} */
  name: function () {
    const target = this.shadowRoot.querySelector('#name');
    return target.textContent;
  },
  /** @returns {string} */
  sub: function () {
    const target = this.shadowRoot.querySelector('#sub');
    return target.textContent;
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/progressbar/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
const IDL = {
  /** @returns {Element} */
  box: function () {
    return this.shadowRoot.querySelector('.box');
  },
  /** @returns {Element} */
  element: function () {
    return this.shadowRoot.querySelector('progress');
  },
  /** @returns {number} */
  value: function () {
    const element = IDL.element.call(this);
    return Number(element && (element.ariaValueNow || element.value) || 0);
  },
  /** @returns {number} */
  max: function () {
    const element = IDL.element.call(this);
    return Number(element && (element.ariaValueMax || element.max) || 0);
  },
  /** @returns {number} */
  min: function () {
    const element = IDL.element.call(this);
    return Number(element && (element.ariaValueMin || element.min) || 0);
  },
  /** @returns {Element} */
  animate: function () {
    return this.shadowRoot.querySelector('.indeterminate');
  },
  /** @returns {Element} */
  template: function () {
    return this.shadowRoot.querySelector('.text');
  },
  /** @returns {string|null} */
  classify: function () {
    const box = IDL.box.call(this);
    return box?.dataset.classify ?? null;
  },
  /** @returns {string} */
  classifyColor: function () {
    const element = IDL.element.call(this);
    return getComputedStyle(element).color;
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/radio/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
const IDL = {
  /** @returns {Array<Element>} */
  radioList: function () {
    return this.querySelectorAll('aui-radio');
  },
  /** @returns {Element} */
  groupBox: function () {
    return this.querySelector('.aui-choice-group');
  },
  /** @returns {Element} */
  label: function () {
    return this.querySelector('.aui-radio');
  },
  /** @returns {Element} */
  input: function () {
    return this.querySelector('.aui-choice-input');
  },
  /** @returns {boolean} */
  checked: function () {
    const element = this.querySelector('.aui-choice-input');
    const checked = element.getAttribute('aria-checked');
    return checked === 'true';
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/rangepicker/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _calendar_test_idl_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__("./src/features/calendar/_test.idl.js");
/* harmony import */ var _comboboxshell_test_idl_js__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__("./src/features/comboboxshell/_test.idl.js");


const {
  arias,
  clear,
  content,
  disabled,
  input,
  placeholder,
  popup,
  popupContent,
  tooltip,
  triggerButton
} = _comboboxshell_test_idl_js__WEBPACK_IMPORTED_MODULE_1__["default"];
const IDL = {
  arias,
  clear,
  content,
  disabled,
  input,
  placeholder,
  popup,
  popupContent,
  tooltip,
  triggerButton
};
Object.assign(IDL, {
  /** @returns {Element} */
  calendar: function () {
    return this.querySelector('.aui-calendar');
  },
  /** @returns {Element|null} */
  ok: function () {
    return _comboboxshell_test_idl_js__WEBPACK_IMPORTED_MODULE_1__["default"].button.call(this, 'ok');
  }
});
const calendarMethods = ['pageButtons', 'monthView', 'monthItem', 'yearView', 'yearItem', 'decadeView', 'decadeItem'];
for (const method of calendarMethods) {
  IDL[method] = function (...args) {
    const calendar = IDL.calendar.call(this);
    return _calendar_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"][method].call(calendar, ...args);
  };
}
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/rating/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
const IDL = {
  /** @returns {Promise<void>} */
  // eslint-disable-next-line require-await
  setUIValue: async function (content) {
    const targets = this.shadowRoot.querySelectorAll('input[type="radio"]');
    if (!targets.length || targets[0].disabled) return Promise.reject();
    const target = targets[content - 1];
    return target ? Promise.resolve(target.click()) : Promise.reject();
  },
  /** @returns {number} */
  active: function () {
    const [...checked] = this.shadowRoot.querySelectorAll('input[checked]'),
      element = checked.at(-1);
    return element ? Number(element.value) : 0;
  },
  /** @returns {number|null} */
  stackValue: function () {
    const element = this.shadowRoot.querySelector('.rating');
    if (element.role !== 'radiogroup') return null;
    const [...checkedList] = element.querySelectorAll('[data-checked="true"]'),
      checked = checkedList.at(-1);
    if (!checked) return 0;
    const value = Number(checked.value),
      percentEl = checked.parentNode.querySelector('.percent>.current');
    if (!percentEl) return value;
    const percent = Number.parseInt(percentEl.getAttribute('offset'), 10) / 100;
    return value - 1 + percent;
  },
  /** @returns {Element} */
  action: function (index) {
    const elementList = this.shadowRoot.querySelectorAll('.rating>label');
    return elementList[index].querySelector('input');
  },
  /** @returns {Array<Element>} */
  elementList: function () {
    return this.shadowRoot.querySelectorAll('.rating>label');
  },
  /** @returns {Array<string>} */
  icons: function () {
    const [...list] = this.shadowRoot.querySelectorAll('label');
    return list.flatMap(label => {
      const [...classList] = label.querySelector('.icon').classList;
      return classList.find(c => c !== 'icon') || [];
    });
  },
  /** @returns {string|null} */
  classify: function () {
    const element = this.shadowRoot.querySelector('.rating');
    return element?.dataset.classify ?? null;
  },
  /** @returns {string} */
  classifyColor: function () {
    const util = globalThis._Util,
      element = this.shadowRoot.querySelector('.rating'),
      color = window.getComputedStyle(element).getPropertyValue('--classify');
    return util.hex2rgb(color);
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/richcombobox/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _comboboxshell_test_old_idl_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__("./src/features/comboboxshell/_test.old.idl.js");
/* harmony import */ var _profile_test_idl_js__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__("./src/features/profile/_test.idl.js");
/* harmony import */ var _selection_test_old_idl_js__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__("./src/features/selection/_test.old.idl.js");



const {
  arias,
  clear,
  content,
  disabled,
  element,
  placeholder,
  popup,
  triggerButton
} = _comboboxshell_test_old_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"];
const ComboboxIDL = {
  arias,
  clear,
  content,
  disabled,
  element,
  placeholder,
  popup,
  triggerButton
};
Object.assign(ComboboxIDL, {
  /** @returns {Element} */
  selection: function () {
    const popup = ComboboxIDL.popup.call(this);
    return popup.querySelector('.aui-selection');
  }
});
for (const [key, value] of Object.entries(_selection_test_old_idl_js__WEBPACK_IMPORTED_MODULE_2__["default"])) {
  if (key.startsWith('searchbox')) continue;
  ComboboxIDL[key] = function (...args) {
    const selection = ComboboxIDL.selection.call(this);
    return value.call(selection, ...args);
  };
}

/** @returns {Element} */
function getSearchbox() {
  return this.querySelector('.aui-richcombobox-input > input');
}
const IDL = {
  ...ComboboxIDL,
  content: () => null,
  icon: _comboboxshell_test_old_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].icon,
  searchbox: getSearchbox,
  searchboxInput: getSearchbox,
  /** @returns {Array<Element>} */
  selectedItems: function () {
    const box = this.querySelector('.aui-richcombobox-selected-area'),
      [...items] = box.querySelectorAll('[role="listitem"]:not(.aui-richcombobox-input)');
    return items;
  },
  // TD: merge invalid & conflict into status.
  /** @returns { {delete: Element|null, content: string|null, value: string|null, tooltip: boolean, readonly: boolean, invalid: boolean, conflict: boolean} } */
  selectedItemInfo: function (item) {
    const {
        classList
      } = item,
      {
        tooltip,
        value
      } = item.dataset;
    let content = item.textContent;
    if (!content) {
      const target = item.querySelector('.aui-richcombobox-selected-item > *');
      switch (target?.localName) {
        case 'aui-profile':
          content = _profile_test_idl_js__WEBPACK_IMPORTED_MODULE_1__["default"].name.call(target);
          break;
      }
    }
    const remove = item.querySelector('.aui-richcombobox-selected-item-close');
    return {
      delete: remove?.checkVisibility() ? remove : null,
      content: content || null,
      value: value || null,
      tooltip: !!tooltip && tooltip !== 'false',
      readonly: classList.contains('aui-richcombobox-undeleteable-item'),
      invalid: classList.contains('aui-richcombobox-unmatched-item'),
      conflict: classList.contains('aui-richcombobox-conflict-item')
    };
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/searchbox/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__("./src/features/button/_test.idl.js");

const IDL = {
  /** @returns {Promise<void>} */
  setUIValue: function (content) {
    const input = IDL.input.call(this);
    if (!input.matches(':read-write')) return Promise.reject();
    input.value = String(content ?? '');
    input.dispatchEvent(new InputEvent('input', {
      bubbles: true
    }));
    IDL.action.call(this)?.click();
    return Promise.resolve();
  },
  /** @returns {Element} */
  input: function () {
    const target = this.shadowRoot.querySelector('input,textarea');
    return target;
  },
  /** @returns {boolean} */
  searched: function () {
    const target = IDL.input.call(this);
    return target.ariaBusy === 'true';
  },
  /** @returns {Element} */
  action: function () {
    const target = this.shadowRoot.querySelector('.button'),
      button = _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].element.call(target);
    return button.disabled || button.getAttribute('tabindex') === '-1' ? null : button;
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/selection/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__("./src/features/button/_test.idl.js");
/* harmony import */ var _loading_test_idl_js__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__("./src/features/loading/_test.idl.js");
/* harmony import */ var _searchbox_test_idl_js__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__("./src/features/searchbox/_test.idl.js");



const getRoot = context => context.shadowRoot || context;
const searchboxMapping = fn => {
  return function (...args) {
    const searchbox = IDL.searchbox.call(this);
    if (!searchbox) return null;
    return fn.apply(searchbox, args);
  };
};
const IDL = {
  /** @returns {Element|null} */
  searchbox: function () {
    const root = getRoot(this);
    return root.querySelector('aui-searchbox');
  },
  searchboxInput: searchboxMapping(_searchbox_test_idl_js__WEBPACK_IMPORTED_MODULE_2__["default"].input),
  searchboxSearched: searchboxMapping(_searchbox_test_idl_js__WEBPACK_IMPORTED_MODULE_2__["default"].searched),
  searchboxAction: searchboxMapping(_searchbox_test_idl_js__WEBPACK_IMPORTED_MODULE_2__["default"].action),
  setUIValue: async function (content) {
    const util = globalThis._Util,
      searchbox = IDL.searchbox.call(this),
      root = getRoot(this),
      box = root.querySelector('[role="listbox"]');
    const setSelection = value => {
      const items = IDL.items.call(this, true),
        item = items.find(el => util.textContent(el) === value);
      if (!item) return Promise.reject();
      item.click();
      return Promise.resolve();
    };
    const setUIValue = value => {
      return _searchbox_test_idl_js__WEBPACK_IMPORTED_MODULE_2__["default"].setUIValue.call(searchbox, value).then(() => {
        const {
          promise,
          resolve,
          reject
        } = Promise.withResolvers();
        setTimeout(() => {
          let loopCount = 0;
          const waitLoading = () => {
            if (loopCount > 20) return reject();
            if (!IDL.isLoading.call(this)) return setSelection(value).then(resolve, reject);
            loopCount++;
            setTimeout(waitLoading, 200);
          };
          waitLoading();
        }, 500); // For cancel out debounce.
        return promise;
      });
    };
    if (box.ariaMultiSelectable === 'true') {
      let hasValue = false;
      for (let item of content.split(',')) {
        /// same as allNames splitter
        item = item.trim();
        if (!item) continue;
        hasValue = true;
        await setUIValue(item);
      }
      return hasValue ? Promise.resolve() : Promise.reject();
    } else {
      content = content.trim();
      if (!content) return Promise.reject();
      return searchbox ? setUIValue(content) : setSelection(content);
    }
  },
  /** @returns {Element|null} */
  createNew: function () {
    const root = getRoot(this),
      target = _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].element.call(root.querySelector('#create-new'));
    return target?.checkVisibility({
      visibilityProperty: true
    }) ? target : null;
  },
  /** @returns {Element|null} */
  selectAll: function () {
    const root = getRoot(this);
    return root.querySelector('#select-all');
  },
  /** @returns {Element} */
  scrollBox: function () {
    const root = getRoot(this);
    return root.querySelector('aui-tabloop');
  },
  /** @returns {Element} */
  lazyloadBox: function () {
    const root = getRoot(this);
    return root.querySelector('aui-tabloop aui-lazyload');
  },
  /** For checking sizes @returns {Element} */
  listbox: function () {
    const root = getRoot(this);
    return root.querySelector('.selection');
  },
  /** @returns {Boolean|null} */
  isLoading: function () {
    const root = getRoot(this),
      loading = root.querySelector('#loading');
    if (!loading) return null;
    return _loading_test_idl_js__WEBPACK_IMPORTED_MODULE_1__["default"].shown.call(loading);
  },
  /** @returns {Array<Element>} */
  items: function (filterDisabled) {
    const box = IDL.lazyloadBox.call(this),
      [...list] = box.querySelectorAll('.item'),
      filters = [el => el.checkVisibility()];
    if (filterDisabled) filters.push(el => el.dataset.disabled !== 'true' && el.dataset.readonly !== 'true');
    return list.filter(item => filters.every(fn => fn(item)));
  },
  /** @returns {Element|null} */
  itemByIndex: function (index) {
    const list = IDL.items.call(this);
    return list.at(index) || null;
  },
  /** @returns {boolean} */
  getChecked: function (item) {
    const input = item.querySelector('input');
    return input?.checked || false;
  },
  /** @returns {Element|null} */
  group: function (name) {
    const box = IDL.lazyloadBox.call(this),
      group = box.querySelectorAll(`details.group-box`);
    return group || null;
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/selection/_test.old.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _loading_test_idl_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__("./src/features/loading/_test.idl.js");
/* harmony import */ var _searchbox_test_idl_js__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__("./src/features/searchbox/_test.idl.js");
/* harmony import */ var _test_idl_js__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__("./src/features/selection/_test.idl.js");



const {
  searchbox,
  searchboxInput,
  searchboxSearched,
  searchboxAction
} = _test_idl_js__WEBPACK_IMPORTED_MODULE_2__["default"];
const IDL = {
  searchbox,
  searchboxInput,
  searchboxSearched,
  searchboxAction
};
Object.assign(IDL, {
  setUIValue: async function (content) {
    const util = globalThis._Util,
      searchbox = IDL.searchbox.call(this);
    const setSelection = value => {
      const items = IDL.items.call(this, true),
        item = items.find(el => util.textContent(el) === value);
      if (!item) return Promise.reject();
      item.click();
      return Promise.resolve();
    };
    const setUIValue = value => {
      return _searchbox_test_idl_js__WEBPACK_IMPORTED_MODULE_1__["default"].setUIValue.call(searchbox, value).then(() => {
        const {
          promise,
          resolve,
          reject
        } = Promise.withResolvers();
        setTimeout(() => {
          let loopCount = 0;
          const waitLoading = () => {
            if (loopCount > 20) return reject();
            if (!IDL.isLoading.call(this)) return setSelection(value).then(resolve, reject);
            loopCount++;
            setTimeout(waitLoading, 200);
          };
          waitLoading();
        }, 500); // For cancel out debounce.
        return promise;
      });
    };
    if (this.ariaMultiSelectable === 'true') {
      let hasValue = false;
      for (let item of content.split(',')) {
        /// same as allNames splitter
        item = item.trim();
        if (!item) continue;
        hasValue = true;
        await setUIValue(item);
      }
      return hasValue ? Promise.resolve() : Promise.reject();
    } else {
      content = content.trim();
      if (!content) return Promise.reject();
      return searchbox ? setUIValue(content) : setSelection(content);
    }
  },
  /** @returns {Element|null} */
  createNew: function () {
    const target = this.querySelector('.aui-selection-create-new');
    return target?.checkVisibility({
      visibilityProperty: true
    }) ? target : null;
  },
  /** @returns {Element|null} */
  selectAll: function () {
    return this.querySelector('.aui-selection-select-all');
  },
  /** @returns {Element} */
  scrollBox: function () {
    return this.querySelector('.aui-selection-listbox-container');
  },
  /** @returns {Element} */
  listbox: function () {
    return this.querySelector('.aui-selection-listbox');
  },
  /** @returns {Boolean|null} */
  isLoading: function () {
    const box = this.querySelector('.aui-selection-loading-box'),
      loading = box.querySelector(':scope > aui-loading');
    if (!loading) return null;
    return _loading_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].shown.call(loading);
  },
  /** @returns {Array<Element>} */
  items: function (filterDisabled) {
    const box = IDL.listbox.call(this),
      [...list] = box.querySelectorAll(':scope > .aui-selection-item'),
      filters = [el => el.checkVisibility()];
    if (filterDisabled) filters.push(el => !el.classList.contains('aui-selection-label-disabled'));
    return list.filter(item => filters.every(fn => fn(item)));
  },
  /** @returns {Element|null} */
  itemByIndex: function (index) {
    const list = IDL.items.call(this);
    return list.at(index) || null;
  },
  /** @returns {Element|null} */
  itemByValue: function (value) {
    const list = IDL.items.call(this);
    return list.find(item => item.dataset.value === value) || null;
  },
  /** @returns {Element|null} */
  group: function (name) {
    const box = IDL.listbox.call(this),
      group = box.querySelector(`:scope > .aui-selection-group[data-group="${name}"]`);
    return group || null;
  }
});
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/shimmer/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
const IDL = {
  /** @returns {boolean} */
  isLoading: function () {
    const graph = this.shadowRoot.querySelector('.shimmer > svg');
    return !!graph;
  },
  /** @returns {Element|null} */
  content: function () {
    const loading = IDL.isLoading.call(this);
    if (loading) return null;
    const slot = this.shadowRoot.querySelector('slot'),
      [node] = slot.assignedElements();
    return node;
  },
  /** @returns {string} */
  shape: function () {
    return this.shadowRoot.firstElementChild.dataset.shape;
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/slider/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
const IDL = {
  /** @returns {Promise<void>} */
  // eslint-disable-next-line require-await
  setUIValue: async function (content) {
    const input = this.shadowRoot.querySelector('input[type="range"]:not([tabindex="-1"])');
    if (!input || input.disabled || content == null) return Promise.reject();
    input.value = content;
    input.dispatchEvent(new InputEvent('input', {
      bubbles: true
    }));
    return Promise.resolve();
  },
  /** @returns {Element} */
  element: function () {
    return this.shadowRoot.querySelector('input');
  },
  /** @returns {number} */
  valuenow: function () {
    const element = IDL.element.call(this);
    return Number(element.value);
  },
  /** @returns {number} */
  valuemax: function () {
    const element = IDL.element.call(this);
    return Number(element.ariaValueMax || element.max);
  },
  /** @returns {number} */
  valuemin: function () {
    const element = IDL.element.call(this);
    return Number(element.ariaValueMin || element.min);
  },
  /** @returns {number} */
  orient: function () {
    const orient = this.ariaOrientation ?? this._.ariaOrientation;
    return orient === 'vertical' ? 'v' : 'h';
  },
  /** @returns {boolean} */
  reverse: function () {
    const element = IDL.element.call(this);
    return element.dir === 'rtl';
  },
  /** @returns {void} */
  setValue: function (num) {
    this.setAttribute('value', num);
    const input = IDL.element.call(this);
    // force trigger callback.
    input.dispatchEvent(new InputEvent('input'));
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/splitter/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
const IDL = {
  /** @returns {Element} */
  element: function () {
    return this.shadowRoot.querySelector('.splitter');
  },
  /** @returns {Element} */
  aElement: function () {
    return this.shadowRoot.querySelector('slot[name="0"]');
  },
  /** @returns {Element} */
  bElement: function () {
    return this.shadowRoot.querySelector('slot[name="1"]');
  },
  /** @returns {Element} */
  bar: function () {
    return this.shadowRoot.querySelector('aui-drag .bar');
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/switch/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
const IDL = {
  /** @returns {Promise<void>} */
  // eslint-disable-next-line require-await
  setUIValue: async function (content) {
    const button = IDL.element.call(this),
      checked = String(content) === 'true';
    if (button.disabled || button.ariaChecked === String(checked)) return Promise.reject();
    button.click();
    return Promise.resolve();
  },
  /** @returns {Element} */
  element: function () {
    return this.shadowRoot.querySelector('.button');
  },
  /** @returns {string} */
  size: function () {
    const s = this.shadowRoot.querySelector('.size-s');
    return s ? 's' : 'm';
  },
  /** @returns {Record<'on'|'off', Element>} */
  mode: function () {
    const on = this.shadowRoot.querySelector('.on');
    const off = this.shadowRoot.querySelector('.off');
    return {
      on: on,
      off: off
    };
  },
  /** @returns {Promise<boolean>} */
  click: function () {
    const util = globalThis._Util,
      element = IDL.element.call(this);
    if (!element) return util.delay(false);
    if (element) element.click();
    return util.delay(true);
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/tabbutton/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
const IDL = {
  /** @returns {void} */
  setActive: function (ele, num) {
    ele.setAttribute('active', num);
  },
  /** @returns {Element} */
  element: function () {
    return this.shadowRoot.querySelector('[role="tablist"]');
  },
  /** @returns {boolean} */
  disable: function () {
    const ele = IDL.element.call(this);
    const bool = ele.ariaDisabled;
    return JSON.parse(bool);
  },
  /** @returns {Array<Element>} */
  items: function () {
    const elementList = this.shadowRoot.querySelectorAll('label');
    return elementList;
  },
  /** @returns {number} */
  active: function () {
    const elementList = this.shadowRoot.querySelector('input[checked]');
    const active = elementList.value;
    return Number(active);
  },
  /** @returns {string} */
  orient: function () {
    const {
      ariaOrientation
    } = IDL.element.call(this);
    return ariaOrientation?.[0] === 'v' ? 'v' : 'h';
  },
  /** @returns {string} */
  background: function (element) {
    return window.getComputedStyle(element).getPropertyValue('background-color');
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/tabcontrol/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__("./src/features/button/_test.idl.js");

const IDL = {
  /** @returns {Array<Element>} */
  tabList: function () {
    const list = this.querySelectorAll('[role="tab"]');
    return Array.prototype.filter.call(list, el => el.checkVisibility());
  },
  /** @returns {Element} */
  activeTab: function () {
    return this.querySelector('[role="tab"][aria-selected="true"]');
  },
  /** @returns {Element} */
  activePanel: function () {
    return this.querySelector('[role="tabpanel"][aria-hidden="false"]');
  },
  /** @returns {Element|null} */
  addTab: function () {
    return this.querySelector('.tab-plus');
  },
  /** @returns {Record<'status'|(string & {}), Element>} */
  tabActions: function (index) {
    const list = IDL.tabList.call(this),
      tab = list[index],
      data = {};
    if (!tab) return data;
    const box = tab.firstElementChild,
      actions = box.querySelectorAll('aui-button,aui-button-lite');
    data.status = box.dataset.status;
    for (const button of actions) {
      const name = _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].name.call(button);
      data[name] = _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].element.call(button);
    }
    return data;
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/table/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__("./src/features/button/_test.idl.js");
/* harmony import */ var _buttongroup_test_idl_js__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__("./src/features/buttongroup/_test.idl.js");
/* harmony import */ var _checkbox_test_idl_js__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__("./src/features/checkbox/_test.idl.js");
// cSpell:ignore rowchecker




/** @param {Element} context  */
function QueryRowAction(context, index) {
  const row = context.querySelector(`.aui-table-body .aui-table-row[data-row="${index}"]`),
    action = row?.querySelector('.aui-table-row-action-box > *');
  return action?.shadowRoot || action || null;
}
const IDL = {
  /** @returns {Element|null} */
  header: function (index = 0) {
    const cells = this.querySelectorAll('.aui-table-header .aui-table-cell[data-cell]');
    return cells[index] || null;
  },
  /** @returns {Element|null} */
  cell: function (x, y) {
    return this.querySelector(`.aui-table-body .aui-table-cell[data-cell="${x},${y}"]`);
  },
  /** @returns {Element|null} */
  checkAll: function () {
    const checker = this.querySelector('.aui-table-header .aui-table-rowchecker');
    return checker ? _checkbox_test_idl_js__WEBPACK_IMPORTED_MODULE_2__["default"].input.call(checker) : null;
  },
  /** @returns {Element|null} */
  rowChecker: function (index) {
    const row = this.querySelector(`.aui-table-body .aui-table-row[data-row="${index}"]`),
      checker = row?.querySelector('.aui-table-rowchecker');
    return checker ? _checkbox_test_idl_js__WEBPACK_IMPORTED_MODULE_2__["default"].input.call(checker) : null;
  },
  /** @returns {Element|null} */
  rowAction: function (index) {
    const action = QueryRowAction(this, index),
      query = 'aui-button,aui-button-lite';
    return action ? _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].element.call(action.nodeType === 1 && action.matches(query) ? action : action.querySelector(query)) : null;
  },
  /** @returns {Array<Element>} */
  rowActions: function (index) {
    const action = QueryRowAction(this, index);
    if (action.role === 'group') return _buttongroup_test_idl_js__WEBPACK_IMPORTED_MODULE_1__["default"].buttonList.call(action);
    return [_button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].element.call(action)];
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/tag/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
const IDL = {
  /** @returns {Element} */
  delete: function () {
    return this.shadowRoot.querySelector('.delete');
  },
  /** @returns {Element} */
  element: function () {
    return this.shadowRoot.querySelector('.tag');
  },
  /** @returns {Element} */
  blank: function () {
    return this.shadowRoot.querySelector('.tag');
  },
  /** @returns {string} */
  borderColor: function () {
    const element = this.shadowRoot.querySelector('.tag');
    return window.getComputedStyle(element).borderColor;
  },
  /** @returns {string|null} */
  classify: function () {
    const element = this.shadowRoot.querySelector('.tag');
    return element?.dataset.classify ?? null;
  },
  /** @returns {string} */
  classifyColor: function () {
    const util = globalThis._Util,
      element = this.shadowRoot.querySelector('.tag'),
      color = window.getComputedStyle(element).getPropertyValue('--classify');
    return util.hex2rgb(color);
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/timeline/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
const IDL = {
  /** @returns {Array<Element>} */
  items: function () {
    const [...elements] = this.children;
    return elements;
  },
  /** @returns {boolean} */
  dot: function (ele) {
    const element = ele.shadowRoot.querySelector('.dot');
    const bool = !!element;
    return bool;
  },
  /** @returns {boolean} */
  showDate: function (ele) {
    const element = ele.shadowRoot.querySelector("slot[name='date']");
    const bool = !!element;
    return bool;
  },
  /** @returns {Element} */
  toggle: function (ele) {
    const element = ele.shadowRoot.querySelector('.toggleable');
    return element;
  },
  /** @returns {boolean} */
  showContent: function (ele) {
    const element = ele.shadowRoot.querySelector("slot[name='content']");
    return !element.hasAttribute('hidden');
  },
  /** @returns {string} */
  orient: function () {
    return this.getAttribute('orient');
  },
  /** @returns {string} */
  classify: function (ele) {
    const header = ele.shadowRoot.querySelector('.header');
    return window.getComputedStyle(header).color;
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/timepicker/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _comboboxshell_test_idl_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__("./src/features/comboboxshell/_test.idl.js");
/* harmony import */ var _tabbutton_test_idl_js__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__("./src/features/tabbutton/_test.idl.js");


const {
  arias,
  clear,
  closeAll,
  content,
  disabled,
  input,
  placeholder,
  popup,
  popupContent,
  popupSize,
  size,
  tooltip,
  triggerButton
} = _comboboxshell_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"];
const ShellIDL = {
  arias,
  clear,
  closeAll,
  content,
  disabled,
  input,
  placeholder,
  popup,
  popupContent,
  popupSize,
  size,
  tooltip,
  triggerButton
};
function getShell(context) {
  const name = 'aui-combobox-shell';
  if (context.localName === name) return context;
  return context.shadowRoot.querySelector(name);
}
const TimePartIDL = {
  /** @returns {Promise<void>} */
  setUIValue: function (content) {
    const input = IDL.input.call(this, false);
    if (!input || input.disabled || input.ariaReadOnly === 'true') return Promise.reject();
    content = String(content ?? '');
    input.value = content;
    input.dispatchEvent(new InputEvent('input', {
      bubbles: true
    }));
    IDL.ok.call(this).click();
    return input.value === content ? Promise.resolve() : Promise.reject();
  },
  /** @returns {Element} */
  timepart: function () {
    const name = 'aui-time-part';
    if (this.localName === name) return this;
    const shell = getShell(this) || this;
    return shell.querySelector(name);
  },
  /** @returns {Element} */
  hourBox: function () {
    const timepart = TimePartIDL.timepart.call(this);
    return timepart.shadowRoot.querySelector('#hour');
  },
  /** @returns {number} */
  hourValue: function () {
    const box = TimePartIDL.hourBox.call(this),
      value = box.getValue();
    return Number(value);
  },
  /** @returns {Element|null} */
  hourItem: function (query) {
    const box = TimePartIDL.hourBox.call(this),
      list = box.shadowRoot.getElementById('main'),
      [...items] = list.children;
    if (typeof query === 'number') return items[query] || null;
    return items.find(item => item.textContent === String(query)) || null;
  },
  /** @returns {Element} */
  minuteBox: function () {
    const timepart = TimePartIDL.timepart.call(this);
    return timepart.shadowRoot.querySelector('#minute');
  },
  /** @returns {number} */
  minuteValue: function () {
    const box = TimePartIDL.minuteBox.call(this),
      value = box.getValue();
    return Number(value);
  },
  /** @returns {Element|null} */
  minuteItem: function (query) {
    const box = TimePartIDL.minuteBox.call(this),
      list = box.shadowRoot.getElementById('main'),
      [...items] = list.children;
    if (typeof query === 'number') return items[query] || null;
    return items.find(item => item.textContent === String(query)) || null;
  },
  /** @returns {Element|null} */
  periodItem: function (value) {
    const timepart = TimePartIDL.timepart.call(this),
      period = timepart?.shadowRoot.querySelector('aui-tabbutton');
    if (!period) return null;
    const mapping = {
        AM: 0,
        PM: 1
      },
      items = _tabbutton_test_idl_js__WEBPACK_IMPORTED_MODULE_1__["default"].items.call(period);
    return items[mapping[value]] || null;
  },
  /** @returns {string|null} */
  activePeriod: function () {
    const timepart = TimePartIDL.timepart.call(this),
      tab = timepart.shadowRoot.querySelector('aui-tabbutton');
    if (!tab) return null;
    const util = globalThis._Util,
      slot = tab.shadowRoot.querySelector('[aria-selected="true"] + slot'),
      [choice] = slot.assignedElements({
        flatten: true
      });
    return util.textContent(choice) || null;
  }
};
const IDL = {
  ...ShellIDL,
  ...TimePartIDL,
  /** @returns {Element} */
  ok: function () {
    return _comboboxshell_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].button.call(this, 'ok');
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/toast/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__("./src/features/button/_test.idl.js");

const IDL = {
  /** @returns {number} */
  toastsNum: function () {
    const ele = this.parentNode;
    if (ele?.id !== 'aui-toaster') return 0;
    let num = 0;
    for (const child of ele.children) {
      if (child.checkVisibility()) num++;
    }
    return num;
  },
  /** @returns {Element} */
  element: function () {
    return this.shadowRoot.firstElementChild;
  },
  /** @returns {Element|null} */
  close: function () {
    const action = this.shadowRoot.querySelector('.close');
    return _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].element.call(action);
  },
  /** @returns {Element|null} */
  progress: function () {
    return this.shadowRoot.querySelector('aui-progressbar');
  },
  /** @returns {string|null} */
  classify: function () {
    const element = IDL.element.call(this);
    return element.dataset.classify ?? null;
  },
  /** @returns {string} */
  date: function () {
    const ele = this.shadowRoot.querySelector('.date');
    return ele.textContent;
  },
  /** @returns {string} */
  icon: function (full) {
    const slot = this.shadowRoot.querySelector('.icon'),
      [element] = slot.assignedElements({
        flatten: true
      }),
      [...list] = element.classList;
    if (full) return list.join(' ');
    return list.find(name => !name.startsWith('icon-'));
  },
  /** @returns {string} */
  state: function () {
    const element = IDL.element.call(this);
    return element.dataset.state;
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/togglebutton/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
const IDL = {
  /** @returns {Element} */
  element: function () {
    return this.shadowRoot.querySelector('label');
  },
  /** @returns {Element} */
  input: function () {
    return this.shadowRoot.querySelector('input');
  },
  /** @returns {boolean} */
  checked: function () {
    const input = IDL.input.call(this),
      checked = JSON.parse(input.ariaPressed);
    return checked;
  },
  /** @returns {string|null} */
  classify: function () {
    const element = IDL.element.call(this);
    for (const cls of element.classList) {
      if (cls.startsWith('classify-')) return cls.replace('classify-', '');
    }
    return null;
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/uploader/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__("./src/features/button/_test.idl.js");

const IDL = {
  /** @returns {Element} */
  filePicker: function () {
    return this.querySelector('aui-uploader-picker').shadowRoot.querySelector('input');
  },
  /** @returns {Element|null} */
  fileByIndex: function (index = 0) {
    const list = this.querySelector('aui-uploader-echo').shadowRoot.querySelectorAll("[role='listitem']");
    return list[index] || null;
  },
  /**
   * @param {Element|number} index The file element or index
   * @returns {Record<'icon'|'name'|'size'|'message'|(string & {}), Element|string>}
   */
  file: function (index = 0) {
    const file = index?.nodeType === Node.ELEMENT_NODE ? index : IDL.fileByIndex.call(this, index);
    const data = {};
    if (file) {
      const content = file.querySelector('.aui-uploader-item-content');
      Object.assign(data, {
        icon: file.querySelector('.icon'),
        name: content.querySelector('.name').textContent,
        size: content.querySelector('.size')?.textContent || null,
        message: content.querySelector('.message')?.textContent || null
      });
      const actions = file.querySelectorAll('.action > :is(aui-button, aui-button-lite)');
      for (const action of actions) {
        const name = _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].name.call(action);
        if (name) data[name] = _button_test_idl_js__WEBPACK_IMPORTED_MODULE_0__["default"].element.call(action);
      }
    }
    return data;
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/features/validation/_test.idl.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
const IDL = {
  /** @returns {Element} */
  element: function () {
    return this.closest('.aui-validation');
  },
  /** @returns {boolean} */
  valid: function () {
    const element = IDL.element.call(this),
      invalid = element.querySelector('[aria-invalid="true"]');
    return !invalid;
  },
  /** @returns {Element} */
  messageElement: function () {
    const element = IDL.element.call(this),
      {
        errorby
      } = element.dataset,
      doc = element.ownerDocument;
    return doc.querySelector(errorby) || doc.querySelector('.aui-validation-message');
  },
  /** @returns {string|null} */
  message: function () {
    const element = IDL.messageElement.call(this);
    if (element?.checkVisibility({
      visibilityProperty: true
    })) return element.textContent;
    return null;
  },
  /** @todo @returns {Element|null} */
  target: function () {
    return null;
  }
};
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (IDL);

/***/ },

/***/ "./src/util/css/absolute-color.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (/* binding */ absoluteColor)
/* harmony export */ });
const canvas = document.createElement('canvas'),
  ctx = canvas.getContext('2d', {
    willReadFrequently: true
  });
canvas.width = 1;
canvas.height = 1;

/** @returns {[r: number, g: number, b: number, a: number]} */
function colorData(value) {
  ctx.fillStyle = value;
  ctx.fillRect(0, 0, 1, 1);
  const imageData = ctx.getImageData(0, 0, 1, 1),
    [r, g, b, a] = imageData.data;
  ctx.clearRect(0, 0, 1, 1);
  return [r, g, b, a];
}
const hexColorRegex = /^#([0-9a-f]{3,8})$/i;
function hexValue(r, g, b, a, alpha) {
  const p = v => v.toString(16).padStart(2, '0');
  const v = '#' + p(r) + p(g) + p(b);
  return alpha ? v + p(a) : v;
}
function rgbValue(r, g, b, a, alpha) {
  return alpha ? `rgb(${r} ${g} ${b} / ${a / 255})` : `rgb(${r} ${g} ${b})`;
}
function rgbaValue(r, g, b, a, alpha) {
  return alpha ? `rgba(${r}, ${g}, ${b}, ${a / 255})` : `rgb(${r}, ${g}, ${b})`;
}
const isLightScheme = function () {
  let value;
  return function () {
    if (value == null) {
      const {
        colorScheme
      } = getComputedStyle(document.body);
      value = colorScheme.includes('light') || !colorScheme.includes('dark');
    }
    return value;
  };
}();
function tryLightDark(value) {
  const match = /^light-dark\s*\(/i.exec(value);
  if (match && value.endsWith(')')) {
    const content = value.slice(match[0].length, -1);
    let depth = 0;
    for (let i = 0; i < content.length; i++) {
      const ch = content[i];
      if (ch === '(') {
        depth++;
      } else if (ch === ')') {
        depth--;
      } else if (ch === ',' && depth === 0) {
        const result = isLightScheme() ? content.slice(0, i) : content.slice(i + 1);
        return result.trim();
      }
    }
  }
  return value;
}

/** @type {typeof $$.css.absoluteColor} */
function absoluteColor(value, type = 'hex', alpha = false) {
  if (!value?.length) throw new TypeError('Value is required');
  if (!CSS.supports('color', value)) throw new TypeError('Not supported color value: ' + value);
  if (value.includes('var(--')) throw new TypeError('Cannot parse CSS variable, you should use getComputedStyle to resolve it first.');
  if (type === 'hex' && hexColorRegex.test(value)) return value;
  value = tryLightDark(value);
  const [r, g, b, a] = colorData(value),
    useAlpha = alpha === 'auto' ? a < 255 : alpha;
  switch (type) {
    case 'rgb':
      return rgbValue(r, g, b, a, useAlpha);
    case 'rgba':
      return rgbaValue(r, g, b, a, useAlpha);
    case 'hex':
      return hexValue(r, g, b, a, useAlpha);
    case 'rgbArr':
      return [r, g, b, a];
  }
}

/***/ },

/***/ "./src/util/string/kebab-case.js"
(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (/* binding */ kebabCase)
/* harmony export */ });
const SeparatorChars = ' _.';

/** @type {typeof $$.string.kebabCase} */
function kebabCase(val, effectFirst) {
  let result = '',
    needDash = false;
  for (let i = 0; i < val.length; i++) {
    const char = val[i];
    if (SeparatorChars.includes(char)) {
      needDash = true;
    } else if (char >= 'A' && char <= 'Z') {
      if (effectFirst || i) result += '-';
      result += char.toLowerCase();
      needDash = false;
    } else {
      if (needDash && result) {
        result += '-';
        needDash = false;
      }
      result += char;
    }
  }
  return result;
}

/***/ },

/***/ "./src/features sync recursive _test\\.idl\\.js$"
(module, __unused_webpack_exports, __webpack_require__) {

const map = {
	"./accordion/_test.idl.js": "./src/features/accordion/_test.idl.js",
	"./avatar/_test.idl.js": "./src/features/avatar/_test.idl.js",
	"./badge/_test.idl.js": "./src/features/badge/_test.idl.js",
	"./breadcrumb/_test.idl.js": "./src/features/breadcrumb/_test.idl.js",
	"./button/_test.idl.js": "./src/features/button/_test.idl.js",
	"./buttonfitbox/_test.idl.js": "./src/features/buttonfitbox/_test.idl.js",
	"./buttongroup/_test.idl.js": "./src/features/buttongroup/_test.idl.js",
	"./calendar/_test.idl.js": "./src/features/calendar/_test.idl.js",
	"./carousel/_test.idl.js": "./src/features/carousel/_test.idl.js",
	"./checkbox/_test.idl.js": "./src/features/checkbox/_test.idl.js",
	"./colorpicker/_test.idl.js": "./src/features/colorpicker/_test.idl.js",
	"./combobox/_test.idl.js": "./src/features/combobox/_test.idl.js",
	"./comboboxshell/_test.idl.js": "./src/features/comboboxshell/_test.idl.js",
	"./datepicker/_test.idl.js": "./src/features/datepicker/_test.idl.js",
	"./dialog/_test.idl.js": "./src/features/dialog/_test.idl.js",
	"./editor/_test.idl.js": "./src/features/editor/_test.idl.js",
	"./ellipsis/_test.idl.js": "./src/features/ellipsis/_test.idl.js",
	"./eventcalendar/_test.idl.js": "./src/features/eventcalendar/_test.idl.js",
	"./filepreview/_test.idl.js": "./src/features/filepreview/_test.idl.js",
	"./icon/_test.idl.js": "./src/features/icon/_test.idl.js",
	"./imgcrop/_test.idl.js": "./src/features/imgcrop/_test.idl.js",
	"./input/_test.idl.js": "./src/features/input/_test.idl.js",
	"./loading/_test.idl.js": "./src/features/loading/_test.idl.js",
	"./messagebar/_test.idl.js": "./src/features/messagebar/_test.idl.js",
	"./messagedialog/_test.idl.js": "./src/features/messagedialog/_test.idl.js",
	"./multicombobox/_test.idl.js": "./src/features/multicombobox/_test.idl.js",
	"./navpanel/_test.idl.js": "./src/features/navpanel/_test.idl.js",
	"./notification/_test.idl.js": "./src/features/notification/_test.idl.js",
	"./pager/_test.idl.js": "./src/features/pager/_test.idl.js",
	"./panel/_test.idl.js": "./src/features/panel/_test.idl.js",
	"./popover/_test.idl.js": "./src/features/popover/_test.idl.js",
	"./popup/_test.idl.js": "./src/features/popup/_test.idl.js",
	"./profile/_test.idl.js": "./src/features/profile/_test.idl.js",
	"./progressbar/_test.idl.js": "./src/features/progressbar/_test.idl.js",
	"./radio/_test.idl.js": "./src/features/radio/_test.idl.js",
	"./rangepicker/_test.idl.js": "./src/features/rangepicker/_test.idl.js",
	"./rating/_test.idl.js": "./src/features/rating/_test.idl.js",
	"./richcombobox/_test.idl.js": "./src/features/richcombobox/_test.idl.js",
	"./searchbox/_test.idl.js": "./src/features/searchbox/_test.idl.js",
	"./selection/_test.idl.js": "./src/features/selection/_test.idl.js",
	"./shimmer/_test.idl.js": "./src/features/shimmer/_test.idl.js",
	"./slider/_test.idl.js": "./src/features/slider/_test.idl.js",
	"./splitter/_test.idl.js": "./src/features/splitter/_test.idl.js",
	"./switch/_test.idl.js": "./src/features/switch/_test.idl.js",
	"./tabbutton/_test.idl.js": "./src/features/tabbutton/_test.idl.js",
	"./tabcontrol/_test.idl.js": "./src/features/tabcontrol/_test.idl.js",
	"./table/_test.idl.js": "./src/features/table/_test.idl.js",
	"./tag/_test.idl.js": "./src/features/tag/_test.idl.js",
	"./timeline/_test.idl.js": "./src/features/timeline/_test.idl.js",
	"./timepicker/_test.idl.js": "./src/features/timepicker/_test.idl.js",
	"./toast/_test.idl.js": "./src/features/toast/_test.idl.js",
	"./togglebutton/_test.idl.js": "./src/features/togglebutton/_test.idl.js",
	"./uploader/_test.idl.js": "./src/features/uploader/_test.idl.js",
	"./validation/_test.idl.js": "./src/features/validation/_test.idl.js"
};


function webpackContext(req) {
	const id = webpackContextResolve(req);
	return __webpack_require__(id);
}
function webpackContextResolve(req) {
	if(!__webpack_require__.o(map, req)) {
		const e = new Error("Cannot find module '" + req + "'");
		e.code = 'MODULE_NOT_FOUND';
		throw e;
	}
	return map[req];
}
webpackContext.keys = function webpackContextKeys() {
	return Object.keys(map);
};
webpackContext.resolve = webpackContextResolve;
module.exports = webpackContext;
webpackContext.id = "./src/features sync recursive _test\\.idl\\.js$";

/***/ }

/******/ });
/************************************************************************/
/******/ // The module cache
/******/ const __webpack_module_cache__ = {};
/******/ 
/******/ // The require function
/******/ function __webpack_require__(moduleId) {
/******/ 	// Check if module is in cache
/******/ 	const cachedModule = __webpack_module_cache__[moduleId];
/******/ 	if (cachedModule !== undefined) {
/******/ 		return cachedModule.exports;
/******/ 	}
/******/ 	// Create a new module (and put it into the cache)
/******/ 	const module = __webpack_module_cache__[moduleId] = {
/******/ 		// no module.id needed
/******/ 		// no module.loaded needed
/******/ 		exports: {}
/******/ 	};
/******/ 
/******/ 	// Execute the module function
/******/ 	if (!(moduleId in __webpack_modules__)) {
/******/ 		delete __webpack_module_cache__[moduleId];
/******/ 		const e = new Error("Cannot find module '" + moduleId + "'");
/******/ 		e.code = 'MODULE_NOT_FOUND';
/******/ 		throw e;
/******/ 	}
/******/ 	__webpack_modules__[moduleId](module, module.exports, __webpack_require__);
/******/ 
/******/ 	// Return the exports of the module
/******/ 	return module.exports;
/******/ }
/******/ 
/************************************************************************/
/******/ /* webpack/runtime/define property getters */
/******/ (() => {
/******/ 	// define getter/value functions for harmony exports
/******/ 	__webpack_require__.d = (exports, definition) => {
/******/ 		if(Array.isArray(definition)) {
/******/ 			var i = 0;
/******/ 			while(i < definition.length) {
/******/ 				var key = definition[i++];
/******/ 				var binding = definition[i++];
/******/ 				if(!__webpack_require__.o(exports, key)) {
/******/ 					if(binding === 0) {
/******/ 						Object.defineProperty(exports, key, { enumerable: true, value: definition[i++] });
/******/ 					} else {
/******/ 						Object.defineProperty(exports, key, { enumerable: true, get: binding });
/******/ 					}
/******/ 				} else if(binding === 0) { i++; }
/******/ 			}
/******/ 		} else {
/******/ 			for(var key in definition) {
/******/ 				if(__webpack_require__.o(definition, key) && !__webpack_require__.o(exports, key)) {
/******/ 					Object.defineProperty(exports, key, { enumerable: true, get: definition[key] });
/******/ 				}
/******/ 			}
/******/ 		}
/******/ 	};
/******/ })();
/******/ 
/******/ /* webpack/runtime/hasOwnProperty shorthand */
/******/ (() => {
/******/ 	__webpack_require__.o = (obj, prop) => (Object.prototype.hasOwnProperty.call(obj, prop))
/******/ })();
/******/ 
/******/ /* webpack/runtime/make namespace object */
/******/ (() => {
/******/ 	// define __esModule on exports
/******/ 	__webpack_require__.r = (exports) => {
/******/ 		if(Symbol.toStringTag) {
/******/ 			Object.defineProperty(exports, Symbol.toStringTag, { value: 'Module' });
/******/ 		}
/******/ 		Object.defineProperty(exports, '__esModule', { value: true });
/******/ 	};
/******/ })();
/******/ 
/************************************************************************/
let __webpack_exports__ = {};
// This entry needs to be wrapped in an IIFE because it needs to be isolated against other modules in the chunk.
(() => {
__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _core_element_text_contents_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__("./src/core/element/text-contents.js");
/* harmony import */ var _util_css_absolute_color_js__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__("./src/util/css/absolute-color.js");
/* harmony import */ var _util_string_kebab_case_js__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__("./src/util/string/kebab-case.js");



const {
  error
} = console;
const AriaKeys = Object.getOwnPropertyNames(Element.prototype).filter(x => x.startsWith('aria'));
const Util = {
  kebabCase: _util_string_kebab_case_js__WEBPACK_IMPORTED_MODULE_2__["default"],
  /** @returns {Record<string, string>} */
  arias(element) {
    if (!element || element.nodeType !== Node.ELEMENT_NODE) throw new Error(`Invalid element for arias!`);
    const result = {};
    for (const k of AriaKeys) {
      const v = element[k];
      if (typeof v !== 'function' && v != null && v !== '') result[k] = v;
    }
    return result;
  },
  delay(value = null, timeout = 0) {
    return new Promise(resolve => {
      setTimeout(() => resolve(value), timeout);
    });
  },
  absoluteColor: _util_css_absolute_color_js__WEBPACK_IMPORTED_MODULE_1__["default"],
  hex2rgb(value) {
    if (!value || !value.startsWith('#')) return value;
    const bigint = Number.parseInt(value.slice(1), 16);
    const r = bigint >> 16 & 255;
    const g = bigint >> 8 & 255;
    const b = bigint & 255;
    return `rgb(${r}, ${g}, ${b})`;
  },
  textContent(element, query = ':not([role="img"])') {
    return (0,_core_element_text_contents_js__WEBPACK_IMPORTED_MODULE_0__.textContents)(element, query).join('');
  }
};
class Widget {
  #proto = null;
  #proxy = null;
  #current = null;
  constructor(proto) {
    if (!proto) throw new Error('Widget IDL undefined!');
    this.#proto = proto;
    this.#proxy = new Proxy(this.#proto, {
      get: (cache, prop) => {
        const fn = cache[prop];
        if (fn) return fn.bind(this.#current);
        throw new Error(`Method ${prop} is not defined!`);
      }
    });
  }
  entry = element => {
    const arrayLike = element && typeof element[Symbol.iterator] === 'function',
      current = arrayLike ? element[0] : element;
    if (!current) error('Element is undefined!');
    this.#current = current || null;
    return this.#proxy;
  };
  get() {
    return this.entry;
  }
}

/**
 * @typedef {{
 *   [key: string]: (element: HTMLElement) => Record<string, (...args:any[]) => any>,
 *   setUIValue: typeof setUIValue,
 *   util: typeof Util,
 * }} AuiQuery
 */
/** @type {AuiQuery} */
const result = {};
const Files = __webpack_require__("./src/features sync recursive _test\\.idl\\.js$");
for (const file of Files.keys()) {
  const name = file.toLowerCase().split('/').at(-2),
    idl = Files(file).default;
  if (result[name] || name === 'util') {
    error(`Duplicate IDL for ${name} found!`);
  } else {
    result[name] = new Widget(idl).get();
  }
}

/** @param {Element} element  */
function getWidgetIDL(element) {
  if (!element) return null;
  const prefix = 'aui-',
    sliceCount = prefix.length;
  /** @param {string} v */
  const filter = v => {
    const name = v.slice(sliceCount).replaceAll('-', '');
    return Object.hasOwn(result, name) ? result[name](element) : null;
  };
  if (element.localName.startsWith(prefix)) return filter(element.localName);
  const identifier = 'aui-widget',
    // For legacy widgets (Pure React Widget or jQuery UI Widget).
    {
      classList
    } = element;
  if (classList.contains(identifier)) {
    // Search className reversely to get the most specific one, such as "aui-comboboxshell aui-datepicker".
    for (let i = classList.length - 1; i >= 0; i--) {
      const c = classList[i];
      if (c === identifier || !c.startsWith(prefix)) continue;
      const widget = filter(c);
      if (widget) return widget;
    }
  }
  return null;
}
/**
 * Set UI value (display content) for a widget.
 * Will overwrite the current value. The content format is determined by each widget, which can be string, number or boolean.
 * If the widget is disabled or the content is invalid, the promise will be rejected.
 * @param {HTMLElement} element The host element.
 * @param {string|number|boolean} content The display content to set.
 * @returns {Promise<void>} Will reject if set failed, otherwise resolved.
 */
function setUIValue(element, content) {
  const reject = v => Promise.reject(error(element) || v);
  const IDL = getWidgetIDL(element);
  if (!IDL) return reject('Widget cannot be recognized!');
  return IDL.setUIValue ? IDL.setUIValue(content) : reject(`setUIValue hasn't been supported!`);
}
Object.assign(result, {
  util: Util,
  setUIValue
});
Object.freeze(result);
globalThis._AuiQuery = result;
globalThis._Util = Util;
/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (result);
})();

const __webpack_exports__default = __webpack_exports__["default"];
export { __webpack_exports__default as default };
