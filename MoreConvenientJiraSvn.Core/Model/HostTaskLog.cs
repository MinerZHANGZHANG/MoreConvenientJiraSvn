using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreConvenientJiraSvn.Core.Model
{
    public class HostTaskLog
    {
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.NewObjectId();
        public required DateTime DateTime { get; set; }
        public required string TaskServiceName { get; set; }
        public bool IsSucccess { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
