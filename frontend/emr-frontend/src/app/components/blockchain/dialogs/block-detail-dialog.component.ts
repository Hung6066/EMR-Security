// block-detail-dialog.component.ts
import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { BlockchainBlock } from '../../services/blockchain.service';

@Component({
  selector: 'app-block-detail-dialog',
  templateUrl: './block-detail-dialog.component.html',
  styleUrls: ['./block-detail-dialog.component.css']
})
export class BlockDetailDialogComponent {
  blockData: any;
  transactions: any[] = [];

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    private dialogRef: MatDialogRef<BlockDetailDialogComponent>
  ) {
    if (data.type === 'validation') {
      // Validation result
    } else {
      // Block details
      const block = data as BlockchainBlock;
      try {
        this.blockData = JSON.parse(block.data);
        this.transactions = this.blockData.transactions || [];
      } catch (e) {
        console.error('Error parsing block data:', e);
      }
    }
  }

  close(): void {
    this.dialogRef.close();
  }

  copyToClipboard(text: string): void {
    navigator.clipboard.writeText(text);
  }
}