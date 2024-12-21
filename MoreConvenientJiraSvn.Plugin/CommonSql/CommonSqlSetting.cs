using LiteDB;

namespace MoreConvenientJiraSvn.Plugin.CommonSql
{
    public record CommonSqlSetting
    {
        /// <summary>
        /// Global setting
        /// </summary>
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.Empty;

    }

}
