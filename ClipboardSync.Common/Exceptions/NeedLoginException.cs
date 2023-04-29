using System;
using System.Collections.Generic;
using System.Text;

namespace ClipboardSync.Common.Exceptions
{
    public class NeedLoginException : ApplicationException
    {
        public string ServerUrl { get; set; }
        public NeedLoginException(string serverUrl)
        {
            ServerUrl = serverUrl;
        }
    }

    public class UserNameOrPasswordWrongException : ApplicationException
    {
    }
}
