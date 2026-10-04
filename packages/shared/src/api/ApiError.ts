export class ApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
    public readonly raw?: unknown,
  ) {
    super(message);
    this.name = 'ApiError';
  }
}
