import { AfterViewInit, Component, OnDestroy, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { UntypedFormControl, FormBuilder, FormGroup } from '@angular/forms';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSort, Sort } from '@angular/material/sort';
import { FuseConfirmationService } from '@fuse/services/confirmation';
import { InventoryService } from 'app/modules/admin/apps/ecommerce/inventory/inventory.service';
import { InventoryProduct } from 'app/modules/admin/apps/ecommerce/inventory/inventory.types';
import { Observable, takeUntil, debounceTime, switchMap, map, merge, catchError, of, exhaustMap, finalize, tap, take, EMPTY, filter, distinctUntilChanged, mergeMap } from 'rxjs';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { ScraperApi } from '../../services/scraper-api';
import { ProductModel } from '../../models/product.model';
import { fuseAnimations } from '@fuse/animations';
import { ServerResponseBase } from '@scraper/modules/shared/server-response-base';
import { MatDialog } from '@angular/material/dialog';
import { ProductDefinitionComponent } from '../product-definition/product-definition.component';
import { ToastrService } from 'ngx-toastr';
import { FileUploadDialogComponent } from '../file-upload-dialog/file-upload-dialog.component';

@UntilDestroy()
@Component({
    selector: 'product-list',
    templateUrl: './product-list.component.html',
    styleUrls: ['./product-list.component.scss'],
    encapsulation: ViewEncapsulation.None,
    styles: [
        `
            .inventory-grid {
                grid-template-columns: 48px auto 40px;

                @screen sm {
                    grid-template-columns: 48px auto 112px 72px;
                }

                @screen md {
                    grid-template-columns: 48px 112px auto 112px 72px;
                }

                @screen lg {
                    grid-template-columns: 60px auto 100px 100px 200px 100px 100px 100px 100px 72px 100px;
                }
            }
        `
    ],
    animations: fuseAnimations
})
export class ProductListComponent implements OnInit, OnDestroy, AfterViewInit {

    @ViewChild(MatPaginator) private _paginator: MatPaginator;
    @ViewChild(MatSort) private _sort: MatSort;

    // eslint-disable-next-line @typescript-eslint/member-ordering
    @ViewChild(MatPaginator) paginator!: MatPaginator;
    // eslint-disable-next-line @typescript-eslint/member-ordering
    @ViewChild(MatSort) sort!: MatSort;

    form!: FormGroup;
    products$: Observable<InventoryProduct[]>;
    searchInputControl: UntypedFormControl = new UntypedFormControl();
    isLoading = false;
    products: ProductModel[] = [];

    pageIndex: number = 0;
    pageSize: number = 50;
    sortKey = 'name';
    sortDescription = 'asc';
    totalRowCount = 0;

    constructor(
        private _fuseConfirmationService: FuseConfirmationService,
        /* private _inventoryService: InventoryService, */
        private route: ActivatedRoute,
        private router: Router,
        private scraperApi: ScraperApi,
        public matDialog: MatDialog,
        private toastr: ToastrService) {
    }

    @ViewChild(MatSort) set matSort(ms: MatSort) {
        this.sort = ms;
    }

    @ViewChild(MatPaginator) set matPaginator(mp: MatPaginator) {
        this.paginator = mp;
        if (this.paginator) {
            this.paginator.page
                .subscribe(() => {
                    const queryParams = { pageSize: this.paginator.pageSize, pageIndex: this.paginator.pageIndex };
                    this.router.navigate(
                        [],
                        {
                            replaceUrl: true,
                            relativeTo: this.route,
                            queryParams: queryParams,
                            queryParamsHandling: 'merge',
                        });
                });
        }
    }

    ngOnInit(): void {
        this.route.queryParams.subscribe((params: Params) => {
            this.getAllProducts(params['sortKey'],
                params['sortDirection'],
                params['pageIndex'],
                params['pageSize']);
        });
    }

    ngAfterViewInit(): void {
        this.searchInputControl?.valueChanges.pipe(
            untilDestroyed(this),
            distinctUntilChanged(),
            /* filter((value: string) => value?.length > 2), */
            mergeMap((val: string) => this.scraperApi.getProductByNameLike(val)),
            catchError((err) => {
                console.log(err);
                this.toastr.error('Error occured. Please check console.', 'Error');
                return EMPTY;
            })
        ).subscribe((response: ServerResponseBase<ProductModel>) => {
            const data = response.isSuccess ? response.data : [];
            this.products = data;
            this.totalRowCount = response?.totalRowCount ?? 0;
        });
    }

    ngOnDestroy(): void {

    }

    preFetch(sort: Sort): void {
        if (!this.sort.active) {
            return;
        }

        const queryParams = { sortKet: sort.active, sortDirection: sort.direction };
        this.router.navigate(
            [],
            {
                replaceUrl: true,
                relativeTo: this.route,
                queryParams: queryParams,
                queryParamsHandling: 'merge',
            });
    }

    trackByFn(index: number, item: any): any {
        return item.id || index;
    }

    addProduct(): void {
        this.matDialog.open(ProductDefinitionComponent, {
            height: '650px',
            width: '600px'
        }).afterClosed().subscribe((dialogResult) => {
            if (dialogResult === 'success') {
                this.getAllProducts();
            }
        });
    }

    getAllProducts(sortKey: string = 'name', sortDirection: string = 'asc', pageIndex: number = 0, pageSize: number = 50): void {
        this.scraperApi.getAllProducts(sortKey, sortDirection, pageIndex, pageSize)
            .pipe(
                take(1),
                tap(() => {
                    this.isLoading = true;
                }),
                untilDestroyed(this),
                catchError((error: any) => {
                    this.toastr.error('Error occured. Please check console.', 'Error');
                    console.log(error);
                    return of([]);
                }),
                finalize(() => {
                    this.isLoading = false;
                })
            )
            .subscribe((response: ServerResponseBase<ProductModel>) => {
                const data = response.isSuccess ? response.data : [];
                this.products = data;
                this.totalRowCount = response?.totalRowCount ?? 0;
            });
    }

    updateProduct(product: ProductModel): void {
        this.matDialog.open(ProductDefinitionComponent, {
            height: '650px',
            width: '600px',
            data: {
                entityId: product.id
            }
        }).afterClosed().subscribe((dialogResult) => {
            if (dialogResult === 'success') {
                this.getAllProducts();
            }
        });
    }

    deleteProduct(product: ProductModel): void {
        const dialogRef = this._fuseConfirmationService.open({
            dismissible: true,
            message: 'Are you sure to delete item ' + product.id + ' - ' + product.name,
            title: 'Delete Operation'
        });

        dialogRef.afterClosed().subscribe((result) => {
            if (result === 'confirmed') {
                this.scraperApi.deleteProductById(product.id).pipe(
                    untilDestroyed(this),
                    take(1),
                    catchError((error) => {
                        console.log(error);
                        this.toastr.error('Error occured on delete. Check console for error.', 'Error');
                        return EMPTY;
                    })
                ).subscribe((response: ServerResponseBase<ProductModel>) => {
                    const data = response.isSuccess && response.data && response.data[0];
                    if (data) {
                        this.toastr.success('Success deleted', 'Success');
                        this.getAllProducts();
                        return;
                    }
                    this.toastr.error('Unknown error occured on delete.', 'Error');
                });
            }
        });
    }

    uploadProductList(): void {
        this.matDialog.open(FileUploadDialogComponent, {
            height: '650px',
            width: '600px'
        }).afterClosed().subscribe((dialogResult) => {
            if (dialogResult === 'success') {
                
            }
        });
    }
}
