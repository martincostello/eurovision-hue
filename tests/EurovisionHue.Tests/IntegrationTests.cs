// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.EurovisionHue;

[Collection<BrowserFixture>]
public abstract class IntegrationTests(
    BrowserFixture browser,
    ITestOutputHelper outputHelper) : IDisposable
{
    private bool _disposed;

    ~IntegrationTests()
    {
        Dispose(false);
    }

    protected AppFixture Application { get; } = new(outputHelper);

    protected BrowserFixture Browser { get; } = browser;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            Application?.Dispose();
            _disposed = true;
        }
    }
}
