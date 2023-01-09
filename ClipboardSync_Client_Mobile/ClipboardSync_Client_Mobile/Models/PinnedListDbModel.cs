using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ClipboardSync_Client_Mobile.Models
{
    public class PinnedListDbModel
    {
        // https://learn.microsoft.com/en-us/xamarin/get-started/quickstarts/database?pivots=windows
        [PrimaryKey]
        public int ID { get; set; }
        public List<string> PinnedList { get; set; }

        public PinnedListDbModel()
        {
            ID = 1;
            PinnedList = new List<string>();
        }

        public PinnedListDbModel(List<string> list)
        {
            ID = 1;
            PinnedList = list;
        }
    }
}
