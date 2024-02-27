using System.ComponentModel.DataAnnotations;

namespace Sample
{
    public class DataMyClass
    {
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
