using System.Collections.Generic;

namespace Delivery.Contracts.Error
{
    public class ErrorResponse
    {
        public List<ErrorModel> Errors { get; set; } = new List<ErrorModel>();
    }
}