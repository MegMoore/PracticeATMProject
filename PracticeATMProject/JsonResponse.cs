using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracticeATMProject
{
    public class JsonResponse
    {
        public int HttpStatusCode { get; set; } = 0;
        public object? DataReturned { get; set; } = null;
    }
}
