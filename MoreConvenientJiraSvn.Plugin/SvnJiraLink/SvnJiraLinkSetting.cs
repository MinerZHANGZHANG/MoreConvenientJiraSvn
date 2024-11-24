using LiteDB;

namespace MoreConvenientJiraSvn.Plugin.SvnJiraLink
{
    public record SvnJiraLinkSetting
    {
        /// <summary>
        /// Global setting
        /// </summary>
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.Empty;

    }

}
