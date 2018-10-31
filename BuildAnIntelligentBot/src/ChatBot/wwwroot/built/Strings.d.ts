export interface Strings {
    title: string;
    send: string;
    unknownFile: string;
    unknownCard: string;
    receiptTax: string;
    receiptVat: string;
    receiptTotal: string;
    messageRetry: string;
    messageFailed: string;
    messageSending: string;
    timeSent: string;
    consolePlaceholder: string;
    listeningIndicator: string;
    uploadFile: string;
    speak: string;
}
export declare const defaultStrings: Strings;
export declare const strings: (locale: string) => Strings;
