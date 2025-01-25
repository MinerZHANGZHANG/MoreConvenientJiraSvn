using LiteDB;
using MoreConvenientJiraSvn.Core.Service;
using System.Text.Json;

namespace MoreConvenientJiraSvn.Core.Model
{
    public record JiraInfo
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string JiraId { get; set; } = string.Empty;

        [BsonIgnore]
        public string? JsonResult
        {
            get
            {
                return _jsonResult;
            }
            set
            {
                _jsonResult = value;
                this.ConvertJsonToProperty();
            }
        }
        private string? _jsonResult;

        #region Common Property

        public string? ParentJiraId { get; set; }
        public string? ParentSummary { get; set; }

        public string? SelfUrl { get; set; }
        public string? CreatorName { get; set; }
        public string? Summary { get; set; }
        public string? StatusName { get; set; }
        public string? PriorityName { get; set; }
        public string? IssueTypeName { get; set; }
        public IEnumerable<string>? FixVersionsName { get; set; }
        public string FixVersionNameText
        {
            get
            {
                if (FixVersionsName != null && FixVersionsName.Any())
                {
                    return string.Join(",", FixVersionsName);
                }
                return string.Empty;
            }
        }
        public string? ResolutionName { get; set; }
        public string? Descrpition { get; set; }

        #endregion

        #region Custom Property
        /// <summary>
        /// in customfield_12813
        /// </summary>
        public string? DeveloperLeaderName { get; set; }

        /// <summary>
        /// in customfield_10514 
        /// </summary>
        public IEnumerable<string>? DeveloperNames { get; set; }

        public string DeveloperNamesString
        {
            get
            {
                if (DeveloperNames != null && DeveloperNames.Any())
                {
                    return string.Join(",", DeveloperNames);
                }
                return string.Empty;
            }
        }
        #endregion

        private bool ConvertJsonToProperty()
        {
            if (!string.IsNullOrWhiteSpace(this._jsonResult))
            {
                try
                {
                    using JsonDocument doc = JsonDocument.Parse(this._jsonResult);
                    this.SelfUrl = doc.RootElement.GetProperty("self").GetString();
                    this.JiraId = doc.RootElement.GetProperty("key").GetString()
                        ?? throw new KeyNotFoundException("can`t get jira id");

                    var fieldsElement = doc.RootElement.GetProperty("fields");

                    // some issue has parent issue
                    if (fieldsElement.TryGetProperty("parent", out var parentElement))
                    {
                        this.ParentJiraId = parentElement
                            .GetProperty("key")
                            .GetString();

                        this.ParentSummary = parentElement
                            .GetProperty("fields")
                            .GetProperty("summary")
                            .GetString();
                    }

                    this.FixVersionsName = fieldsElement.GetProperty("fixVersions")
                        .EnumerateArray()
                        .Select(e => e.GetProperty("name").ToString())
                        .ToArray();

                    if (fieldsElement.TryGetProperty("resolution", out var resolutionElement)
                        && resolutionElement.ValueKind == JsonValueKind.Object)
                    {
                        this.ResolutionName = resolutionElement.GetProperty("name").GetString();
                    }

                    this.StatusName = fieldsElement.GetProperty("status")
                        .GetProperty("name")
                        .ToString();

                    this.CreatorName = fieldsElement.GetProperty("creator")
                        .GetProperty("name")
                        .ToString();

                    this.IssueTypeName = fieldsElement.GetProperty("issuetype")
                        .GetProperty("name")
                        .ToString();

                    this.PriorityName = fieldsElement.GetProperty("priority")
                        .GetProperty("name")
                        .ToString();

                    this.Summary = fieldsElement.GetProperty("summary").ToString();

                    this.Descrpition = fieldsElement.GetProperty("description")
                        .GetString();


                    // Custom Field
                    this.DeveloperLeaderName = fieldsElement.GetProperty("customfield_12813")
                        .GetProperty("child")
                        .GetProperty("value")
                        .ToString();

                    this.DeveloperNames = fieldsElement.GetProperty("customfield_10514")
                        .EnumerateArray()
                        .Select(e => e.GetProperty("name").ToString())
                        .ToArray();

                    return true;
                }
                catch
                {

                    return false;
                }
            }
            return false;
        }

        public static ObjectId GetKey(string jiraId)
        {
            // Temporary solution
            return DataService.ConvertToObjectId(jiraId);
        }
        public JiraInfo()
        {
            JsonResult = string.Empty;
            Id = ObjectId.Empty;
        }

        public JiraInfo(string json)
        {
            JsonResult = json;
            Id = GetKey(JiraId);
        }

        public JiraInfo()
        {
            Id = GetKey(JiraId);
        }
    }
}
