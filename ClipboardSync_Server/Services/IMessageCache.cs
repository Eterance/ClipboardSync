using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardSync_Server.Services
{
    public interface IMessageCache
    {
        void Push(string sender, string message);
        int Capacity
        { get; set; }

    }
}
