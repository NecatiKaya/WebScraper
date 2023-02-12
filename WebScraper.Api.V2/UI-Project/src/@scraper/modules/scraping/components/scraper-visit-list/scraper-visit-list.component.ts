import { trigger, state, style, transition, animate } from '@angular/animations';
import { AfterViewInit, Component, OnDestroy, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { ActivatedRoute, Router } from '@angular/router';
import { UntilDestroy } from '@ngneat/until-destroy';
import { ServerResponseBase } from '@scraper/modules/shared/server-response-base';
import { ToastrService } from 'ngx-toastr';
import { catchError, map, merge, of, startWith, switchMap } from 'rxjs';
import { ScraperVisit } from '../../models/scraper-visit.model';
import { ScraperApi } from '../../services/scraper-api';
import { shortenText } from '../../utilities';

@UntilDestroy()
@Component({
    selector: 'product-definition',
    templateUrl: './scraper-visit-list.component.html',
    styleUrls: ['./scraper-visit-list.component.scss'],
    encapsulation: ViewEncapsulation.None,
    animations: [
        trigger('detailExpand', [
            state('void', style({ height: '0px', minHeight: '0', visibility: 'hidden' })),
            state('*', style({ height: '*', visibility: 'visible' })),
            transition('void <=> *', animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)')),
        ]),
    ],
})
export class ScraperVisitListComponent implements OnInit, OnDestroy, AfterViewInit {

    @ViewChild(MatPaginator) private _paginator: MatPaginator;
    @ViewChild(MatSort) private _sort: MatSort;

    // eslint-disable-next-line @typescript-eslint/member-ordering
    @ViewChild(MatPaginator) paginator!: MatPaginator;
    // eslint-disable-next-line @typescript-eslint/member-ordering
    @ViewChild(MatSort) sort!: MatSort;

    // eslint-disable-next-line max-len
    displayedColumns: string[] = [
        'visitDate', 'productName', 'amazonPreviousPrice', 'amazonCurrentPrice', 'amazonCurrentDiscountAsAmount', 'amazonCurrentDiscountAsPercentage', 'trendyolPreviousPrice',
        'trendyolCurrentPrice', 'trendyolCurrentDiscountAsAmount', 'trendyolCurrentDiscountAsPercentage', 'requestedPriceDifferenceAsPercentage',
        'requestedPriceDifferenceAsAmount', 'calculatedPriceDifferenceAsPercentage', 'calculatedPriceDifferenceAsAmount', 'needToNotify', 'notified'];

    data: ScraperVisit[] = [];
    isLoading = false;
    pageIndex: number = 0;
    pageSize: number = 50;
    sortKey = 'visitDate';
    sortOrder = 'desc';

    resultsLength = 0;
    isLoadingResults = true;

    constructor(private route: ActivatedRoute,
        private router: Router,
        private scraperApi: ScraperApi,
        private toastr: ToastrService) {

    }

    ngOnInit(): void {

    }

    ngAfterViewInit(): void {
        // If the user changes the sort order, reset back to the first page.
        this.sort.sortChange.subscribe(() => (this.paginator.pageIndex = 0));

        merge(this.sort.sortChange, this.paginator.page)
            .pipe(
                startWith({}),
                switchMap(() => {
                    this.isLoadingResults = true;
                    return this.scraperApi.getVisits(
                        {
                            pageIndex: this.paginator.pageIndex,
                            pageSize: this._paginator.pageSize,
                            sortDirection: this.sort.direction === 'asc' ? 'asc' : 'desc',
                            sortKey: this.sort.active
                        }
                    ).pipe(catchError((err) => {
                        this.toastr.error('Could not get web visit data. Check console please.', 'Error');
                        console.log(err);
                        return of(null);
                    }));
                }),
                map((serverResponse: ServerResponseBase<ScraperVisit>) => {
                    this.isLoadingResults = false;

                    if (!serverResponse || !serverResponse.isSuccess) {
                        return [];
                    }

                    this.resultsLength = serverResponse?.totalRowCount ?? 0;
                    return serverResponse.data;
                }),
            )
            .subscribe(data => (this.data = data));
    }

    ngOnDestroy(): void {

    }

    makeShorter(value: string, length: number): string {
        return shortenText(value, length);
    }
}
