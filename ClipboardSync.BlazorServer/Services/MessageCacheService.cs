using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardSync.BlazorServer.Services
{
    public class MessageCacheService
    {
        private readonly ILogger _logger = null;
        private int _capacity = 30;
        private Queue<string> _queue = new();

        public int Capacity
        {
            get
            {
                return _capacity;
            }
            set
            {
                _capacity = value;
                // Dequeue any excess messages
                if (value > 0)
                    _queue.EnsureCapacity(Capacity + 1);
                CheckQueueCapacity();
            }
        }

        public MessageCacheService(ILogger<MessageCacheService> logger)
        {
            _logger = logger;
            _queue.EnsureCapacity(Capacity + 1);
        }

        public bool Push(string message)
        {
            if (_queue.Contains(message) != true)
            {
                _queue.Enqueue(message);
                CheckQueueCapacity();
                return true;
            }
            return false;
        }

        private void CheckQueueCapacity()
        {
            if (Capacity > 0 && _queue.Count > Capacity)
                _queue.Dequeue();
        }

        public void Clear()
        {
            _queue.Clear();
        }

        public List<string> GetMessages()
        {
            return _queue.ToList();
        }

    }
}
