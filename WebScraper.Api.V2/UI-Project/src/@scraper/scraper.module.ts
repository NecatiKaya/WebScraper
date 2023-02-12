import { HttpClient } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatRippleModule } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSortModule } from '@angular/material/sort';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Route, RouterModule } from '@angular/router';
import { SharedModule } from 'app/shared/shared.module';
import { ProductDefinitionComponent } from './modules/scraping/components/product-definition/product-definition.component';
import { ProductListComponent } from './modules/scraping/components/product-list/product-list.component';
import { ScraperApi } from './modules/scraping/services/scraper-api';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatCardModule } from '@angular/material/card';
import { MatDialogModule } from '@angular/material/dialog';
import { MatTableModule } from '@angular/material/table';
import { ScraperVisitListComponent } from './modules/scraping/components/scraper-visit-list/scraper-visit-list.component';
import { ProgressComponent } from './modules/scraping/components/progress-bar/progress-bar.component';
import { FileUploadDialogComponent } from './modules/scraping/components/file-upload-dialog/file-upload-dialog.component';
import { FileUploadComponent } from './modules/scraping/components/file-upload-component/file-upload.component';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

export const scraperRoutes: Route[] = [
    {
        path: 'products',
        children: [
            {
                path: 'list',
                component: ProductListComponent
            },
            {
                path: ':id',
                component: ProductDefinitionComponent
            }
        ]
    },
    {
        path: 'visits',
        children: [
            {
                path: 'list',
                component: ScraperVisitListComponent
            }
        ]
    }
];

@NgModule({
    declarations: [
        ProductDefinitionComponent,
        ProductListComponent,
        ScraperVisitListComponent,
        FileUploadComponent,
        FileUploadDialogComponent,
        ProgressComponent
    ],
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        RouterModule.forChild(scraperRoutes),
        MatButtonModule,
        MatCheckboxModule,
        MatFormFieldModule,
        MatIconModule,
        MatInputModule,
        MatMenuModule,
        MatPaginatorModule,
        MatProgressBarModule,
        MatRippleModule,
        MatSortModule,
        MatSelectModule,
        MatSlideToggleModule,
        MatTooltipModule,
        SharedModule,
        MatGridListModule,
        MatCardModule,
        MatDialogModule,
        MatTableModule,
        MatPaginatorModule
    ],
    providers: [{
        provide: ScraperApi,
        useFactory: (httpClient: HttpClient): ScraperApi => new ScraperApi(httpClient),
        deps: [HttpClient]
    }]
})
export class ScraperModule {

}
