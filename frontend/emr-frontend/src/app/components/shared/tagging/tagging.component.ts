// src/app/components/shared/tagging/tagging.component.ts
import { Component, Input, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { ClassificationService, Tag } from '../../../services/classification.service';
import {COMMA, ENTER} from '@angular/cdk/keycodes';
import {MatChipInputEvent} from '@angular/material/chips';

@Component({
  selector: 'app-tagging',
  templateUrl: './tagging.component.html',
  styleUrls: ['./tagging.component.css']
})
export class TaggingComponent implements OnInit {
  @Input() resourceType!: string;
  @Input() resourceId!: number;
  
  tags: Tag[] = [];
  separatorKeysCodes: number[] = [ENTER, COMMA];

  constructor(private classificationService: ClassificationService) {}

  ngOnInit(): void {
    if (this.resourceType && this.resourceId) {
      this.loadTags();
    }
  }

  loadTags(): void {
    this.classificationService.getTags(this.resourceType, this.resourceId).subscribe(
      data => {
        this.tags = data;
      }
    );
  }

  add(event: MatChipInputEvent): void {
    const value = (event.value || '').trim();
    if (value) {
      this.classificationService.addTag(this.resourceType, this.resourceId, value).subscribe(() => {
        this.loadTags();
      });
    }
    event.chipInput!.clear();
  }

  remove(tag: Tag): void {
    // Implement remove tag service call if needed
    // For now, just remove from view for demo
    const index = this.tags.indexOf(tag);
    if (index >= 0) {
      this.tags.splice(index, 1);
    }
  }
}