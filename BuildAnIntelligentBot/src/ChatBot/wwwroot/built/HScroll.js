"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var tslib_1 = require("tslib");
var React = require("react");
var Observable_1 = require("rxjs/Observable");
require("rxjs/add/observable/fromEvent");
require("rxjs/add/observable/merge");
var HScroll = (function (_super) {
    tslib_1.__extends(HScroll, _super);
    function HScroll(props) {
        return _super.call(this, props) || this;
    }
    HScroll.prototype.clearScrollTimers = function () {
        clearInterval(this.scrollStartTimer);
        clearInterval(this.scrollSyncTimer);
        clearTimeout(this.scrollDurationTimer);
        document.body.removeChild(this.animateDiv);
        this.animateDiv = null;
        this.scrollStartTimer = null;
        this.scrollSyncTimer = null;
        this.scrollDurationTimer = null;
    };
    HScroll.prototype.updateScrollButtons = function () {
        this.prevButton.disabled = !this.scrollDiv || Math.round(this.scrollDiv.scrollLeft) <= 0;
        this.nextButton.disabled = !this.scrollDiv || Math.round(this.scrollDiv.scrollLeft) >= Math.round(this.scrollDiv.scrollWidth - this.scrollDiv.offsetWidth);
    };
    HScroll.prototype.componentDidMount = function () {
        var _this = this;
        this.scrollDiv.style.marginBottom = -(this.scrollDiv.offsetHeight - this.scrollDiv.clientHeight) + 'px';
        this.scrollSubscription = Observable_1.Observable.fromEvent(this.scrollDiv, 'scroll').subscribe(function (_) {
            _this.updateScrollButtons();
        });
        this.clickSubscription = Observable_1.Observable.merge(Observable_1.Observable.fromEvent(this.prevButton, 'click').map(function (_) { return -1; }), Observable_1.Observable.fromEvent(this.nextButton, 'click').map(function (_) { return 1; })).subscribe(function (delta) {
            _this.scrollBy(delta);
        });
        this.updateScrollButtons();
    };
    HScroll.prototype.componentDidUpdate = function () {
        this.scrollDiv.scrollLeft = 0;
        this.updateScrollButtons();
    };
    HScroll.prototype.componentWillUnmount = function () {
        this.scrollSubscription.unsubscribe();
        this.clickSubscription.unsubscribe();
    };
    HScroll.prototype.scrollAmount = function (direction) {
        if (this.props.scrollUnit == 'item') {
            // TODO: this can be improved by finding the actual item in the viewport,
            // instead of the first item, because they may not have the same width.
            // the width of the li is measured on demand in case CSS has resized it
            var firstItem = this.scrollDiv.querySelector('ul > li');
            return firstItem ? direction * firstItem.offsetWidth : 0;
        }
        else {
            // TODO: use a good page size. This can be improved by finding the next clipped item.
            return direction * (this.scrollDiv.offsetWidth - 70);
        }
    };
    HScroll.prototype.scrollBy = function (direction) {
        var _this = this;
        var easingClassName = 'wc-animate-scroll';
        //cancel existing animation when clicking fast
        if (this.animateDiv) {
            easingClassName = 'wc-animate-scroll-rapid';
            this.clearScrollTimers();
        }
        var unit = this.scrollAmount(direction);
        var scrollLeft = this.scrollDiv.scrollLeft;
        var dest = scrollLeft + unit;
        //don't exceed boundaries
        dest = Math.max(dest, 0);
        dest = Math.min(dest, this.scrollDiv.scrollWidth - this.scrollDiv.offsetWidth);
        if (scrollLeft == dest)
            return;
        //use proper easing curve when distance is small
        if (Math.abs(dest - scrollLeft) < 60) {
            easingClassName = 'wc-animate-scroll-near';
        }
        this.animateDiv = document.createElement('div');
        this.animateDiv.className = easingClassName;
        this.animateDiv.style.left = scrollLeft + 'px';
        document.body.appendChild(this.animateDiv);
        //capture ComputedStyle every millisecond
        this.scrollSyncTimer = window.setInterval(function () {
            var num = parseFloat(getComputedStyle(_this.animateDiv).left);
            _this.scrollDiv.scrollLeft = num;
        }, 1);
        //don't let the browser optimize the setting of 'this.animateDiv.style.left' - we need this to change values to trigger the CSS animation
        //we accomplish this by calling 'this.animateDiv.style.left' off this thread, using setTimeout
        this.scrollStartTimer = window.setTimeout(function () {
            _this.animateDiv.style.left = dest + 'px';
            var duration = 1000 * parseFloat(getComputedStyle(_this.animateDiv).transitionDuration);
            if (duration) {
                //slightly longer that the CSS time so we don't cut it off prematurely
                duration += 50;
                //stop capturing
                _this.scrollDurationTimer = window.setTimeout(function () { return _this.clearScrollTimers(); }, duration);
            }
            else {
                _this.clearScrollTimers();
            }
        }, 1);
    };
    HScroll.prototype.render = function () {
        var _this = this;
        return (React.createElement("div", null,
            React.createElement("button", { className: "scroll previous", disabled: true, ref: function (button) { return _this.prevButton = button; }, type: "button" },
                React.createElement("svg", null,
                    React.createElement("path", { d: this.props.prevSvgPathData }))),
            React.createElement("div", { className: "wc-hscroll-outer" },
                React.createElement("div", { className: "wc-hscroll", ref: function (div) { return _this.scrollDiv = div; } }, this.props.children)),
            React.createElement("button", { className: "scroll next", disabled: true, ref: function (button) { return _this.nextButton = button; }, type: "button" },
                React.createElement("svg", null,
                    React.createElement("path", { d: this.props.nextSvgPathData })))));
    };
    return HScroll;
}(React.Component));
exports.HScroll = HScroll;
//# sourceMappingURL=HScroll.js.map