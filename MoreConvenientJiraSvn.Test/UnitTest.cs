using MoreConvenientJiraSvn.Infrastructure;
using System.Threading.Tasks;

namespace MoreConvenientJiraSvn.Test
{
    public class Tests
    {
        public HtmlConvert htmlConvert;

        [SetUp]
        public void Setup()
        {
            htmlConvert = new();
        }

        [Test]
        public async Task TryConvert()
        {
            string htmlString = @"<div class=""jira-dialog-content"">
    <div class=""dialog-title hidden"">Operation Title [Issue-Key]</div>

    <form action=""/secure/CommentAssignIssue.jspa?atl_token=xxxxxxxxxxxxxxxxxxx"" class=""aui dnd-attachment-support""
        enctype=""multipart/form-data"" id=""issue-workflow-transition"" method=""post"">
        <div class=""form-body"">

            <h2 class=""dialog-title"">

                Dialog Title

            </h2>

            <div class=""hidden"">
                <input name=""action"" type=""hidden"" value=""123"" />
            </div>

            <div class=""hidden"">
                <input name=""id"" type=""hidden"" value=""123"" />
            </div>

            <div class=""hidden"">
                <input name=""viewIssueKey"" type=""hidden"" />
            </div>

            <div class=""hidden"">
                <input name=""formToken"" type=""hidden"" value=""a12343243543"" />
            </div>

            <div class=""field-group"">
                <label for=""summary"">Summary <span class=""visually-hidden"">Required</span><span
                        class=""aui-icon icon-required"" aria-hidden=""true""></span> </label> <input
                    class=""text long-field"" id=""summary"" name=""summary"" type=""text"" value=""Issue Summary"" />
            </div>

            <div class=""field-group aui-field-componentspicker frother-control-renderer"">
                <label for=""components"">Component</label>
                <div class=""ajs-multi-select-placeholder textarea long-field""></div> <select class=""select hidden""
                    id=""components"" multiple=""multiple"" name=""components"" size=""5"" aria-hidden=""true""
                    aria-labelledby=""components-textarea"" data-remove-null-options=""true"" data-submit-input-val=""true""
                    data-input-text="""" data-create-permission=""false"">
                    <option value=""-1"">
                        UnKnow
                    </option>
                    <option title=""AAA "" value=""111"">
                        AAA
                    </option>
                    <option title=""BBB "" value=""222"">
                        BBB
                    </option>
                    <option title=""CCC "" value=""333"">
                        CCC
                    </option>
                </select>
                <div class=""description"">Input Tips</div>
            </div>

            <div class=""field-group"">
                <label for=""customfield_11111"">Field Title</label>

                <textarea class=""textarea long-field"" cols=""40"" id=""customfield_11111"" name=""customfield_11111""
                    rows=""5"">
                  Content
                </textarea>

            </div>

            <div class=""field-group"">
                <label for=""customfield_22222"">Field Title</label> <select class=""select cf-select""
                    name=""customfield_22222"" id=""customfield_22222"">
                    <option value=""-1"">Unknow</option>
                    <option value=""xxxx"">XX</option>
                    <option value=""yyyy"">YY</option>
                    <option selected=""selected"" value=""zzzz"">XX&amp;YY </option>
                </select>
                <div class=""description"">
                    <p>Description</p>
                </div>
            </div>


            <div class=""field-group"">
                <label for=""description"">Field Title</label>

                <div class=""jira-wikifield"" field-id=""description"" renderer-type=""atlassian-wiki-renderer""
                    issue-key=""IssueKey"">
                    <div class=""wiki-edit"">
                        <div id=""description-wiki-edit"" class=""wiki-edit-content"">
                            <textarea class=""textarea long-field wiki-textfield long-field mentionable"" id=""description""
                                name=""description"" rows=""12"" wrap=""virtual"" data-projectkey=""Projectkey""
                                data-issuekey=""Issue Key"">Raw Issue Description </textarea>
                            <div class=""rte-container""><rich-editor contenteditable=""true"" data-issue-key=""Issue Key""
                                    data-content-present=""true"" tabindex=""-1"">
                                    Issue Description
                                </rich-editor></div>
                            <div class=""content-inner"">
                            </div>
                        </div>
                    </div>
                    <div class=""field-tools"">
                        <dl id=""wiki-prefs"" class=""wiki-js-prefs"" style=""display:none"">
                            <dt>trigger</dt>
                            <dd>description-preview_link</dd>
                            <dt>fieldId</dt>
                            <dd>description</dd>
                            <dt>fieldName</dt>
                            <dd>Descrition</dd>
                            <dt>rendererType</dt>
                            <dd>atlassian-wiki-renderer</dd>
                            <dt>issueKey</dt>
                            <dd>Issue Key</dd>
                        </dl>
                        <button class=""jira-icon-button fullscreen wiki-preview"" id=""description-preview_link""
                            type=""button"">
                            <span class=""aui-icon wiki-renderer-icon"">Preview description</span>
                        </button>
                        <a class=""help-lnk"" id=""viewHelp"" href=""/secure/WikiRendererHelpAction.jspa?section=texteffects""
                            title=""Getwikihelp"" aria-label=""Getwikihelp"" data-helplink=""local""><span
                                class=""aui-icon aui-icon-small aui-iconfont-help""></span></a>
                    </div>
                </div>
                <div class=""save-options wiki-button-bar"">
                </div>

            </div>


            <div class=""field-group aui-field-datepicker"">
                <label for=""customfield_33333"">Field Title</label> <input class=""text medium-field datepicker-input""
                    id=""customfield_33333"" name=""customfield_33333"" type=""text"" value="""" />
                <a href=""#"" id=""customfield_33333-trigger"" title=""Select Date"">
                    <span class=""icon-default aui-icon aui-icon-small aui-iconfont-calendar"">Pick Date</span>
                </a>

                <fieldset class=""hidden datepicker-params"">
                    <input title=""firstDay"" type=""hidden"" value=""0"" /> <input title=""inputField"" type=""hidden""
                        value=""customfield_33333"" /> <input title=""button"" type=""hidden""
                        value=""customfield_33333-trigger"" /> <input title=""align"" type=""hidden"" value=""Br"" /> <input
                        title=""singleClick"" type=""hidden"" value=""true"" />
                    <input title=""date"" type=""hidden"" value=""2025/01/01 11:22:33"" />
                    <input title=""todayDate"" type=""hidden"" value=""2025/01/01 11:22:33"" />
                    <input title=""useISO8601WeekNumbers"" type=""hidden"" value=""true"" /> <input title=""ifFormat""
                        type=""hidden"" value=""%Y-%m-%d"" />
                </fieldset>
            </div>


            <div class=""field-group aui-field-wikiedit comment-input"">
                <label for=""comment"">Comment</label>

                <div class=""jira-wikifield"" field-id=""comment"" renderer-type=""atlassian-wiki-renderer""
                    issue-key=""IssueKey"">
                    <div class=""wiki-edit"">
                        <div id=""comment-wiki-edit"" class=""wiki-edit-content"">
                            <textarea class=""textarea long-field wiki-textfield mentionable"" cols=""60"" id=""comment""
                                name=""comment"" rows=""10"" wrap=""virtual"" data-projectkey=""ProjectKey""
                                data-issuekey=""IssueKey""></textarea>
                            <div class=""rte-container""><rich-editor contenteditable=""true"" data-issue-key=""IssueKey""
                                    data-content-present=""true"" tabindex=""-1""></rich-editor></div>
                            <div class=""content-inner"">
                            </div>
                        </div>
                    </div>
                    <div class=""field-tools"">
                        <dl id=""wiki-prefs"" class=""wiki-js-prefs"" style=""display:none"">
                            <dt>trigger</dt>
                            <dd>comment-preview_link</dd>
                            <dt>fieldId</dt>
                            <dd>comment</dd>
                            <dt>fieldName</dt>
                            <dd>Comment</dd>
                            <dt>rendererType</dt>
                            <dd>atlassian-wiki-renderer</dd>
                            <dt>issueKey</dt>
                            <dd>IssueKey</dd>
                        </dl>
                        <button class=""jira-icon-button fullscreen wiki-preview"" id=""comment-preview_link""
                            type=""button"">
                            <span class=""aui-icon wiki-renderer-icon"">Preview comment</span>
                        </button>
                        <a class=""help-lnk"" id=""viewHelp"" href=""/secure/WikiRendererHelpAction.jspa?section=texteffects""
                            title=""Getwikihelp"" aria-label=""Getwikihelp"" data-helplink=""local""><span
                                class=""aui-icon aui-icon-small aui-iconfont-help""></span></a>
                    </div>
                </div>
                <div class=""save-options wiki-button-bar"">
                    <div class=""security-level"">
                        <fieldset class=""hidden parameters"">
                            <input type=""hidden"" title=""securityLevelViewableByAll"" value=""All-view"">
                            <input type=""hidden"" title=""securityLevelViewableRestrictedTo"" value=""Only <span class=""
                                redText"">{0}</span>"">
                        </fieldset>
                        <a class=""drop"" href=""#"">
                            <span class=""security-level-drop-icon aui-icon aui-icon-small  aui-iconfont-unlocked"">
                                The comment will view to all user
                            </span>
                            <span class=""icon drop-menu""></span>
                        </a>
                        <select name=""commentLevel"" id=""commentLevel"" data-enable-default=""true""
                            data-apply-default=""true"">
                            <option value="""">All user</option>
                            <optgroup label=""ProjectCharacter"">
                                <option value=""role:11111"">Developer</option>
                            </optgroup>
                        </select>
                        <span class=""current-level"">All-view</span>
                        <span class=""default-comment-level"" data-project-id=""10000""></span>
                    </div>
                    <span class=""security-level-inline-error""></span>
                </div>

            </div>


            <div class=""hidden"">
                <input name=""atl_token"" type=""hidden""
                    value=""ATLTOKEN123425234-4324235"" />
            </div>

        </div>

        <div class=""buttons-container form-footer"">
            <div class=""buttons"">

                <input accesskey=""S"" class=""aui-button"" id=""issue-workflow-transition-submit"" name=""Transition""
                    title=""Tap Alt+S to submit this form"" type=""submit"" value=""OperationTitle"" />


                <a accesskey=""`"" class=""aui-button aui-button-link cancel"" href=""/browse/IssueKey""
                    id=""issue-workflow-transition-cancel"" title=""TapAlt+`Cancel"">Cancel</a>

            </div>

        </div>

    </form> <!-- // .aui.dnd-attachment-support #issue-workflow-transition -->

</div>";

            var result = await htmlConvert.ConvertHtmlToJiraFieldsAsync(htmlString, CancellationToken.None);

            Assert.Equals(result.Count, 6);
        }

    }
}
