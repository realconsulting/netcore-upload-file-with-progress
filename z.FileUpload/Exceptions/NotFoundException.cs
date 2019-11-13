using System;
using System.Collections.Generic;
using System.Text;

namespace z.FileUpload.Exceptions
{
    public class NotFoundException : ApiException
    {
        public NotFoundException(string msg) : base(404, msg)
        {
        }
    }
}
