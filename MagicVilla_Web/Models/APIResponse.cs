﻿using System.Net;

namespace MagicVilla_Web.Models
{
    public class APIResponse
    {
        //all responses can use all of these
        //i.e. 201, 204, 404, etc
        public HttpStatusCode StatusCode { get; set; }
        public bool IsSuccess { get; set; } = true;
        public List<string> ErrorMessages { get; set; }

        public object Result { get; set; }
    }
}
