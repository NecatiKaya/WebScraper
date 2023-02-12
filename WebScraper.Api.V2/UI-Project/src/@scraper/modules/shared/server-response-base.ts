export class ServerResponseBase<TData>{
    isSuccess: boolean = true;
    totalRowCount: number = 0;
    data: TData[] = [];
}
