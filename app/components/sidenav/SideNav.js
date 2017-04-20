import React, { Component } from 'react';
import styles from './SideNav.css';

class SideNav extends Component {
  render() {
    const sideNav = `${styles.sideNav} col-3`;
    return (
      <div className={sideNav}>test</div>
    );
  }
}

export default SideNav;
