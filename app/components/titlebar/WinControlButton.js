import React, { Component } from 'react';
import PropTypes from 'prop-types';

class WinControlButton extends Component {
  render() {
    // TODO rework this to use an image / icon from a font.
    const icon = (this.props.type === 'CLOSE') ? 'X' : '_';

    return (
      // window.currentWindow is defined in app.html along with an apology for doing it this way.
      <button onClick={() => (this.props.type === 'CLOSE' ? window.currentWindow.close() : window.currentWindow.minimize())}>{icon}</button>
    );
  }
}

WinControlButton.propTypes = {
  type: PropTypes.string.isRequired,
};

export default WinControlButton;
