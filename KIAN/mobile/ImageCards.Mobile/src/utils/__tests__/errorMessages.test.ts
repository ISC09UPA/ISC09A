import { strings } from '../../constants/strings';
import { ApiError, ConfigurationError, NetworkError } from '../../services/api/errors';
import { getErrorMessage } from '../errorMessages';

describe('getErrorMessage', () => {
  it.each([
    [new ConfigurationError('missing'), strings.errors.configuration],
    [new NetworkError('timeout'), strings.errors.timeout],
    [new NetworkError('unreachable'), strings.errors.unreachable],
    [new ApiError(401, null), strings.errors.unauthorized],
    [new ApiError(404, { title: 'Resource not found' }), strings.errors.notFound],
    [new ApiError(503, null), strings.errors.unavailable],
    [new ApiError(500, { title: 'An unexpected error occurred' }), strings.errors.server],
    [new ApiError(400, null), strings.errors.badRequest],
    [new Error('boom'), strings.errors.unexpected],
    ['not an error', strings.errors.unexpected],
  ])('maps %p to a user-facing message', (error, expected) => {
    expect(getErrorMessage(error)).toBe(expected);
  });

  it('shows the first validation message for 400 responses', () => {
    const error = new ApiError(400, { errors: { image: ['The image exceeds the maximum size.'] } });

    expect(getErrorMessage(error)).toBe('The image exceeds the maximum size.');
  });
});
