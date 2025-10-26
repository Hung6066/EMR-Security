// behavioral-tracking.directive.ts
import { Directive, OnInit, OnDestroy } from '@angular/core';
import { BehavioralAnalyticsService } from '../services/behavioral-analytics.service';

@Directive({
  selector: '[appBehavioralTracking]'
})
export class BehavioralTrackingDirective implements OnInit, OnDestroy {
  constructor(private behavioralService: BehavioralAnalyticsService) {}

  ngOnInit(): void {
    this.behavioralService.startTracking();
  }

  ngOnDestroy(): void {
    this.behavioralService.stopTracking();
  }
}