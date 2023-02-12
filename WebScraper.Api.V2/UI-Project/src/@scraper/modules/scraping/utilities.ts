export function shortenText(value: string, length: number): string {
    if (!value) {
        return '';
    }

    const newValue = value.substring(0, length);
    return newValue;
}
