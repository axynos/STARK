import React, { Component } from 'react';
import PropTypes from 'prop-types';
import styles from './SideNavButton.scss';

class SideNavButton extends Component {
  render() {
    const row = `${styles.row} row ${this.props.active ? styles.active : ''}`;
    const iconContainer = `${styles.iconContainer} col-4`;
    const style = { color: this.props.iconColor };
    const labelContainer = `${styles.labelContainer} col-8`;

    return (
      <button className={row} onClick={() => {}}>
        <div className={iconContainer}>
          <i className="material-icons md-24" style={style}>{this.props.icon}</i>
        </div>
        <div className={labelContainer}>{this.props.label}</div>
      </button>);
  }
}

SideNavButton.propTypes = {
  icon: PropTypes.string.isRequired,
  iconColor: PropTypes.string,
  active: PropTypes.bool,
  label: PropTypes.string.isRequired
  // onClick: PropTypes.func.isRequired
};

SideNavButton.defaultProps = {
  iconColor: '#fcf7f8',
  active: false
};

export default SideNavButton;
