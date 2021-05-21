
using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Components;

namespace BlazorCat.Pages
    {
    public class CatMode:ComponentBase
    {
        public string time { get; set; } = DateTime.Now.ToString("D");
       


        
    }

   

    
}