using System.ComponentModel.DataAnnotations;

namespace SampleProject
{
    public enum MyEnum
    {
        [Required]
        One,
        Two,
        ///<summary>
        /// Test
        ///</summary>
        Three
    }
}
