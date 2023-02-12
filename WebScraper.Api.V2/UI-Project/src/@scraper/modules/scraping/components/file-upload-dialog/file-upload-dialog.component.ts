import { HttpEvent, HttpEventType, HttpResponse } from '@angular/common/http';
import { Component, OnDestroy, ViewEncapsulation } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { UntilDestroy } from '@ngneat/until-destroy';
import { filter, map, pipe, tap } from 'rxjs';
import { ScraperApi } from '../../services/scraper-api';

@UntilDestroy()
@Component({
    selector: 'file-upload-dialog',
    templateUrl: './file-upload-dialog.component.html',
    styleUrls: ['./file-upload-dialog.component.scss'],
    encapsulation: ViewEncapsulation.None,
})
export class FileUploadDialogComponent implements OnDestroy {

    progress = 0;
    success = true;

    form = new FormGroup({
        selectedFile: new FormControl(null)
    });

    constructor(private scraperApi: ScraperApi) {

    }

    public get selectedFile(): FormControl {
        return this.form.controls.selectedFile;
    }

    upload(): void {
        this.success = false;
        if (!this.form.valid) {
            this.markAllAsDirty(this.form);
            return;
        }

        const request = this.toFormData(this.form.value);
        this.scraperApi.uploadProductFile(request).pipe(
            this.uploadProgress(progress => (this.progress = progress)),
            this.toResponseBody()
        ).subscribe(res => {
            debugger;
            this.progress = 0;
            this.success = true;
            this.form.reset();
        });
    }

    ngOnDestroy(): void {

    }

    markAllAsDirty(form: FormGroup): void {
        for (const control of Object.keys(form.controls)) {
            form.controls[control].markAsDirty();
        }
    }

    toFormData<T>(formValue: T): FormData {
        const formData = new FormData();

        for (const key of Object.keys(formValue)) {
            const value = formValue[key];
            formData.append(key, value);
        }

        return formData;
    }

    // eslint-disable-next-line @typescript-eslint/explicit-function-return-type
    uploadProgress<T>(cb: (progress: number) => void) {
        return tap((event: HttpEvent<T>) => {
            if (event.type === HttpEventType.UploadProgress) {
                cb(Math.round((100 * event.loaded) / event.total));
            }
        });
    }

    // eslint-disable-next-line @typescript-eslint/explicit-function-return-type
    toResponseBody<T>() {
        return pipe(
            filter((event: HttpEvent<T>) => event.type === HttpEventType.Response),
            map((res: HttpResponse<T>) => res.body)
        );
    }

    requiredFileType(type: string) {
        // eslint-disable-next-line @typescript-eslint/explicit-function-return-type, prefer-arrow/prefer-arrow-functions
        return function (control: FormControl) {
            const file = control.value;
            if (file) {
                const extension = file.name.split('.')[1].toLowerCase();
                if (type.toLowerCase() !== extension.toLowerCase()) {
                    return {
                        requiredFileType: true
                    };
                }

                return null;
            }

            return null;
        };
    }
}
