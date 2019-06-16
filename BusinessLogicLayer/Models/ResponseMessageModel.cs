using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Models
{
    public class ResponseMessageModel
    {
        public object Message { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static string CreateResponseMessage(dynamic message, string description)
        {
            return new ResponseMessageModel()
            {
                Message = message,
                Description = description
            }.ToString();
        }
    }
}
