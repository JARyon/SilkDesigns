using Microsoft.AspNetCore.Mvc.Rendering;

namespace ADO.Models
{
    public class Size
    {
        public int SizeID { get; set; }
        public string Code { get; set; }

        public Size(string codeVal, int SizeIDVal)
        {
            this.Code = codeVal;    
            this.SizeID = SizeIDVal;    
        }
        public Size() { }   
    }
}
