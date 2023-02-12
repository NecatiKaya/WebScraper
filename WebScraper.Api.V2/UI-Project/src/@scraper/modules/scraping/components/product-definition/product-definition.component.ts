import { Component, Inject, OnDestroy, OnInit, ViewEncapsulation } from '@angular/core';
import { FormBuilder, FormControl, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { ServerResponseBase } from '@scraper/modules/shared/server-response-base';
import { ToastrService } from 'ngx-toastr';
import { catchError, EMPTY, filter, iif, Observable, of, take } from 'rxjs';
import { ProductModel } from '../../models/product.model';
import { ScraperApi } from '../../services/scraper-api';

@UntilDestroy()
@Component({
    selector: 'product-definition',
    templateUrl: './product-definition.component.html',
    styleUrls: ['./product-definition.component.scss'],
    encapsulation: ViewEncapsulation.None,
})
export class ProductDefinitionComponent implements OnInit, OnDestroy {

    productForm = this.fb.group({
        id: [0],
        name: ['', [Validators.required, Validators.minLength(5)]],
        barcode: ['', [Validators.required, Validators.minLength(5)]],
        asin: ['', [Validators.required, Validators.minLength(5)]],
        trendyolUrl: ['', [Validators.required, Validators.minLength(5)]],
        amazonUrl: ['', [Validators.required, Validators.minLength(5)]],
        requestedPriceDiffrenceWithAmount: [null],
        RequestedPriceDiffrenceWithPercentage: [null],
    });

    entityId!: number;

    constructor(private fb: FormBuilder, private toastrService: ToastrService, private scraperApi: ScraperApi,
        public dialogRef: MatDialogRef<ProductDefinitionComponent>,
        @Inject(MAT_DIALOG_DATA) public data: any) {
        this.entityId = data?.entityId;
    }

    public get idControl(): FormControl {
        return this.productForm.controls['id'] as FormControl;
    }

    public get nameControl(): FormControl {
        return this.productForm.controls['name'] as FormControl;
    }

    public get barcodeControl(): FormControl {
        return this.productForm.controls['barcode'] as FormControl;
    }

    public get asinControl(): FormControl {
        return this.productForm.controls['asin'] as FormControl;
    }

    public get trendyolUrlControl(): FormControl {
        return this.productForm.controls['trendyolUrl'] as FormControl;
    }

    public get amazonUrlControl(): FormControl {
        return this.productForm.controls['amazonUrl'] as FormControl;
    }

    public get requestedPriceDiffrenceWithAmountControl(): FormControl {
        return this.productForm.controls['requestedPriceDiffrenceWithAmount'] as FormControl;
    }

    public get requestedPriceDiffrenceWithPercentControl(): FormControl {
        return this.productForm.controls['RequestedPriceDiffrenceWithPercentage'] as FormControl;
    }

    public get isUpdate(): boolean {
        return !!this.entityId;
    }

    ngOnInit(): void {
        if (this.isUpdate) {
            this.scraperApi.getProductById(this.entityId).pipe(
                untilDestroyed(this),
                take(1),
                catchError((err) => {
                    this.toastrService.error('Error occured during get information of product having id of ' + this.entityId, 'Error');
                    console.log(err);
                    return EMPTY;
                })
            ).subscribe((response: ServerResponseBase<ProductModel>) => {
                const data = response.isSuccess && response.data && response.data[0];
                if (data) {
                    this.productForm.setValue({
                        asin: data.asin,
                        barcode: data.barcode,
                        id: data.id,
                        name: data.name,
                        amazonUrl: data.amazonUrl,
                        requestedPriceDiffrenceWithAmount: data.requestedPriceDiffrenceWithAmount,
                        RequestedPriceDiffrenceWithPercentage: data.RequestedPriceDiffrenceWithPercentage,
                        trendyolUrl: data.trendyolUrl
                    });
                }
                else {
                    this.toastrService.error('Can get product information with id : ' + this.entityId, 'Error');
                    return;
                }
            });
        }
    }

    ngOnDestroy(): void {

    }

    getError(formControl: FormControl): string {
        if (formControl && formControl.errors) {
            const errorList: string[] = [];
            Object.keys(formControl.errors).forEach((key: string) => {
                errorList.push(key + ': ' + (formControl.errors && formControl.errors[key]));
            });

            const str: string = errorList.join(' ');
            return str;
        }

        return '';
    }

    onSaveClick(): void {
        if (this.productForm.invalid) {
            this.toastrService.error('Can not save. Please fix errors on the form', 'Error');
            return;
        }

        const request = {
            id: this.entityId,
            asin: this.asinControl.value,
            barcode: this.barcodeControl.value,
            name: this.nameControl.value,
            trendyolUrl: this.trendyolUrlControl.value,
            amazonUrl: this.amazonUrlControl.value,
            requestedPriceDiffrenceWithAmount: !this.requestedPriceDiffrenceWithAmountControl.value ? null : this.requestedPriceDiffrenceWithAmountControl.value,
            RequestedPriceDiffrenceWithPercentage: !this.requestedPriceDiffrenceWithPercentControl.value ? null : this.requestedPriceDiffrenceWithPercentControl.value
        };
        const updateRequest = this.scraperApi.updateProduct(request);
        const createRequest = this.scraperApi.createProduct(request);

        iif(() => !!this.entityId, updateRequest, createRequest)
            .pipe(
                untilDestroyed(this),
                take(1),
                catchError((err) => {
                    this.toastrService.error('Error occured on save. Check console for error.', 'Error');
                    console.log(err);
                    return EMPTY;
                }))
            .subscribe((response: ServerResponseBase<ProductModel>) => {
                if (response.isSuccess && response.data) {
                    this.toastrService.success('Succesfully saved.', 'Sucess');
                    this.dialogRef.close('success');
                    return;
                }

                this.toastrService.error('Unknown error occured on save.', 'Error');
            });
    }
}
