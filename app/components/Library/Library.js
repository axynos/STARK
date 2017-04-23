import React, { Component } from 'react';
import styles from './Library.scss';
import Button from './Button';

class Library extends Component {
  render() {
    const functionRow = `${styles.functionRow} col-12`;
    const trackList = `${styles.trackList} col-12`;

    return (
      <div className="col-9">
        <div className="container">
          <div className={functionRow}>
            <Button icon="library_add" />
          </div>
          <div className={trackList}>list-box</div>
        </div>
      </div>
    );
  }
}

export default Library;
