using System;
using System.ComponentModel.DataAnnotations;

namespace Trail365.ViewModels
{
    public class LayoutViewModel
    {
        public string Key { get; set; }

        [Display(Name = "Label for text")]
        public string TextValue { get; set; } = "Dummy Text Value";

        [Display(Name = "Label for multiline")]
        public string MultiLineTextValue { get; set; } = "Line1" + Environment.NewLine + "Line2" + Environment.NewLine + "Line3";
    }
}
