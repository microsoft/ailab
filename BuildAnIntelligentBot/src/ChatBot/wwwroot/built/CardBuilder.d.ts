import { CardAction } from 'botframework-directlinejs';
import { AdaptiveCard, CardElement, Column, Container, TextBlock } from 'adaptivecards';
export declare class AdaptiveCardBuilder {
    private container;
    card: AdaptiveCard;
    constructor();
    addColumnSet(sizes: number[], container?: Container): Column[];
    addItems(cardElements: CardElement[], container?: Container): void;
    addTextBlock(text: string, template: Partial<TextBlock>, container?: Container): void;
    addButtons(cardActions: CardAction[], includesOAuthButtons?: boolean): void;
    private static addCardAction(cardAction, includesOAuthButtons?);
    addCommonHeaders(content: ICommonContent): void;
    addCommon(content: ICommonContent): void;
    addImage(url: string, container?: Container, selectAction?: CardAction): void;
}
export interface ICommonContent {
    title?: string;
    subtitle?: string;
    text?: string;
    buttons?: CardAction[];
}
export declare const buildCommonCard: (content: ICommonContent) => AdaptiveCard;
export declare const buildOAuthCard: (content: ICommonContent) => AdaptiveCard;
