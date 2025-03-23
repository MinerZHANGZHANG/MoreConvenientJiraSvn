using MoreConvenientJiraSvn.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MoreConvenientJiraSvn.App.Styles
{
    public class JiraFieldTemplateSelector : DataTemplateSelector
    {
        public required DataTemplate JiraTextTemplate { get; set; }
        public required DataTemplate JiraSelectTemplate { get; set; }
        public required DataTemplate JiraDateTemplate { get; set; }
        public required DataTemplate JiraMultiSelectTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is JiraTextField)
            {
                return JiraTextTemplate;
            }
            if (item is JiraSelectField)
            {
                return JiraSelectTemplate;
            }
            if(item is JiraDateField)
            {
                return JiraDateTemplate;
            }
            if(item is JiraMultiSelectField)
            {
                return JiraMultiSelectTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
