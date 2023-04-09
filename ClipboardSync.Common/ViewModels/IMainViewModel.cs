using ClipboardSync.Common.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClipboardSync.Common.ViewModels
{
    public interface IMainViewModel
    {
        public ClipboardManagementViewModel SubViewModel { get; set; }
    }
}
