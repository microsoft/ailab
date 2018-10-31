"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var tslib_1 = require("tslib");
var React = require("react");
var Attachment_1 = require("./Attachment");
var HScroll_1 = require("./HScroll");
var konsole = require("./Konsole");
var Carousel = (function (_super) {
    tslib_1.__extends(Carousel, _super);
    function Carousel(props) {
        return _super.call(this, props) || this;
    }
    Carousel.prototype.updateContentWidth = function () {
        //after the attachments have been rendered, we can now measure their actual width
        var width = this.props.size.width - this.props.format.carouselMargin;
        //important: remove any hard styling so that we can measure the natural width
        this.root.style.width = '';
        //now measure the natural offsetWidth
        if (this.root.offsetWidth > width) {
            // the content width is bigger than the space allotted, so we'll clip it to force scrolling
            this.root.style.width = width.toString() + "px";
            // since we're scrolling, we need to show scroll buttons
            this.hscroll.updateScrollButtons();
        }
    };
    Carousel.prototype.componentDidMount = function () {
        this.updateContentWidth();
    };
    Carousel.prototype.componentDidUpdate = function () {
        this.updateContentWidth();
    };
    Carousel.prototype.render = function () {
        var _this = this;
        return (React.createElement("div", { className: "wc-carousel", ref: function (div) { return _this.root = div; } },
            React.createElement(HScroll_1.HScroll, { ref: function (hscroll) { return _this.hscroll = hscroll; }, prevSvgPathData: "M 16.5 22 L 19 19.5 L 13.5 14 L 19 8.5 L 16.5 6 L 8.5 14 L 16.5 22 Z", nextSvgPathData: "M 12.5 22 L 10 19.5 L 15.5 14 L 10 8.5 L 12.5 6 L 20.5 14 L 12.5 22 Z", scrollUnit: "item" },
                React.createElement(CarouselAttachments, tslib_1.__assign({}, this.props)))));
    };
    return Carousel;
}(React.PureComponent));
exports.Carousel = Carousel;
var CarouselAttachments = (function (_super) {
    tslib_1.__extends(CarouselAttachments, _super);
    function CarouselAttachments() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    CarouselAttachments.prototype.render = function () {
        konsole.log("rendering CarouselAttachments");
        var _a = this.props, attachments = _a.attachments, props = tslib_1.__rest(_a, ["attachments"]);
        return (React.createElement("ul", null, this.props.attachments.map(function (attachment, index) {
            return React.createElement("li", { key: index, className: "wc-carousel-item" },
                React.createElement(Attachment_1.AttachmentView, { attachment: attachment, format: props.format, onCardAction: props.onCardAction, onImageLoad: props.onImageLoad }));
        })));
    };
    return CarouselAttachments;
}(React.PureComponent));
//# sourceMappingURL=Carousel.js.map