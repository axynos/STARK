import React, { Component } from 'react';
import PropTypes from 'prop-types';

class Button extends Component {
  render() {
    return (
      <div>
        <i className="material-icons">{this.props.icon}</i>{this.props.label}
      </div>
    );
  }
}

Button.defaultProps = {
  label: ''
};

Button.propTypes = {
  icon: PropTypes.string.isRequired,
  label: PropTypes.string
};

export default Button;
