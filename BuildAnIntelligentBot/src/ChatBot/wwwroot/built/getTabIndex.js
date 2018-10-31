"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var IE_FOCUSABLE_LIST = ['a', 'body', 'button', 'frame', 'iframe', 'img', 'input', 'isindex', 'object', 'select', 'textarea'];
var IS_FIREFOX = /Firefox\//i.test(navigator.userAgent);
var IS_IE = /Trident\//i.test(navigator.userAgent);
function getTabIndex(element) {
    var tabIndex = element.tabIndex;
    if (IS_IE) {
        var tabIndexAttribute = element.attributes.getNamedItem('tabindex');
        if (!tabIndexAttribute || !tabIndexAttribute.specified) {
            return ~IE_FOCUSABLE_LIST.indexOf(element.nodeName.toLowerCase()) ? 0 : null;
        }
    }
    else if (!~tabIndex) {
        var attr = element.getAttribute('tabindex');
        if (attr === null || (attr === '' && !IS_FIREFOX)) {
            return null;
        }
    }
    return tabIndex;
}
exports.getTabIndex = getTabIndex;
;
//# sourceMappingURL=getTabIndex.js.map