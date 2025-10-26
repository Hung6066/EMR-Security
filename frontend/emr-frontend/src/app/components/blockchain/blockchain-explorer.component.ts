// blockchain-explorer.component.ts
import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { BlockchainService, BlockchainBlock, BlockchainTransaction } from '../../services/blockchain.service';
import { BlockDetailDialogComponent } from './block-detail-dialog.component';
import { TransactionDetailDialogComponent } from './transaction-detail-dialog.component';

@Component({
  selector: 'app-blockchain-explorer',
  templateUrl: './blockchain-explorer.component.html',
  styleUrls: ['./blockchain-explorer.component.css']
})
export class BlockchainExplorerComponent implements OnInit {
  blocks: BlockchainBlock[] = [];
  pendingTransactions: BlockchainTransaction[] = [];
  latestBlock: BlockchainBlock | null = null;
  
  displayedColumnsBlocks = ['index', 'timestamp', 'hash', 'transactions', 'status', 'actions'];
  displayedColumnsPending = ['transactionId', 'type', 'timestamp', 'user', 'actions'];

  chainValid = true;
  validating = false;
  mining = false;

  constructor(
    private blockchainService: BlockchainService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadBlockchain();
    this.loadPendingTransactions();
  }

  loadBlockchain(): void {
    this.blockchainService.getChain(0, 50).subscribe(
      data => {
        this.blocks = data;
      },
      error => console.error('Error loading blockchain:', error)
    );

    this.blockchainService.getLatestBlock().subscribe(
      data => {
        this.latestBlock = data;
      },
      error => console.error('Error loading latest block:', error)
    );
  }

  loadPendingTransactions(): void {
    this.blockchainService.getPendingTransactions().subscribe(
      data => {
        this.pendingTransactions = data;
      },
      error => console.error('Error loading pending transactions:', error)
    );
  }

  viewBlockDetails(block: BlockchainBlock): void {
    this.dialog.open(BlockDetailDialogComponent, {
      width: '900px',
      data: block
    });
  }

  viewTransactionDetails(transaction: BlockchainTransaction): void {
    this.dialog.open(TransactionDetailDialogComponent, {
      width: '700px',
      data: transaction
    });
  }

  validateChain(): void {
    this.validating = true;
    
    this.blockchainService.validateChain().subscribe(
      result => {
        this.chainValid = result.isValid;
        this.validating = false;
        
        const message = result.isValid 
          ? 'Blockchain hợp lệ! Tất cả blocks đều được xác thực.' 
          : 'Cảnh báo: Phát hiện lỗi trong blockchain!';
        
        this.snackBar.open(message, 'Đóng', { 
          duration: 5000,
          panelClass: result.isValid ? 'success-snackbar' : 'error-snackbar'
        });
      },
      error => {
        this.validating = false;
        this.snackBar.open('Lỗi khi validate blockchain', 'Đóng', { duration: 3000 });
      }
    );
  }

  performIntegrityCheck(): void {
    this.blockchainService.performIntegrityCheck().subscribe(
      validation => {
        const dialogRef = this.dialog.open(BlockDetailDialogComponent, {
          width: '700px',
          data: { type: 'validation', validation }
        });
      },
      error => {
        this.snackBar.open('Lỗi khi kiểm tra toàn vẹn', 'Đóng', { duration: 3000 });
      }
    );
  }

  mineNewBlock(): void {
    if (this.pendingTransactions.length === 0) {
      this.snackBar.open('Không có transaction nào để mine', 'Đóng', { duration: 3000 });
      return;
    }

    this.mining = true;
    
    this.blockchainService.mineBlock().subscribe(
      block => {
        this.mining = false;
        this.snackBar.open(`Block #${block.index} đã được mine thành công!`, 'Đóng', { duration: 3000 });
        this.loadBlockchain();
        this.loadPendingTransactions();
      },
      error => {
        this.mining = false;
        this.snackBar.open('Lỗi khi mine block', 'Đóng', { duration: 3000 });
      }
    );
  }

  getTransactionCount(block: BlockchainBlock): number {
    try {
      const data = JSON.parse(block.data);
      return data.transactions?.length || 0;
    } catch {
      return 0;
    }
  }

  getBlockColor(block: BlockchainBlock): string {
    return block.isValid ? 'primary' : 'warn';
  }

  shortenHash(hash: string): string {
    return `${hash.substring(0, 8)}...${hash.substring(hash.length - 8)}`;
  }

  copyToClipboard(text: string): void {
    navigator.clipboard.writeText(text).then(() => {
      this.snackBar.open('Đã sao chép', 'Đóng', { duration: 2000 });
    });
  }
}