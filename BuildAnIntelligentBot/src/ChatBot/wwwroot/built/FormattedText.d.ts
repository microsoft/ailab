/// <reference types="react" />
export interface IFormattedTextProps {
    text: string;
    format: string;
    onImageLoad: () => void;
}
export declare const FormattedText: (props: IFormattedTextProps) => JSX.Element;
