import * as React from "react";
import Button from "material-ui/Button";
const styles = require('./link.styles.scss');

interface Props {
  to: string;
  target?: string;
}

export const LinkComponent: React.StatelessComponent<Props> = ({ to, target, children }) => {
  return (
    <Button variant="flat" color="primary" href={to} target={target} className={styles.link}>
      {children}
    </Button>
  );
};

LinkComponent.defaultProps = {
  target: '_blank',
};
