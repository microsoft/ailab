export interface RectangleProps {
  id: string;
  left: number;
  top: number;
  height: number;
  tooltipMessage: string;
  isHover: boolean;
}

export const createEmptyRectangleProps = (): RectangleProps => ({
  id: '',
  isHover: false,
  height: 0,
  left: 0,
  top: 0,
  tooltipMessage: '',
});
