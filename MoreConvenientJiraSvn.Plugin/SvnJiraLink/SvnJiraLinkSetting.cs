using LiteDB;

namespace MoreConvenientJiraSvn.Plugin.SvnJiraLink
{
    public record SvnJiraLinkSetting
    {
        /// <summary>
        /// Global setting, id is zeri
        /// </summary>
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.Empty;

    }
}
