// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace PulsarAdmin.Models
{
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public partial class BundlesData
    {
        /// <summary>
        /// Initializes a new instance of the BundlesData class.
        /// </summary>
        public BundlesData()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the BundlesData class.
        /// </summary>
        public BundlesData(IList<string> boundaries = default(IList<string>), int? numBundles = default(int?))
        {
            Boundaries = boundaries;
            NumBundles = numBundles;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "boundaries")]
        public IList<string> Boundaries { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "numBundles")]
        public int? NumBundles { get; set; }

    }
}