using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public enum TestEnum
    {
        One,
        Two,
        // comment
        Three,
        /// <summary>
        /// Test Sumary
        /// </summary>
        Four,
    }
    public class GetViewModel
    {
        public string Hometown { get; set; }
        public string Hometown1 { get; set; }
        public string Hometown2 { get; set; }
        public string Hometown3 { get; set; }
        public string Hometown4 { get; set; }
        public string Hometown5 { get; set; }
        public string Hometown6 { get; set; }
        public string Hometown7 { get; set; }
        public string Hometown8 { get; set; }
        public string Hometown9 { get; set; }
        public string Hometown11 { get; set; }
        public string Hometown22 { get; set; }
       
        public string Hometown33 { get; set; }
        public string Hometown44 { get; set; }
      
        public string Hometown55 { get; set; }

        public string Hometown66 { get; set; }
        public string Hometown77 { get; set; }
        public string Hometown88 { get; set; }
        public string Hometown99 { get; set; }

    }
}
