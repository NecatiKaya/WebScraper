/* eslint-disable @typescript-eslint/ban-types */
import { Component, ElementRef, HostListener, Input, OnDestroy, ViewEncapsulation } from '@angular/core';
import { AbstractControl, ControlValueAccessor, FormBuilder, FormGroup, NG_VALIDATORS, NG_VALUE_ACCESSOR, ValidationErrors, Validator } from '@angular/forms';
import { UntilDestroy } from '@ngneat/until-destroy';

@UntilDestroy()
@Component({
    selector: 'file-upload',
    templateUrl: './file-upload.component.html',
    styleUrls: ['./file-upload.component.scss'],
    encapsulation: ViewEncapsulation.None,
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            multi: true,
            useExisting: FileUploadComponent
        },
        {
            provide: NG_VALIDATORS,
            multi: true,
            useExisting: FileUploadComponent
        }
    ],
})
export class FileUploadComponent implements ControlValueAccessor, OnDestroy, Validator {

    @Input() progress: number | null = null;
    file: File | null = null;

    constructor(private host: ElementRef<HTMLInputElement>, private fb: FormBuilder) {

    }

    // eslint-disable-next-line @typescript-eslint/explicit-function-return-type
    @HostListener('change', ['$event.target.files']) emitFiles(event: FileList) {
        const file = event && event.item(0);
        this.file = file;
        this.onChanged(file);
    }

    onChanged: any = () => { };
    onTouched: any = () => { };

    writeValue(value: null): void {
        // clear file input
        this.host.nativeElement.value = '';
        this.file = null;
    }

    registerOnChange(fn: any): void {
        this.onChanged = fn;
    }

    registerOnTouched(fn: any): void {
        this.onTouched = fn;
    }

    validate(control: AbstractControl<any, any>): ValidationErrors | null {
        const validationResult: ValidationErrors = {

        };

        if (!this.file) {
            validationResult['required'] = 'File is not selected.';
        }

        return validationResult;
    }

    ngOnDestroy(): void {

    }
}
