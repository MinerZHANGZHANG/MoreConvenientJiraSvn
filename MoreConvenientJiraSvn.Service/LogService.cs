using MoreConvenientJiraSvn.Core.Interfaces;

namespace MoreConvenientJiraSvn.Service
{
    public class LogService(IRepository repository, NotificationService notificationService, bool isDebugMode = true)
    {
        private readonly IRepository _repository = repository;
        private readonly NotificationService _notificationService = notificationService;
        private readonly bool _isDebugMode = isDebugMode;

        public void Debug(string message)
        {
            if (_isDebugMode)
            {
                _notificationService.DebugMessage(message);
            }
        }
    }
}
