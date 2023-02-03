using Microsoft.AspNetCore.Mvc.Rendering;

namespace SilkDesign.Models
{
    public class Size
    {
        public string SizeID { get; set; }
        
         public string Code { get; set; }

        public string Description { get; set; }
        public int SortOrder { get; set; }  

        public Size(string codeVal, string SizeIDVal)
        {
            this.Code = codeVal;    
            this.SizeID = SizeIDVal;    
        }
        public Size() { }   
    }
}
