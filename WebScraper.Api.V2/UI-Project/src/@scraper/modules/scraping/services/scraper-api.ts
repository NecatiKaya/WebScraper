import { HttpClient, HttpEvent } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ServerPagingRequest } from '@scraper/modules/shared/server-paging-request';
import { ServerResponseBase } from '@scraper/modules/shared/server-response-base';
import { Observable } from 'rxjs';
import { ProductModel } from '../models/product.model';
import { ScraperVisit } from '../models/scraper-visit.model';

@Injectable()
export class ScraperApi {

    private readonly scraperApiUrl = 'https://localhost:7037';
    //private readonly scraperApiUrl = 'http://localhost:5037';

    constructor(private httpClient: HttpClient) {

    }

    public getAllProducts(sortKey: string, sortDirection: string, pageIndex: number = 0, pageSize: number = 50): Observable<ServerResponseBase<ProductModel>> {
        const queryString: string = `?pageIndex=${pageIndex}&pageSize=${pageSize}&sortKey=${sortKey}&sortDirection=${sortDirection}`;
        return this.httpClient.get<ServerResponseBase<ProductModel>>(this.scraperApiUrl + '/products' + queryString);
    }

    public createProduct(request: {
        name: string;
        barcode: string;
        asin: string;
        trendyolUrl: string;
        amazonUrl: string;
        requestedPriceDiffrenceWithAmount: number;
        RequestedPriceDiffrenceWithPercentage: number;

    }): Observable<ServerResponseBase<ProductModel>> {
        return this.httpClient.post<ServerResponseBase<ProductModel>>(this.scraperApiUrl + '/products', request);
    }

    public updateProduct(request: {
        id: number;
        name: string;
        barcode: string;
        asin: string;
        trendyolUrl: string;
        amazonUrl: string;
        requestedPriceDiffrenceWithAmount?: number;
        RequestedPriceDiffrenceWithPercentage?: number;
    }): Observable<ServerResponseBase<ProductModel>> {
        return this.httpClient.patch<ServerResponseBase<ProductModel>>(this.scraperApiUrl + '/products', request);
    }

    public getProductById(id: number): Observable<ServerResponseBase<ProductModel>> {
        return this.httpClient.get<ServerResponseBase<ProductModel>>(this.scraperApiUrl + '/products/' + id);
    }

    public deleteProductById(id: number): Observable<ServerResponseBase<ProductModel>> {
        return this.httpClient.delete<ServerResponseBase<ProductModel>>(this.scraperApiUrl + '/products/' + id);
    }

    public getProductByNameLike(name: string): Observable<ServerResponseBase<ProductModel>> {
        return this.httpClient.get<ServerResponseBase<ProductModel>>(this.scraperApiUrl + '/products/like-search?name=' + name);
    }

    public getVisits(request: ServerPagingRequest): Observable<ServerResponseBase<ScraperVisit>> {
        return this.httpClient.post<ServerResponseBase<ScraperVisit>>(this.scraperApiUrl + '/visits/', request);
    }

    public uploadProductFile(formData: FormData): Observable<HttpEvent<any>> {
        return this.httpClient.post(this.scraperApiUrl + '/products/upload/', formData, {
            reportProgress: true,
            observe: 'events'
        });
    }
}
