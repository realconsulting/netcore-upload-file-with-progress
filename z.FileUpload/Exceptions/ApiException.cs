using System;
using System.Collections.Generic;
using System.Text;

namespace z.FileUpload.Exceptions
{
    public class ApiException : System.Exception
    {
        private int code;
        public ApiException(int code, string msg) : base(msg)
        {
            this.code = code;
        }
    }
}
