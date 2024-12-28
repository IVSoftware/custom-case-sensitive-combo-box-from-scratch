using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace custom_case_sensitive_combo_box_from_scratch
{
    public interface ISelectable
    {
        bool IsSelected { get; set; }
        public string Text { get; set; }
    }
}
