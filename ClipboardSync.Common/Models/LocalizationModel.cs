﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ClipboardSync.Common.Models
{
    public class LocalizationModel
    {
        public string DisplayLanguage { get; set; }
        public string LanguageID { get; set; }

        public LocalizationModel(string displayLanguage, string languageID)
        {
            DisplayLanguage = displayLanguage;
            LanguageID = languageID;
        }
    }
}
