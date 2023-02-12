import { ScraperVisit } from './scraper-visit.model';

export interface ProductModel {
    id: number;
    name: string;
    barcode: string;
    asin: string;
    trendyolUrl: string;
    amazonUrl: string;
    requestedPriceDiffrenceWithAmount?: number;
    RequestedPriceDiffrenceWithPercentage?: number;
    isDeleted: boolean;
    scraperVisits: ScraperVisit[];
}
