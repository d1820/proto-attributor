using System.Runtime.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using SampleProject;

namespace Sample
{
    [DataContract]
    public class DataMyClass
    {
        private string _fullProperty;

        [Required]
        /// <summary>
        /// Test Comments
        /// </summary>
        [DataMember(Order = 1)]
        public int MyPropertyLamda => 5;

        [Required]
        [DataMember(Order = 2)]
        public string FullProperty
        {
            get => _fullProperty;
            set => _fullProperty = value;
        }

        [DataMember(Order = 3)]
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
        [DataMember(Order = 4)]
        public int MyPropertyA { get; set; }

        [DataMember(Order = 5)]
        public int MyPropertyB { get; set; }

        [DataMember(Order = 6)]
        public int MyPropertyC { get; set; }

        [DataMember(Order = 7)]
        public int MyPropertyD { get; set; }

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
