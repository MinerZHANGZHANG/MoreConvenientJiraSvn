
namespace MoreConvenientJiraSvn.Core.Enums;

/// <summary>
/// Label where the element at
/// <code>
/// {
///   "id": "101",   ---- Main
///   "key": "exampleKey",   ---- Main
///   "self": "http://localhost:8080/rest/api/3/issue/101",   ---- Main
///   "fields": {
///     "summary": "Example Issue",   ---- Fields
///     "project": {
///       "name": "Test Project"    ---- Fields
///      },
///     "parent": {
///        "status": {
///            "name": "Open"  ---- Parent
///         }
///      }
///    }
/// }
/// </code>
/// </summary>
public enum JsonPositionType
{
    Main,
    Fields,
    Parent,
}
