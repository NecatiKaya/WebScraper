export class ServerPagingRequest {
    sortKey!: string;
    sortDirection: 'asc' | 'desc' = 'asc';
    pageIndex: number = 0;
    pageSize: number = 50;
}
