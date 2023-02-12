import { Component, Input } from '@angular/core';
import { UntilDestroy } from '@ngneat/until-destroy';

@UntilDestroy()
@Component({
    selector: 'progress-bar',
    templateUrl: './progress-bar.component.html',
})
export class ProgressComponent {
    @Input() progress = 0;
}
