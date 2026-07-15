using Chromassist.Core.Services;

namespace Chromassist.Core.Infrastructure;

public sealed class DeferredUpdateCheckService : IUpdateCheckService
{
    public Task<UpdateCheckResult> CheckAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new UpdateCheckResult(
            false,
            false,
            "온라인 최신 version 확인은 향후 release manifest 서명 검증과 함께 추가됩니다.",
            null));
    }
}
