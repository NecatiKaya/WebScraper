import { ProductModel } from './product.model';

export interface ScraperVisit {
    id: string;
    productId: number;
    visitDate: Date;
    amazonPreviousPrice?: number;
    amazoncurrentPrice?: number;
    amazonCurrentDiscountAsAmount?: number;
    amazonCurrentDiscountAsPercentage?: number;
    trendyolPreviousPrice?: number;
    trendyolCurrentPrice?: number;
    trendyolCurrentDiscountAsAmount?: number;
    trendyolCurrentDiscountAsPercentage?: number;
    needToNotify: boolean;
    notified: boolean;
}
