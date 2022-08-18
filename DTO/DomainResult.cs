using System;

namespace SmartCardReader.DTO
{
    public class DomainResult<T> where T : class
    {
        public DomainResult(T value, bool success)
        {
            isSuccess = success;
            result = value;
        }
        public DomainResult(T value)
        {
            isSuccess = true;
            result = value;
        }

        public DateTime occuredIn => DateTime.Now;
        public bool isSuccess { get; set; }
        public T result { get; set; }
    }
}
