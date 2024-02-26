using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using SampleProject;

namespace Sample
{
    public class MyClass<TType> : BaseClass, IMyClass, IMyTypedClass<string> where TType : class
    {
        private string _fullProperty;

        [Required]
        /// <summary>
        /// Test Comments
        /// </summary>
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
        public int MyProperty { get; set; }

        public async Task<int> GetNewIdAsync<TNewType>(string name,
                                                    string address,
                                                    string city,
                                                    string state) where TNewType : class
        {
            Console.WriteLine("starting");
            var coll = new List<string>();
            if (1 == 1)
            {
                foreach (var item in coll)
                {
                }
            }
            Console.WriteLine("ending");
            return 1;
        }
        public Address MethodLambdaMultiLine() => new Address
        {
            Name = "",
            City = "",
            Street = ""
        };
        public int MyMethodLamda() => 5;
        protected async Task<int> GetProtectedAsync<TNewType>(string name,
                                                    string address) where TNewType : class
        {
            Console.WriteLine("protected");
            var coll = new List<string>();
            return 1;
        }
    }
}
