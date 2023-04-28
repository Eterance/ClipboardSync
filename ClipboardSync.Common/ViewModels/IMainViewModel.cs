using ClipboardSync.Common.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClipboardSync.Common.ViewModels
{
    public interface IMainViewModel
    {
        public ClipboardViewModel SubViewModel { get; set; }
    }
}
