import React, { Component } from 'react';
import WinControlButton from './WinControlButton';
import styles from './TitleBar.scss';

export default class TitleBar extends Component {
  render() {
    const outerContainerClasses = `${styles.titlebar} row no-gutters`;
    const titleContainer = `${styles.titleContainer} col-11 row no-gutters align-items-center justify-content-center`;
    const controlContainer = `${styles.controlContainer} col-1 row no-gutters align-items-center justify-content-center`;

    return (
      <div className={outerContainerClasses}>
        <div className={titleContainer}>STARK by axynos</div>
        <div className={controlContainer}>
          <WinControlButton type="MINIMIZE" />
          <WinControlButton type="CLOSE" />
        </div>
      </div>
    );
  }
}
