using Microsoft.Extensions.Time.Testing;

namespace ImageCards.Api.Tests.TestSupport;

public static class TestClock
{
    public static readonly DateTimeOffset Start = new(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);

    public static FakeTimeProvider Create() => new(Start);
}
