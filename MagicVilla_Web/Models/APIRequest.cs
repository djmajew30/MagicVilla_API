using System.Security.AccessControl;
using static MagicVilla_Utility.SD;

namespace MagicVilla_Web.Models
{
    public class APIRequest //when we request from an endpoint
    {
        public ApiType ApiType { get; set; } = ApiType.GET;
        public string Url { get; set; }
        public object Data { get; set; }
    }
}
