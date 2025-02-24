using MoreConvenientJiraSvn.Core.Models;

namespace MoreConvenientJiraSvn.Core.Interfaces;

public interface ISubversionClient : IDisposable
{
    bool InitSvnClient(SvnConfig config);

    Task<List<SvnLog>> GetSvnLogAsync(string path, DateTime beginTime, DateTime endTime, int maxNumber = 200, bool isNeedExtractJiraId = false, CancellationToken cancellationToken = default);

    Task<List<SvnLog>> GetSvnLogAsync(string path, long beginRevision, long endRevision, int maxNumber = 200, bool isNeedExtractJiraId = false, CancellationToken cancellationToken = default);

}
