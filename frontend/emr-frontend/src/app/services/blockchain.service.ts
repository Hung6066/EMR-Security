// blockchain.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface BlockchainBlock {
  id: number;
  index: number;
  timestamp: Date;
  data: string;
  hash: string;
  previousHash: string;
  nonce: number;
  difficulty: number;
  merkleRoot: string;
  isValid: boolean;
}

export interface BlockchainTransaction {
  id: number;
  transactionId: string;
  transactionType: string;
  payload: string;
  userId: number;
  userName?: string;
  timestamp: Date;
  blockId?: number;
  transactionHash: string;
  isConfirmed: boolean;
}

export interface BlockchainValidation {
  id: number;
  validationTime: Date;
  isValid: boolean;
  totalBlocks: number;
  validBlocks: number;
  invalidBlocks: number;
  validationDetails: string;
}

@Injectable({
  providedIn: 'root'
})
export class BlockchainService {
  private apiUrl = 'http://localhost:5000/api/blockchain';

  constructor(private http: HttpClient) {}

  getChain(skip: number = 0, take: number = 100): Observable<BlockchainBlock[]> {
    return this.http.get<BlockchainBlock[]>(`${this.apiUrl}/chain?skip=${skip}&take=${take}`);
  }

  getLatestBlock(): Observable<BlockchainBlock> {
    return this.http.get<BlockchainBlock>(`${this.apiUrl}/latest`);
  }

  getTransaction(transactionId: string): Observable<BlockchainTransaction> {
    return this.http.get<BlockchainTransaction>(`${this.apiUrl}/transaction/${transactionId}`);
  }

  addTransaction(type: string, data: any): Observable<BlockchainTransaction> {
    return this.http.post<BlockchainTransaction>(`${this.apiUrl}/transaction`, { type, data });
  }

  validateChain(): Observable<{ isValid: boolean }> {
    return this.http.post<{ isValid: boolean }>(`${this.apiUrl}/validate`, {});
  }

  performIntegrityCheck(): Observable<BlockchainValidation> {
    return this.http.post<BlockchainValidation>(`${this.apiUrl}/integrity-check`, {});
  }

  getPendingTransactions(): Observable<BlockchainTransaction[]> {
    return this.http.get<BlockchainTransaction[]>(`${this.apiUrl}/pending`);
  }

  verifyTransaction(transactionId: string): Observable<{ isValid: boolean }> {
    return this.http.post<{ isValid: boolean }>(`${this.apiUrl}/verify/${transactionId}`, {});
  }

  mineBlock(): Observable<BlockchainBlock> {
    return this.http.post<BlockchainBlock>(`${this.apiUrl}/mine`, {});
  }
}