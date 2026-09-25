import { apiRequest, buildUrl } from '../apiClient';
import { reviewsApi } from '../reviewsApi';
import { ApiError, ConfigurationError, NetworkError } from '../errors';

function jsonResponse(status: number, body: unknown): Response {
  return {
    ok: status >= 200 && status < 300,
    status,
    json: () => Promise.resolve(body),
  } as Response;
}

describe('apiClient', () => {
  const fetchMock = jest.fn<Promise<Response>, [string, RequestInit]>();

  beforeEach(() => {
    fetchMock.mockReset();
    globalThis.fetch = fetchMock as unknown as typeof fetch;
    process.env.EXPO_PUBLIC_API_URL = 'http://api.test/';
  });

  it('builds URLs from the configured base URL and skips undefined query values', () => {
    expect(buildUrl('http://api.test', 'api/cards', { language: 'es', page: 2, pageSize: undefined })).toBe(
      'http://api.test/api/cards?language=es&page=2',
    );
    expect(buildUrl('http://api.test', '/health')).toBe('http://api.test/health');
    expect(buildUrl('http://api.test', '/x', { q: 'a b&c' })).toBe('http://api.test/x?q=a%20b%26c');
  });

  it('sends GET requests to EXPO_PUBLIC_API_URL and returns the parsed body', async () => {
    fetchMock.mockResolvedValue(jsonResponse(200, { totalDue: 0, cards: [] }));

    const result = await reviewsApi.getDueCards('fr', 10);

    expect(result).toEqual({ totalDue: 0, cards: [] });
    const [url, init] = fetchMock.mock.calls[0]!;
    expect(url).toBe('http://api.test/api/reviews/due?language=fr&limit=10');
    expect(init.method).toBe('GET');
    expect(init.body).toBeUndefined();
  });

  it('sends JSON bodies with the right content type', async () => {
    fetchMock.mockResolvedValue(jsonResponse(200, {}));

    await reviewsApi.submitReview({ cardId: 'card-1', rating: 'Good' });

    const [url, init] = fetchMock.mock.calls[0]!;
    expect(url).toBe('http://api.test/api/reviews');
    expect(init.method).toBe('POST');
    expect(init.body).toBe('{"cardId":"card-1","rating":"Good"}');
    expect(init.headers).toMatchObject({ 'Content-Type': 'application/json' });
  });

  it('throws ApiError with the problem details on error statuses', async () => {
    fetchMock.mockResolvedValue(
      jsonResponse(400, { title: 'Validation failed', status: 400, errors: { rating: ['The rating is required.'] } }),
    );

    const error = await apiRequest('/api/reviews').catch((e: unknown) => e);

    expect(error).toBeInstanceOf(ApiError);
    expect((error as ApiError).status).toBe(400);
    expect((error as ApiError).validationMessages).toEqual(['The rating is required.']);
  });

  it('throws ApiError even when the error body is not JSON', async () => {
    fetchMock.mockResolvedValue({ ok: false, status: 502, json: () => Promise.reject(new SyntaxError('x')) } as Response);

    const error = await apiRequest('/api/cards').catch((e: unknown) => e);

    expect(error).toBeInstanceOf(ApiError);
    expect((error as ApiError).problem).toBeNull();
  });

  it('returns undefined for 204 No Content', async () => {
    fetchMock.mockResolvedValue({ ok: true, status: 204, json: () => Promise.reject(new Error('no body')) } as Response);

    await expect(apiRequest('/api/cards/1', { method: 'DELETE' })).resolves.toBeUndefined();
  });

  it('throws NetworkError when the server cannot be reached', async () => {
    fetchMock.mockRejectedValue(new TypeError('Network request failed'));

    const error = await apiRequest('/api/cards').catch((e: unknown) => e);

    expect(error).toBeInstanceOf(NetworkError);
    expect((error as NetworkError).reason).toBe('unreachable');
  });

  it('throws ConfigurationError without calling fetch when the API URL is missing', async () => {
    process.env.EXPO_PUBLIC_API_URL = '';

    await expect(apiRequest('/api/cards')).rejects.toBeInstanceOf(ConfigurationError);
    expect(fetchMock).not.toHaveBeenCalled();
  });
});
