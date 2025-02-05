using MoreConvenientJiraSvn.Core.Interfaces;
using MoreConvenientJiraSvn.Core.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreConvenientJiraSvn.Service
{
    public class LogService(IRepository repository, NotificationService notificationService, bool isDebugMode = false)
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
