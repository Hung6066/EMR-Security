import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { FimService, FileIntegrityRecord } from '../../../services/fim.service'; // Tạo service này

@Component({
  selector: 'app-fim',
  templateUrl: './fim.component.html',
  styleUrls: ['./fim.component.css']
})
export class FimComponent implements OnInit {
  records: FileIntegrityRecord[] = [];
  displayedColumns = ['status', 'filePath', 'hash', 'lastChecked', 'actions'];
  loading = true;
  scanning = false;

  constructor(
    private fimService: FimService,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit(): void {
    this.loadStatus();
  }

  loadStatus(): void {
    this.loading = true;
    this.fimService.getStatus().subscribe(data => {
      this.records = data;
      this.loading = false;
    });
  }

  scan(): void {
    this.scanning = true;
    this.snackBar.open('Đang quét... Vui lòng đợi.', 'OK');
    this.fimService.scan().subscribe(changes => {
      this.snackBar.open(`Quét hoàn tất. Phát hiện ${changes.length} thay đổi.`, 'OK', { duration: 5000 });
      this.loadStatus();
      this.scanning = false;
    });
  }

  createBaseline(): void {
    if (confirm('Hành động này sẽ xóa baseline cũ và tạo mới. Bạn chắc chắn?')) {
      this.fimService.createBaseline().subscribe(() => {
        this.snackBar.open('Đã tạo baseline mới thành công.', 'OK', { duration: 3000 });
        this.loadStatus();
      });
    }
  }

  acknowledge(record: FileIntegrityRecord): void {
    this.fimService.acknowledge(record.id).subscribe(() => {
      this.snackBar.open(`Đã xác nhận thay đổi cho file: ${record.filePath}`, 'OK', { duration: 3000 });
      this.loadStatus();
    });
  }

  getStatusColor(status: string): string {
    switch (status.toLowerCase()) {
      case 'changed':
      case 'deleted':
        return 'warn';
      case 'new':
        return 'accent';
      case 'acknowledged':
        return '';
      default:
        return 'primary';
    }
  }

  getStatusIcon(status: string): string {
    switch (status.toLowerCase()) {
      case 'changed': return 'sync_alt';
      case 'deleted': return 'delete_forever';
      case 'new': return 'add_circle';
      case 'acknowledged': return 'check';
      default: return 'check_circle';
    }
  }
}