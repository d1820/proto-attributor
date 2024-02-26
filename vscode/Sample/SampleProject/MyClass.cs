using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using SampleProject;

namespace Sample
{
    public class MyClass
    {
        private string _fullProperty;

        [Required]
        public int MyPropertyLamda => 5;

        [Required]
        public string FullProperty
        {
            get => _fullProperty;
            set => _fullProperty = value;
        }

        public string FullPropertyAlt
        {
            get
            {
                return _fullProperty;
            }
            set
            {
                _fullProperty = value;
            }
        }
        /// <summary>
        /// Test Comments
        /// </summary>
        [Required]
        public int MyPropertyA { get; set; }

        public int MyPropertyB { get; set; }

        public int MyPropertyC { get; set; }

        public int MyPropertyD { get; set; }
    }
}
