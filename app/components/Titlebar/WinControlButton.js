import React, { Component } from 'react';
import PropTypes from 'prop-types';
import styles from './WinControlButton.scss';

class WinControlButton extends Component {
  render() {
    const buttonStyle = `${styles.button}`;

    return (
      <button className={buttonStyle} onClick={this.props.onClick}>
        <i className="material-icons">{this.props.icon}</i>
      </button>
    );
  }
}

WinControlButton.propTypes = {
  onClick: PropTypes.func.isRequired,
  icon: PropTypes.string.isRequired
};

export default WinControlButton;
