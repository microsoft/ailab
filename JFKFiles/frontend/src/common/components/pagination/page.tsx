import * as React from 'react';
import Button from 'material-ui/Button';


interface Props {
  pageText: (string | React.ReactNode); // Review this Element not sure if make sense
  pageNumber: number;
  onClick: (pageNumber : number) => void;
  isActive?: boolean;
  isDisabled?: boolean,
  classes?: any;
}


const handleClick = ({onClick, pageNumber} : Props) => (e) => {
  e.preventDefault();
  onClick(pageNumber);
}


export const Page : React.StatelessComponent<Props> = (props) => {
  return (
    !props.isDisabled &&
    <Button
      classes={props.classes}
      onClick={handleClick(props)}
      color={props.isActive ? 'primary' : 'inherit'}
      variant={"fab"}
      mini={true}
    >
      {props.pageText}
    </Button>
  );
}

Page.defaultProps = {
  isActive: false,
  isDisabled: false,
};
